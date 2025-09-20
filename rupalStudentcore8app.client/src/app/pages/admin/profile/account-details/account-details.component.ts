import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PatternEmail } from 'app/shared/data/global-constant';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { ProfileService } from '../../_services/profile.service';
import { finalize } from 'rxjs/operators';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'account-details',
  templateUrl: './account-details.component.html',
  styleUrls: ['./account-details.component.scss']
})
export class AccountDetailsComponent implements OnInit {
  form: FormGroup;
  activeTab: string = 'general';
  imageProfile: string | SafeUrl;

  constructor(
    private formBuilder: FormBuilder,
    private profileService: ProfileService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private sanitizer: DomSanitizer,
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      id: [null],
      fullName: [null, Validators.compose([Validators.required])],
      fullNameAr: [null],
      userName: [null, Validators.compose([Validators.required])],
      email: [null, Validators.compose([Validators.required, Validators.pattern(PatternEmail)])],
      phoneNumber: [null, Validators.compose([Validators.required, Validators.minLength(9), Validators.maxLength(9)])],
      imageSource: [null],
    });
    this.getUserById();
  }

  getUserById() {
    this.spinner.show();
    this.profileService.getUserById().pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res) => {
        if (res) {
          this.form.patchValue({
            id: res.id,
            fullName: res.fullName,
            fullNameAr: res.fullNameAr,
            userName: res.userName,
            email: res.email,
            phoneNumber: res.phoneNumber,
          });
          if (res.profileImage) {
            this.imageProfile = res.profileImage
          }
        }
      },
      error: (error) => {
        this.toastr.error(error, "Fail to get user details");
      }
    });
  }

  onFileChange(event) {
    const file = event.target.files[0];
    if (file) {
      const validTypes = ['image/jpeg', 'image/png', 'image/jpg'];
      if (!validTypes.includes(file.type)) {
        this.toastr.error('Only JPG, JPEG, and PNG files are allowed.');
        this.form.get('imageSource').setValue(null);
        return;
      }

      this.imageProfile = this.sanitizer.bypassSecurityTrustUrl(
        window.URL.createObjectURL(file)
      );
      this.form.get('imageSource').setValue(file);
    }
  }

  onSubmit_Profile() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Enter valid form details.");
      return;
    }
    let formData = new FormData();
    formData.append('id', this.form.get('id').value.toString());
    formData.append('fullName', this.form.get('fullName').value);
    formData.append('fullNameAr', this.form.get('fullNameAr').value);
    formData.append('userName', this.form.get('userName').value);
    formData.append('email', this.form.get('email').value);
    formData.append('phoneNumber', this.form.get('phoneNumber').value);
    formData.append('profileImage', this.form.get('imageSource').value);
    this.spinner.show();
    this.profileService.editUser(formData).pipe(finalize(() => this.spinner.hide())).subscribe(
      {
        next: (res) => {
          if (res) {
            this.toastr.success("Profile updated successfully.");
            this.getUserById();
          }
          else {
            this.toastr.error("Fail to update user details");
          }
        },
        error: (error) => {
          this.toastr.error(error, "Fail to update user details");
        }
      }
    );
  }

  resetImage() {
    this.imageProfile = 'assets/image/user.png'; // reset to default
  }
}
