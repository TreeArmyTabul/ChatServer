import { Component, input } from '@angular/core';
import { ChatMessage, ChatMessageType } from '../chat.service';

@Component({
  selector: 'chat-message',
  styleUrl: './message.component.scss',
  templateUrl: './message.component.html',
})
export class MessageComponent {
  isSelf = input.required<boolean>();
  message = input.required<ChatMessage>();

  ChatMessageType = ChatMessageType;
}