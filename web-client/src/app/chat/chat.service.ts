import { Injectable,  signal } from '@angular/core';

export enum ChatMessageType {
  Gift = 0,
  Join,
  Leave,
  Message,
  System,
  UserList,
  Welcome
}

export interface ChatMessage {
  Nickname: string;
  Text: string;
  Type: ChatMessageType;
}

@Injectable()
export class ChatService {
  readonly nickname = signal<string>('');
  private socket?: WebSocket;
  
  readonly messages = signal<ChatMessage[]>([]);
  readonly userList = signal<string[]>([]);

  connect(token: string): void {
    this.socket = new WebSocket(`ws://localhost:5000/chat?token=${token}`);

    this.socket.onopen = () => {};

    this.socket.onmessage = (event) => {
      const message = JSON.parse(event.data) as ChatMessage;

      switch (message.Type) {
        case ChatMessageType.UserList:
          const nicknames = message.Text.split(", ");
          this.userList.set(nicknames);
          break;
        case ChatMessageType.Welcome:
          this.nickname.set(message.Nickname);
          this.messages.update((prev) => [...prev, message]);
          break;
        default:
          this.messages.update((prev) => [...prev, message]);
          break;
      }
    };

    this.socket.onclose = () => {
      this.nickname.set('');
    };

    this.socket.onerror = () => {
      this.nickname.set('');
    };
  }

  send(message: string): void {
    if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
      console.warn('WebSocket이 아직 열리지 않았습니다.');
      return;
    }

    const payload: ChatMessage = {
      Nickname: this.nickname(),
      Text: message,
      Type: ChatMessageType.Message
    };

    if (!message.startsWith("/")) {
      this.messages.update((prev) => [...prev, payload]);
    }
    this.socket.send(JSON.stringify(payload));
  }
}