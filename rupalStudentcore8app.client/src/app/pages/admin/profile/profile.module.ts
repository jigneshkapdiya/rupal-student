import { NgModule } from "@angular/core";
import { MyProfileComponent } from './my-profile/my-profile.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { TwoFactorAuthenticatorComponent } from './two-factor-authenticator/two-factor-authenticator.component';
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { ProfileRoutingModule } from "./profile-routing.module";
import { NgxSpinnerModule } from "ngx-spinner";
import { ToastrModule } from "ngx-toastr";
import { AccountDetailsComponent } from './account-details/account-details.component';

@NgModule({
  declarations: [
    MyProfileComponent,
    ChangePasswordComponent,
    TwoFactorAuthenticatorComponent,
    AccountDetailsComponent
  ],
  imports: [
    CommonModule,
    ProfileRoutingModule,
    NgbModule,
    FormsModule,
    ReactiveFormsModule,
    NgxSpinnerModule,
    ToastrModule.forRoot(),
  ],
})
export class ProfileModule { }
