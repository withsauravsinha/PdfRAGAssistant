using PdfKnowledgeAssistant.Models;
using PdfKnowledgeAssistant.Services.Employee;

namespace PdfKnowledgeAssistant.Services.Agent
{
    public class AgentOrchestrator
    {
        private readonly ConversationMemory _conversationMemory;
        private readonly EmployeeLeaveService _employeeService;
        private readonly SemanticSearchService _semanticSearchService;
        private readonly LlmService _llmService;

        public AgentOrchestrator(
            ConversationMemory conversationMemory,
            EmployeeLeaveService employeeService,
            SemanticSearchService semanticSearchService,
            LlmService llmService)
        {
            _conversationMemory = conversationMemory;
            _employeeService = employeeService;
            _semanticSearchService = semanticSearchService;
            _llmService = llmService;
        }

        public async Task<ChatResponse> ProcessAsync(ChatRequest request)
        {
            var conversationId =
                request.ConversationId ?? Guid.NewGuid().ToString();

            var state = _conversationMemory.Get(conversationId);

            string question = request.Question.Trim();

            // STEP 1: Handle employee ID response
            if (state.PendingInput == "EmployeeId")
            {
                var employee = _employeeService.GetEmployee(question);

                if (employee == null)
                {
                    return new ChatResponse
                    {
                        ConversationId = conversationId,
                        Answer = "Invalid employee ID. Please enter a valid employee ID.",
                        RequiresInput = true,
                        RequiredInput = "EmployeeId"
                    };
                }

                state.EmployeeId = employee.EmployeeId;
                state.PendingInput = null;

                // Continue with the original question
                question = state.OriginalQuestion!;
            }

            // STEP 2: Handle category response
            else if (state.PendingInput == "Category")
            {
                var category = question.ToUpperInvariant();

                if (!new[] { "T0", "T1", "T2", "T3", "T4" }
                    .Contains(category))
                {
                    return new ChatResponse
                    {
                        ConversationId = conversationId,
                        Answer = "Please enter a valid category: T0, T1, T2, T3 or T4.",
                        RequiresInput = true,
                        RequiredInput = "Category"
                    };
                }

                _employeeService.UpdateCategory(
                    state.EmployeeId!,
                    category
                );

                state.PendingInput = null;

                // Continue with the original question
                question = state.OriginalQuestion!;
            }

            // STEP 3: Identify leave balance question
            bool isLeaveBalanceQuestion =
                                            question.Contains("leave", StringComparison.OrdinalIgnoreCase)
                                            && (
                                                question.Contains("remaining", StringComparison.OrdinalIgnoreCase)
                                                || question.Contains("balance", StringComparison.OrdinalIgnoreCase)
                                                || question.Contains("left", StringComparison.OrdinalIgnoreCase)
                                                || question.Contains("taken", StringComparison.OrdinalIgnoreCase)
                                            );

            // STEP 4: Ask for employee ID
            if (isLeaveBalanceQuestion &&
                string.IsNullOrWhiteSpace(state.EmployeeId))
            {
                state.OriginalQuestion = question;
                state.PendingInput = "EmployeeId";

                return new ChatResponse
                {
                    ConversationId = conversationId,
                    Answer = "Please provide your employee ID.",
                    RequiresInput = true,
                    RequiredInput = "EmployeeId"
                };
            }

            // STEP 5: Retrieve employee
            if (isLeaveBalanceQuestion)
            {
                var employee = _employeeService.GetEmployee(
                    state.EmployeeId!
                );

                if (employee == null)
                {
                    state.PendingInput = "EmployeeId";

                    return new ChatResponse
                    {
                        ConversationId = conversationId,
                        Answer = "Employee not found. Please provide a valid employee ID.",
                        RequiresInput = true,
                        RequiredInput = "EmployeeId"
                    };
                }

                // STEP 6: Ask for category
                if (string.IsNullOrWhiteSpace(employee.Category))
                {
                    state.OriginalQuestion = question;
                    state.PendingInput = "Category";

                    return new ChatResponse
                    {
                        ConversationId = conversationId,
                        Answer = "Your category is missing. Are you T0, T1, T2, T3 or T4?",
                        RequiresInput = true,
                        RequiredInput = "Category"
                    };
                }

                // STEP 7: Add employee context
                question += $"""

                                Employee details:
                                Employee ID: {employee.EmployeeId}
                                Category: {employee.Category}
                                Casual leave taken: {employee.CasualLeaveTaken}
                                Sick leave taken: {employee.SickLeaveTaken}
                                Earned leave taken: {employee.EarnedLeaveTaken}
                                """;
            }

            // STEP 8: RAG search
            var chunks = await _semanticSearchService.SearchAsync(question);

            if (chunks.Count == 0)
            {
                return new ChatResponse
                {
                    ConversationId = conversationId,
                    Answer = "I couldn't find relevant information in the available documents."
                };
            }

            // STEP 9: LLM response
            var answer = await _llmService.GenerateAnswerAsync(
                question,
                chunks
            );

            // Clear completed request context
            state.OriginalQuestion = null;

            return new ChatResponse
            {
                ConversationId = conversationId,
                Answer = answer
            };
        }
    }
}
