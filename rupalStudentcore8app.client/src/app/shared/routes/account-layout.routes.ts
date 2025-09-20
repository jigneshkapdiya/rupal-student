import { Routes, RouterModule } from '@angular/router';

//Route for account layout without sidebar, navbar and footer for pages like Login, Registration etc...

export const ACCOUNT_ROUTES: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('../../pages/account/account-pages.module').then(m => m.AccountPagesModule)
  }
];
