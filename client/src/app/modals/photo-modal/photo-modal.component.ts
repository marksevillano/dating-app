import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-photo-modal',
  templateUrl: './photo-modal.component.html',
  styleUrls: ['./photo-modal.component.css']
})
export class PhotoModalComponent implements OnInit {
  photo: Photo;

  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  close() {
    this.bsModalRef.hide();
  }


}
