namespace PdfKnowledgeAssistant.Services.Agent
{
    public class ConversationState
    {
        public string? EmployeeId { get; set; }

        public string? PendingInput { get; set; }

        public string? OriginalQuestion { get; set; }
    }
}
