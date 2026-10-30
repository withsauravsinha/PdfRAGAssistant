using Npgsql;
using System.Globalization;

namespace PdfKnowledgeAssistant.Services;

public class VectorStorageService
{
    private readonly string _connectionString;

    public VectorStorageService(IConfiguration configuration)
    {
        _connectionString = configuration
            .GetConnectionString("PostgresConnection")!;
    }

    public async Task SaveChunksAsync(
        string documentName,
        List<string> chunks,
        float[][] embeddings)
    {
        if (chunks.Count != embeddings.Length)
            throw new ArgumentException("Chunks and embeddings count do not match.");

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        for (int i = 0; i < chunks.Count; i++)
        {
            string vector = "[" + string.Join(",",
                embeddings[i].Select(x =>
                    x.ToString("R", CultureInfo.InvariantCulture))) + "]";

            await using var command = new NpgsqlCommand(
                """
                INSERT INTO document_chunks
                (document_name, chunk_index, content, embedding)
                VALUES ($1, $2, $3, $4::vector);
                """,
                connection,
                transaction);

            command.Parameters.AddWithValue(documentName);
            command.Parameters.AddWithValue(i);
            command.Parameters.AddWithValue(chunks[i]);
            command.Parameters.AddWithValue(vector);

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}