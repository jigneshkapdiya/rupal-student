import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { AccountLayoutComponent } from './layouts/account/account-layout.component';
import { FullLayoutComponent } from "./layouts/full/full-layout.component";
import { StudentMarkSheetComponent } from './pages/reg-form/student-mark-sheet/student-mark-sheet.component';
import { AuthGuard } from './shared/auth/auth-guard.service';
import { ACCOUNT_ROUTES } from './shared/routes/account-layout.routes';
import { Full_ROUTES } from './shared/routes/full-layout.routes';
import { ViewStudentMarkSheetComponent } from './pages/reg-form/view-student-mark-sheet/view-student-mark-sheet.component';

const appRoutes: Routes = [
  {
    path: "",
    redirectTo: "/reg-form",
    pathMatch: "full",
  },
  // Direct routes without layout
  {
    path: "reg-form",
    component: StudentMarkSheetComponent,
    data: { title: "Registration Form" }
  },
  {
    path: "reg-form/view/:id",
    component: ViewStudentMarkSheetComponent,
    data: { title: "Confirmation" }
  },
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
    path: '**',
    redirectTo: '/reg-form'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(appRoutes, {
    preloadingStrategy: PreloadAllModules,
    relativeLinkResolution: 'legacy',
    useHash: true,  // This will use hash routing (/#/reg-form/view/123)
    enableTracing: false
  })],
  exports: [RouterModule]
})

export class AppRoutingModule {
}
