using UglyToad.PdfPig;

namespace PdfKnowledgeAssistant.Services;

public class PdfExtractionService
{
    public string ExtractText(string filePath)
    {
        using var document = PdfDocument.Open(filePath);

        var text = string.Join(
            Environment.NewLine,
            document.GetPages().Select(page => page.Text)
        );

        return text;
    }
}