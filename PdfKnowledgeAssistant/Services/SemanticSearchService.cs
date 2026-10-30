using Npgsql;

namespace PdfKnowledgeAssistant.Services;

public class SemanticSearchService
{
    private readonly string _connectionString;
    private readonly EmbeddingService _embeddingService;

    public SemanticSearchService(
        IConfiguration configuration,
        EmbeddingService embeddingService)
    {
        _connectionString = configuration
            .GetConnectionString("PostgresConnection")!;

        _embeddingService = embeddingService;
    }

    public async Task<List<string>> SearchAsync(string question)
    {
        // 1. Generate question embedding
        var embeddings = await _embeddingService
            .GenerateEmbeddingsAsync(new List<string> { question });

        var questionVector = "[" +
            string.Join(",", embeddings[0]) + "]";

        // 2. Search PostgreSQL
        var results = new List<string>();

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT content
            FROM document_chunks
            ORDER BY embedding <=> $1::vector
            LIMIT 3;
            """,
            connection);

        command.Parameters.AddWithValue(questionVector);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }
}