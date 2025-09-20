import { NgModule } from "@angular/core";
import { DashboardComponent } from './dashboard/dashboard.component';
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { NgbModule, NgbPaginationModule } from "@ng-bootstrap/ng-bootstrap";
import { TranslateModule } from "@ngx-translate/core";
import { SharedModule } from "app/shared/shared.module";
import { NgxSpinnerModule } from "ngx-spinner";
import { AdminPagesRoutingModule } from "./admin-pages-routing.module";
import { InquiryComponent } from './student/inquiry/inquiry.component';

@NgModule({
  declarations: [
    DashboardComponent,
  ],
  imports: [
    AdminPagesRoutingModule,
    NgbModule,
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    NgbPaginationModule,
    NgxSpinnerModule,
    TranslateModule,
    FormsModule,
  ],
})
export class AdminPagesModule { }
