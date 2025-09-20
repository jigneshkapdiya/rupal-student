import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PASSWORD_PATTERN } from 'app/shared/data/global-constant';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { ProfileService } from '../../_services/profile.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent implements OnInit {
  form: FormGroup;
  show_eye1 = false;
  show_eye2 = false;
  show_eye3 = false;

  constructor(
    private formBuilder: FormBuilder,
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private profileService: ProfileService
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      oldPassword: [null, Validators.compose([Validators.required, Validators.pattern(PASSWORD_PATTERN)])],
      newPassword: [null, Validators.compose([Validators.required, Validators.pattern(PASSWORD_PATTERN)])],
      confirmPassword: [null, Validators.compose([Validators.required])],
    },
      {
        validator: ConfirmedValidator("newPassword", "confirmPassword"),
      });
  }

  toggleOldPassword() {
    this.show_eye1 = !this.show_eye1;
  }

  toggleNewPassword() {
    this.show_eye2 = !this.show_eye2;
  }

  toggleConfirmPassword() {
    this.show_eye3 = !this.show_eye3;
  }

  onClick_Submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Enter valid form details");
      return;
    }
    const data = {
      oldPassword: this.form.get('oldPassword')?.value,
      newPassword: this.form.get('newPassword')?.value,
      confirmPassword: this.form.get('confirmPassword')?.value,
    }
    this.spinner.show();
    this.profileService.changePassword(data).pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        this.toastr.success("Password changed successfully");
      },
      error: (err: any) => {
        this.toastr.error("Failed to change password");
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
