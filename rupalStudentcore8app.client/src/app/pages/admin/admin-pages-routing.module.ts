import { RouterModule, Routes } from "@angular/router";
import { DashboardComponent } from "./dashboard/dashboard.component";
import { NgModule } from "@angular/core";

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: "dashboard",
        component: DashboardComponent,
        data: {
          title: "Dashboard",
          title_ar: 'لوحة القيادة  ',
        },
      },
      {
        path: "profile",
        loadChildren: () =>
          import("../admin/profile/profile.module").then(
            (m) => m.ProfileModule
          ),
      },
      {
        path: "administration",
        loadChildren: () =>
          import("../admin/administration/administration.module").then(
            (m) => m.AdministrationModule
          ),
      },
      {
        path: "student",
        loadChildren: () =>
          import("../admin/student/student.module").then(
            (m) => m.StudentModule
          ),
      },
    ]
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminPagesRoutingModule { }
