import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm: NgForm;
  member: Member;
  user: User;
  genderList = [{
    value: 'male', label: 'Male'
  }, {
    value: 'female', label: 'Female'
  }, {
    value: 'both', label: 'Both'
  }];
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(private accountService: AccountService, 
      private memberService: MembersService,
      private toastrService: ToastrService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user)
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService.getMember(this.user.username)
      .subscribe(member => {
        this.member = member;
    });
  }

  updateMember() {
    console.log(this.member);
    this.memberService.updateMember(this.member).subscribe(() => {
      this.editForm.reset(this.member);
      this.user.preferenceGender = this.member.preferenceGender;
      this.accountService.setCurrentUser(this.user);
      this.toastrService.success('Profile updated successfully');
    });
    
  }
}
