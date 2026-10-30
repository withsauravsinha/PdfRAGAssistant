namespace PdfKnowledgeAssistant.Services;

public class ChunkingService
{
    public List<string> CreateChunks(
        string text,
        int chunkSize = 100)
    {
        var words = text.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries
        );

        var chunks = new List<string>();

        for (int i = 0; i < words.Length; i += chunkSize)
        {
            var chunk = string.Join(
                " ",
                words.Skip(i).Take(chunkSize)
            );

            chunks.Add(chunk);
        }

        return chunks;
    }
}