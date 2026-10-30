using Microsoft.AspNetCore.Mvc;
using PdfKnowledgeAssistant.Services;

namespace PdfKnowledgeAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly PdfExtractionService _pdfService;
    private readonly ChunkingService _chunkService;
    private readonly IWebHostEnvironment _environment;

    private readonly EmbeddingService _embeddingService;
    private readonly VectorStorageService _vectorStorageService;
    private readonly SemanticSearchService _semanticSearchService;
    private readonly LlmService _llmService;


    public PdfController(
        PdfExtractionService pdfService,
        ChunkingService chunkService,
        EmbeddingService embeddingService,
        VectorStorageService vectorStorageService,
        SemanticSearchService semanticSearchService,
        LlmService llmService,
        IWebHostEnvironment environment)
    {
        _pdfService = pdfService;
        _chunkService = chunkService;
        _embeddingService = embeddingService;
        _environment = environment;
        _vectorStorageService = vectorStorageService;
        _semanticSearchService = semanticSearchService;
        _llmService = llmService;
    }

    [HttpGet("ask")]
    public async Task<IActionResult> Ask(string question)
    {
        // 1. Retrieve relevant chunks from PostgreSQL
        var chunks = await _semanticSearchService.SearchAsync(question);

        if (chunks.Count == 0)
            return NotFound("No relevant information found.");

        // 2. Send question and retrieved chunks to Ollama
        var answer = await _llmService.GenerateAnswerAsync(
            question,
            chunks
        );

        // 3. Return final answer
        return Ok(new
        {
            Question = question,
            Answer = answer,
            RetrievedChunks = chunks.Count
        });
    }

    [HttpGet("process")]
    public async Task<IActionResult> ProcessPdf()
    {
        var path = Path.Combine(
            _environment.ContentRootPath,
            "Documents",
            "superman.pdf"
        );

        if (!System.IO.File.Exists(path))
            return NotFound("PDF not found.");

        // 1. Extract PDF text
        var text = _pdfService.ExtractText(path);

        // 2. Create chunks
        var chunks = _chunkService.CreateChunks(text, 100);

        // 3. Generate embeddings using Ollama
        var embeddings =
            await _embeddingService.GenerateEmbeddingsAsync(chunks);

        // Step 4: Save chunks and embeddings in PostgreSQL
        await _vectorStorageService.SaveChunksAsync(
            "superman.pdf",
            chunks,
            embeddings
        );

        return Ok(new
        {
            TotalChunks = chunks.Count,
            TotalEmbeddings = embeddings.Length,
            VectorDimensions = embeddings.Length > 0
                ? embeddings[0].Length : 0,

            Results = chunks.Select((chunk, index) => new
            {
                Chunk = chunk,
                Embedding = embeddings[index]
            })
        });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string question)
    {
        var results = await _semanticSearchService.SearchAsync(question);

        return Ok(results);
    }
}