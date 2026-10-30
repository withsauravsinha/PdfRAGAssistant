
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PdfKnowledgeAssistant.Services;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;

    public EmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[][]> GenerateEmbeddingsAsync(
        List<string> chunks)
    {
        var request = new
        {
            model = "nomic-embed-text",
            input = chunks
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/embed",
            request
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<EmbeddingResponse>();

        if (result?.Embeddings == null)
        {
            throw new Exception(
                "Ollama returned no embeddings.");
        }

        return result.Embeddings;
    }
}

public class EmbeddingResponse
{
    [JsonPropertyName("embeddings")]
    public float[][] Embeddings { get; set; } = [];
}
