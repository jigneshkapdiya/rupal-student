import { Routes } from '@angular/router';
export const STUDENT_ROUTES: Routes = [
  {
    path: 'reg-form',
    loadChildren: () => import('../../pages/reg-form/reg-form-pages.module').then(m => m.RegFormPagesModule)
  }
];
