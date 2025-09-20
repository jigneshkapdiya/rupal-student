import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule, NgbPaginationModule } from "@ng-bootstrap/ng-bootstrap";
import { SharedModule } from "app/shared/shared.module";
import { NgxSpinnerModule } from "ngx-spinner";
import { AdministrationRoutingModule } from "./administration-routing.module";
import { SystemUsersComponent } from './system-users/system-users.component';
import { SystemRolesComponent } from './system-roles/system-roles.component';
import { SystemPermissionsComponent } from './system-permissions/system-permissions.component';
import { SystemMenuComponent } from './system-menu/system-menu.component';
import { NgSelectModule } from "@ng-select/ng-select";
import { AddEditSystemUserModalComponent } from './system-users/add-edit-system-user-modal/add-edit-system-user-modal.component';
import { ChangePasswordModalComponent } from './system-users/change-password-modal/change-password-modal.component';
import { AddEditSystemRolesComponent } from './system-roles/add-edit-system-roles/add-edit-system-roles.component';
import { AddEditSystemMenuModalComponent } from './system-menu/add-edit-system-menu-modal/add-edit-system-menu-modal.component';
import { SystemPermissionModalComponent } from './system-menu/system-permission-modal/system-permission-modal.component';
import { SystemPermissionFormComponent } from "./system-menu/system-permission-modal/system-permission-form/system-permission-form.component";

@NgModule({
  declarations: [
    SystemUsersComponent,
    SystemRolesComponent,
    SystemPermissionsComponent,
    SystemMenuComponent,
    AddEditSystemUserModalComponent,
    ChangePasswordModalComponent,
    AddEditSystemRolesComponent,
    AddEditSystemMenuModalComponent,
    SystemPermissionModalComponent,
    SystemPermissionFormComponent
  ],
  imports: [
    AdministrationRoutingModule,
    NgbModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    NgbPaginationModule,
    NgxSpinnerModule,
    NgSelectModule
  ]
})
export class AdministrationModule { }
