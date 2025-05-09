import { Component, OnInit } from '@angular/core';
import { ChatService } from './chat.service';
import { FormsModule } from '@angular/forms';
import { MessageComponent } from "./components/message.component";
import { ActivatedRoute } from '@angular/router';

@Component({
  imports: [FormsModule, MessageComponent],
  providers: [ChatService],
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnInit {
  inputMessage: string = '';

  constructor(private activatedRoute: ActivatedRoute, public chatService: ChatService) {}

  ngOnInit(): void {
    const token = this.activatedRoute.snapshot.queryParams["token"]

    this.chatService.connect(token);
  }

  sendMessage(): void {
    const message = this.inputMessage.trim();

    if (message) {
      this.chatService.send(message);
      this.inputMessage = '';
    }
  }
}