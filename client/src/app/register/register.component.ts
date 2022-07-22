import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {


  @Output() cancelRegister = new EventEmitter();
  model: any = {};
  errorMessage: string = "";

  constructor(public accountService : AccountService) { }

  ngOnInit(): void {

  }

  register() {
    console.log('Register!');
    // validate password first
    if (this.model.password !== this.model.confPassword) {
      this.errorMessage = "Password doesn't match";
    } else {
      this.errorMessage = "";
      delete this.model['confPassword'];
      this.accountService.register(this.model).subscribe(
        response => {
          console.log(response);
        },
        error => {
          console.log(error);
        }
      );
    }
  }

  cancel() {
    console.log('Cancel!');
    this.cancelRegister.emit(false);
  }
}
