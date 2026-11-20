namespace PdfKnowledgeAssistant.Models
{
    public class ChatResponse
    {
        public string ConversationId { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public bool RequiresInput { get; set; }

        public string? RequiredInput { get; set; }
    }
}
