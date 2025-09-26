import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AdministrationService } from 'app/pages/admin/_services/administration.service';
import { PASSWORD_PATTERN } from 'app/shared/data/global-constant';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'change-password-modal',
  templateUrl: './change-password-modal.component.html',
  styleUrls: ['./change-password-modal.component.scss',]
})
export class ChangePasswordModalComponent implements OnInit {
  @Input() id: number;
  @Output() onUser_Emit: EventEmitter<boolean> = new EventEmitter();

  form: FormGroup;
  showPassword = false;
  show_eye1 = false;
  show_eye2 = false;

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    public administrationService: AdministrationService
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      newPassword: [null, Validators.compose([Validators.required, Validators.pattern(PASSWORD_PATTERN)])],
      confirmPassword: [null, Validators.compose([Validators.required])],
    },
      {
        validator: ConfirmedValidator("newPassword", "confirmPassword"),
      });
  }

  toggleNewPassword() {
    this.show_eye1 = !this.show_eye1;
  }

  toggleConfirmPassword() {
    this.show_eye2 = !this.show_eye2;
  }

  onClick_Submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning('Enter valid form details');
      return;
    }
    const data = {
      id: this.id,
      newPassword: this.form.get('newPassword')?.value,
    }
    this.spinner.show("modalspin");
    this.administrationService.changePassword(data).pipe(finalize(() => this.spinner.hide("modalspin"))).subscribe({
      next: (res: any) => {
        if (res) {
          this.toastr.success("Password changed successfully");
          this.onUser_Emit.emit(true);
          this.activeModal.close();
        } else {
          this.toastr.error(res, "Failed to change password");
        }
      },
      error: (err: any) => {
        this.toastr.error(err || 'Something went wrong');
      }
    });
  }
}

export function ConfirmedValidator(
  controlName: string,
  matchingControlName: string
) {
  return (formGroup: FormGroup) => {
    const control = formGroup.controls[controlName];
    const matchingControl = formGroup.controls[matchingControlName];
    if (matchingControl.errors && !matchingControl.errors.confirmedValidator) {
      return;
    }
    if (control.value !== matchingControl.value) {
      matchingControl.setErrors({ confirmedValidator: true });
    } else {
      matchingControl.setErrors(null);
    }
  };

}
