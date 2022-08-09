import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { PhotoModalComponent } from 'src/app/modals/photo-modal/photo-modal.component';
import { Pagination } from 'src/app/_models/pagination';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Partial<Photo[]>;
  pagination: Pagination;
  bsModalRef: BsModalRef;
  pageNumber = 1;
  pageSize = 4;
  
  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosToModerate(this.pageNumber, this.pageSize).subscribe(response => {
      this.photos = response.result;
      this.pagination = response.pagination;
      console.log(JSON.stringify(response));
    });
  }

  openPhotoModal(photo: Photo) {
    const config = {
      class: 'modal-dialog-center',
      initialState: {
        photo
      }
    }
    this.bsModalRef = this.modalService.show(PhotoModalComponent, config);
  }
  approve(predicate : boolean) {
    let photosIdsToBeApproved = this.photos.filter(p => p.marked == true).map(p => p.id);
    if (photosIdsToBeApproved.length > 0) {
      const params = {
        PhotoIds : photosIdsToBeApproved,
        Predicate : predicate
      }
      this.adminService.approveOrDisapprovePhotos(params).subscribe(resp => {
        console.log(resp);
        this.getPhotosForApproval();
      });
    } else {
      alert('No Photos to ' + ((predicate)? 'approve' : 'reject'));
    }
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.getPhotosForApproval();
    }
  }
}
