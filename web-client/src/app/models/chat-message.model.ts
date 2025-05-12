export enum ChatMessageType {
  Gift = 0,
  Join = 1,
  Leave = 2,
  Message = 3,
  System = 4,
  UserList = 5,
  Welcome = 6
}

export interface ChatMessage {
  Nickname: string;
  Type: ChatMessageType;
  Text: string;
} 