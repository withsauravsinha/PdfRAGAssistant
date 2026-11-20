namespace PdfKnowledgeAssistant.Services.Agent
{
    using System.Collections.Concurrent;

    public class ConversationMemory
    {
        private readonly ConcurrentDictionary<string, ConversationState>
            _conversations = new();

        public ConversationState Get(string conversationId)
        {
            return _conversations.GetOrAdd(
                conversationId,
                _ => new ConversationState()
            );
        }
    }
}
