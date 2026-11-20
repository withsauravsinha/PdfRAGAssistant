using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PdfKnowledgeAssistant.Models;
using PdfKnowledgeAssistant.Services;
using PdfKnowledgeAssistant.Services.Agent;

namespace PdfKnowledgeAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly PdfExtractionService _pdfService;
    private readonly ChunkingService _chunkService;
    private readonly IWebHostEnvironment _environment;

    private readonly EmbeddingService _embeddingService;
    private readonly VectorStorageService _vectorStorageService;
    private readonly SemanticSearchService _semanticSearchService;
    private readonly LlmService _llmService;
    private readonly AgentOrchestrator _agentOrchestrator;


    public ChatController(
        PdfExtractionService pdfService,
        ChunkingService chunkService,
        EmbeddingService embeddingService,
        VectorStorageService vectorStorageService,
        SemanticSearchService semanticSearchService,
        LlmService llmService,
        AgentOrchestrator agentOrchestrator,
        IWebHostEnvironment environment)
    {
        _pdfService = pdfService;
        _chunkService = chunkService;
        _embeddingService = embeddingService;
        _environment = environment;
        _vectorStorageService = vectorStorageService;
        _semanticSearchService = semanticSearchService;
        _llmService = llmService;
        _agentOrchestrator = agentOrchestrator;
    }

    //[HttpGet("ask")]
    //public async Task<IActionResult> Ask(string question)
    //{
    //    // 1. Retrieve relevant chunks from PostgreSQL
    //    var chunks = await _semanticSearchService.SearchAsync(question);

    //    if (chunks.Count == 0)
    //        return NotFound("No relevant information found.");

    //    // 2. Send question and retrieved chunks to Ollama
    //    var answer = await _llmService.GenerateAnswerAsync(
    //        question,
    //        chunks
    //    );

    //    // 3. Return final answer
    //    return Ok(new
    //    {
    //        Question = question,
    //        Answer = answer,
    //        //RetrievedChunks = chunks.Count
    //    });
    //}

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
    [FromBody] ChatRequest request)
    {
        var response = await _agentOrchestrator.ProcessAsync(request);

        return Ok(response);
    }

    [HttpGet("process")]
    public async Task<IActionResult> ProcessPdf()
    {
        var files = new[]
        {
        "superman.pdf",
        "hr_leave_policy_T0-T4.pdf"
    };

        var results = new List<object>();

        foreach (var fileName in files)
        {
            var path = Path.Combine(
                _environment.ContentRootPath,
                "Documents",
                fileName
            );

            if (!System.IO.File.Exists(path))
            {
                results.Add(new
                {
                    FileName = fileName,
                    Status = "File not found"
                });

                continue;
            }

            // Check whether PDF has already been processed
            var alreadyProcessed =
                await _vectorStorageService.IsDocumentProcessedAsync(fileName);

            if (alreadyProcessed)
            {
                results.Add(new
                {
                    FileName = fileName,
                    Status = "Already processed - Skipped"
                });

                continue;
            }

            // 1. Extract PDF text
            var text = _pdfService.ExtractText(path);

            // 2. Create chunks
            var chunks = _chunkService.CreateChunks(text, 100);

            // 3. Generate embeddings
            var embeddings =
                await _embeddingService.GenerateEmbeddingsAsync(chunks);

            // 4. Save chunks and embeddings
            await _vectorStorageService.SaveChunksAsync(
                fileName,
                chunks,
                embeddings
            );

            results.Add(new
            {
                FileName = fileName,
                Status = "Processed successfully",
                TotalChunks = chunks.Count,
                TotalEmbeddings = embeddings.Length,
                VectorDimensions = embeddings.Length > 0
                    ? embeddings[0].Length
                    : 0
            });
        }

        return Ok(results);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string question)
    {
        var results = await _semanticSearchService.SearchAsync(question);

        return Ok(results);
    }
}