
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ChatService } from './services/chat.service';
import { ChatResponse } from './models/chat-response';

interface Message {
  role: 'user' | 'assistant';
  content: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {

  question = '';
  conversationId: string | null = null;

  messages = signal<Message[]>([]);
  isLoading = signal(false);

  constructor(private chatService: ChatService) {}

  sendMessage() {

    const userQuestion = this.question.trim();

    if (!userQuestion || this.isLoading()) return;

    // Add user message
    this.messages.update(messages => [
      ...messages,
      {
        role: 'user',
        content: userQuestion
      }
    ]);

    this.question = '';
    this.isLoading.set(true);

    this.chatService.askQuestion({
      question: userQuestion,
      conversationId: this.conversationId ?? undefined
    }).subscribe({

      next: (response: ChatResponse) => {

        //this.conversationId = response.conversationId;

        // Add assistant response
        this.messages.update(messages => [
          ...messages,
          {
            role: 'assistant',
            content: response.answer
          }
        ]);

        this.isLoading.set(false);

      },

      error: (error) => {

        console.error(error);

        this.messages.update(messages => [
          ...messages,
          {
            role: 'assistant',
            content: 'Something went wrong. Please try again.'
          }
        ]);

        this.isLoading.set(false);
      }
    });
  }

  newChat() {

    this.messages.set([]);

    this.conversationId = null;
    this.question = '';

    this.isLoading.set(false);
  }
}
