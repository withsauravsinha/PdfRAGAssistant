namespace PdfKnowledgeAssistant.Models
{
    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
    }
}
