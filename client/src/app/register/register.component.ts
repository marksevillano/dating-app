import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
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

  constructor(public accountService : AccountService, private toastrService : ToastrService) { }

  ngOnInit(): void {

  }

  register() {
    console.log('Register!');
    // validate password first
    if (this.model.password !== this.model.confPassword) {
      this.toastrService.error("Password doesn't match","Error");
    } else {
      delete this.model['confPassword'];
      this.accountService.register(this.model).subscribe(
        response => {
          this.toastrService.success("Registration complete!", "Success");
          console.log(response);
        },
        error => {
          this.toastrService.error(error.error, "Error");
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
