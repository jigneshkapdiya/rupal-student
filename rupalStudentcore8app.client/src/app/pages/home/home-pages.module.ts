import { NgModule } from "@angular/core";
import { HomePagesRoutingModule } from "./home-pages-routing.module";
import { StudentMarksheetFormComponent } from './student-marksheet-form/student-marksheet-form.component';
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { TranslateModule } from "@ngx-translate/core";
import { NgxSpinnerModule } from "ngx-spinner";
import { FileUploadModule } from 'ng2-file-upload';
import { ConfirmStudentMarksheetComponent } from './confirm-student-marksheet/confirm-student-marksheet.component';
import { ViewMarkSheetDetailsComponent } from './view-mark-sheet-details/view-mark-sheet-details.component';

@NgModule({
  declarations: [
    StudentMarksheetFormComponent,
    ConfirmStudentMarksheetComponent,
    ViewMarkSheetDetailsComponent,
  ],
  imports: [HomePagesRoutingModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgbModule,
    NgxSpinnerModule,
    TranslateModule,
    NgSelectModule,
    NgxSpinnerModule,
    FileUploadModule
  ],
})
export class HomePagesModule { }
