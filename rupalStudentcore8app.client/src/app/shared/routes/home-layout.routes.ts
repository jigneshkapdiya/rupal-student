import { Routes } from '@angular/router';
export const HOME_ROUTES: Routes = [
  {
    path: 'home',
    loadChildren: () => import('../../pages/home/home-pages.module').then(m => m.HomePagesModule)
  }
];
