import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerModule } from "ngx-spinner";
import { AccountPagesRoutingModule } from "./account-pages-routing.module";
import { LoginComponent } from "./login/login.component";
import { ForgotPasswordModalComponent } from "./login/forgot-password-modal/forgot-password-modal.component";
import { TwoFactorVerificationComponent } from './two-factor-verification/two-factor-verification.component';
import { ToastrModule } from "ngx-toastr";

@NgModule({
  imports: [
    AccountPagesRoutingModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgbModule,
    NgxSpinnerModule,
    ToastrModule,
  ],
  declarations: [
    LoginComponent,
    ForgotPasswordModalComponent,
    TwoFactorVerificationComponent,
  ],
})
export class AccountPagesModule { }
