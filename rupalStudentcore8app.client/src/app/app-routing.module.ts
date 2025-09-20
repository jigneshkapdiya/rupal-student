import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { FullLayoutComponent } from "./layouts/full/full-layout.component";
import { AccountLayoutComponent } from './layouts/account/account-layout.component';
import { AuthGuard } from './shared/auth/auth-guard.service';
import { ACCOUNT_ROUTES } from './shared/routes/account-layout.routes';
import { Full_ROUTES } from './shared/routes/full-layout.routes';
import { HOME_ROUTES } from './shared/routes/home-layout.routes';
import { HomeLayoutComponent } from './layouts/home/home-layout.component';

const appRoutes: Routes = [
  {
    path: "",
    redirectTo: "/home",
    pathMatch: "full",
  },
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
    component: HomeLayoutComponent,
    data: { title: 'Home' },
    children: HOME_ROUTES,
  },
  {
    path: '**',
    redirectTo: 'pages/error'
  },

];

@NgModule({
  imports: [RouterModule.forRoot(appRoutes, { preloadingStrategy: PreloadAllModules, relativeLinkResolution: 'legacy' })],
  exports: [RouterModule]
})

export class AppRoutingModule {
}
