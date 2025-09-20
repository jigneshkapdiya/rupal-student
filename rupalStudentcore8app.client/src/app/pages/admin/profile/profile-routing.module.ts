import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { MyProfileComponent } from "./my-profile/my-profile.component";

const routes: Routes = [
  {
    path: "",
    component: MyProfileComponent,
    data: {
      title: "",
      title_ar: 'حساب تعريفي',
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProfileRoutingModule { }
