import { Injectable, signal } from '@angular/core';
import { ChatMessage, ChatMessageType } from '../models/chat-message.model';
import * as protobuf from 'protobufjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private socket: WebSocket | null = null;
  private root: protobuf.Root | null = null;
  private ChatMessage: protobuf.Type | null = null;
  private format: 'json' | 'protobuf' = 'json';

  readonly messages = signal<ChatMessage[]>([]);
  readonly userList = signal<string[]>([]);
  readonly nickname = signal<string>('');

  constructor() {
    this.loadProto();
  }

  private async loadProto() {
    try {
      this.root = await protobuf.load('/assets/chat_message.proto');
      this.ChatMessage = this.root.lookupType('ChatServer.ChatMessage');
    } catch (error) {
      console.error('프로토 파일 로드 실패:', error);
    }
  }

  connect(token: string, format: 'json' | 'protobuf'): void {
    this.format = format;
    this.socket = new WebSocket(`ws://localhost:5000/chat?format=${format}&token=${token}`);

    this.socket.onopen = () => {};

    this.socket.onmessage = async (event) => {
      let message: ChatMessage;

      if (this.format === 'protobuf' && this.ChatMessage) {
        try {
          const buffer = await event.data.arrayBuffer();
          const decoded = this.ChatMessage.decode(new Uint8Array(buffer));
          const object = this.ChatMessage.toObject(decoded, {
            defaults: true,
            arrays: true,
            objects: true,
            oneofs: true
          });
          
          message = {
            Type: object['type'],
            Nickname: object['nickname'],
            Text: object['text']
          };
        } catch (error) {
          console.error('프로토버프 디코딩 실패:', error);
          return;
        }
      } else {
        message = JSON.parse(event.data) as ChatMessage;
      }

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

    this.socket.onerror = () => {
      this.nickname.set('');
    };

    this.socket.onclose = () => {
      this.nickname.set('');
    };
  }

  sendMessage(text: string): void {
    if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
      return;
    }

    const message: ChatMessage = {
      Nickname: this.nickname(),
      Type: ChatMessageType.Message,
      Text: text
    };

    if (!text.startsWith("/")) {
      this.messages.update((prev) => [...prev, message]);
    }

    if (this.format === 'protobuf' && this.ChatMessage) {
      try {
        const payload = {
          nickname: this.nickname(),
          type: ChatMessageType.Message,
          text: text
        };
        const buffer = this.ChatMessage.encode(payload).finish();
        this.socket.send(buffer);
      } catch (error) {
        console.error('프로토버프 인코딩 실패:', error);
      }
    } else {
      this.socket.send(JSON.stringify(message));
    }
  }
}