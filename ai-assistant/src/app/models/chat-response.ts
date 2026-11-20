export interface ChatResponse {
  conversationId: string;
  answer: string;
  requiresInput: boolean;
  requiredInput?: string;
}