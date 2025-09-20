import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { NgxSpinnerService } from 'ngx-spinner';
import { AccountService } from '../../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-forgot-password-modal',
  templateUrl: './forgot-password-modal.component.html',
  styleUrls: ['./forgot-password-modal.component.scss']
})
export class ForgotPasswordModalComponent implements OnInit {
  forgotPasswordFormSubmitted = false;
  forgotPasswordForm: FormGroup;

  constructor(
    public activeModal: NgbActiveModal,
    private spinner: NgxSpinnerService,
    private accountService: AccountService,
    public toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.forgotPasswordForm = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email])
    });
  }


  get fpf() {
    return this.forgotPasswordForm.controls;
  }


  onSendVerificationCode() {
    this.forgotPasswordFormSubmitted = true;
    if (this.forgotPasswordForm.invalid) {
      return;
    } else {
      this.spinner.show("modalspin");
      const email = {
        email: this.forgotPasswordForm.value.email,
      };
      this.accountService.sendVerificationLink(email).subscribe(
        (result) => {
          this.spinner.hide("modalspin");
          if (result) {
            this.toastr.success("Verification link sent successfully.");
            this.activeModal.close();
          } else {
            this.toastr.error("Something went wrong");
          }
        },
        (error) => {
          this.spinner.hide("modalspin");
          this.toastr.error(error, 'Failed to send verification link');
        }
      );
    }
  }

}
