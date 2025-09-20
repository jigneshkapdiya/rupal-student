import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { TwoFactorVerificationComponent } from "./two-factor-verification/two-factor-verification.component";

const routes: Routes = [
  {
    path: "",
    children: [
      {
        path: "login",
        component: LoginComponent,
        data: {
          title: "Login Page",
        },
      },
      {
        path: "verify-2fa",
        component: TwoFactorVerificationComponent,
        data: {
          title: "Verify 2FA Code",
        },
      },


    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountPagesRoutingModule { }
