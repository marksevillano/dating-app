import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { ConfirmService } from '../_services/confirm.service';
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

  constructor(private messagesService: MessageService, private confirmService: ConfirmService) { }

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
    this.confirmService.popUpConfirm('Confirm delete message', 'This cannot be undone').subscribe(
      result => {
        if(result) {
          this.messagesService.deleteMessage(message.id.toString()).subscribe(() => {
            this.messages = this.messages.filter(m => m.id !== message.id);
          });
        }
      }
    )
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
