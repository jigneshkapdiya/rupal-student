import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { SystemUsersComponent } from "./system-users/system-users.component";
import { SystemRolesComponent } from "./system-roles/system-roles.component";
import { SystemMenuComponent } from "./system-menu/system-menu.component";
import { SystemPermissionsComponent } from "./system-permissions/system-permissions.component";

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'users',
        component: SystemUsersComponent,
        data: {
          title: 'Users',
          title_ar: 'المستخدمون',
        }
      },
      {
        path: 'role',
        component: SystemRolesComponent,
        data: {
          title: 'Roles',
          title_ar: 'الأدوار',
        }
      },
      {
        path: 'menu',
        component: SystemMenuComponent,
        data: {
          title: 'Menus',
          title_ar: 'القوائم',
        }
      },
      {
        path: 'permissions',
        component: SystemPermissionsComponent,
        data: {
          title: 'Permissions',
          title_ar: 'الصلاحيات',
        }
      },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdministrationRoutingModule { }
