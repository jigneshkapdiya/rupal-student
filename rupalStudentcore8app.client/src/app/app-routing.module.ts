import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { AccountLayoutComponent } from './layouts/account/account-layout.component';
import { FullLayoutComponent } from "./layouts/full/full-layout.component";
import { StudentLayoutComponent } from './layouts/student/student-layout.component';
import { AuthGuard } from './shared/auth/auth-guard.service';
import { ACCOUNT_ROUTES } from './shared/routes/account-layout.routes';
import { Full_ROUTES } from './shared/routes/full-layout.routes';
import { STUDENT_ROUTES } from './shared/routes/student-layout.routes';

const appRoutes: Routes = [
  {
    path: "",
    redirectTo: "/reg-form",
    pathMatch: "full",
  },
  // Direct routes without layout
  // {
  //   path: "reg-form",
  //   component: StudentMarkSheetComponent,
  //   data: { title: "Registration Form" }
  // },
  // {
  //   path: "reg-form/view/:id",
  //   component: ViewStudentMarkSheetComponent,
  //   data: { title: "Confirmation" }
  // },
  // Layout-based routes
  {
    path: "",
    component: AccountLayoutComponent,
    data: { title: "Login" },
    children: ACCOUNT_ROUTES,
  },
  {
    path: "",
    component: FullLayoutComponent,
    data: { title: "Dashboard" },
    children: Full_ROUTES,
    canActivate: [AuthGuard],
  },
  {
    path: "",
    component: StudentLayoutComponent,
    data: { title: 'Student' },
    children: STUDENT_ROUTES,
  },
  {
    path: '**',
    redirectTo: '/pages/error'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(appRoutes, { preloadingStrategy: PreloadAllModules, relativeLinkResolution: 'legacy' })],
  exports: [RouterModule]
})

export class AppRoutingModule {
}
