import { Component, OnInit } from '@angular/core';
import { ProfileService } from '../../_services/profile.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/internal/operators/finalize';
import swal from "sweetalert2";

@Component({
  selector: 'two-factor-authenticator',
  templateUrl: './two-factor-authenticator.component.html',
  styleUrls: ['./two-factor-authenticator.component.scss']
})
export class TwoFactorAuthenticatorComponent implements OnInit {
  profileDetails: any;

  constructor(
    private profileService: ProfileService,
    private spinner: NgxSpinnerService,
    public toastr: ToastrService
  ) {
    this.getUserById();
  }

  ngOnInit(): void {
  }

  getUserById() {
    this.spinner.show();
    this.profileService.getUserById().pipe(finalize(() => this.spinner.hide())).subscribe(
      (result) => {
        if (result) {
          this.profileDetails = result;
        }
        else {
          this.profileDetails = [];
        }
      },
      (error) => {
        this.toastr.error(error, "Fail to get user details");
      }
    );
  }

  disableTFA(userId: number, ActivationStatus: boolean) {
    const data = {
      status: ActivationStatus,
      id: userId
    };
    swal.fire({
      title: 'Are you sure?',
      icon: "warning",
      text: data.status ? 'You want to disable 2FA' : 'You want to enable 2FA',
      confirmButtonText: data.status ? 'Yes, Disable it' : 'Yes, Enable it',
      cancelButtonText: 'No, Keep it',
      showCancelButton: true,
      showCloseButton: true,
      customClass: {
        confirmButton: "btn btn-primary m-1",
        cancelButton: "btn btn-secondary m-1",
      },
      buttonsStyling: false,
    }).then((result) => {
      if (result.isConfirmed) {
        this.spinner.show();
        this.profileService.disableTFA(userId).pipe(finalize(() => this.spinner.hide())).subscribe(
          (result) => {
            if (result) {
              this.toastr.success(
                data.status ? 'Two - Factor Authenticator is disabled successfully.' : 'Two - Factor Authenticator is enabled successfully', 'Success'
              );
              this.getUserById();
            }
          },
          (error) => {
            this.toastr.error(error, 'Failed to update Two-Factor Authenticator');
          }
        );
      }
    });
  }

}
