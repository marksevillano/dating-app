import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(private messagesService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messagesService.getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe(resp => {
        this.loading = false;
        this.messages = resp.result;
        this.pagination = resp.pagination;
      });
  }

  deleteMessage(message: Message) {
    this.messagesService.deleteMessage(message.id.toString()).subscribe(() => {
      this.messages = this.messages.filter(m => m.id !== message.id);
    });
  }
}
