import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { TranslateModule } from "@ngx-translate/core";
import { FileUploadModule } from "ng2-file-upload";
import { NgxSpinnerModule } from "ngx-spinner";
import { RegFormPagesRoutingModule } from "./reg-form-pages-routing.module";
import { StudentMarkSheetComponent } from "./student-mark-sheet/student-mark-sheet.component";
import { ViewStudentMarkSheetComponent } from "./view-student-mark-sheet/view-student-mark-sheet.component";
import { InquiryRegFormComponent } from './inquiry-reg-form/inquiry-reg-form.component';

@NgModule({
  declarations: [StudentMarkSheetComponent, ViewStudentMarkSheetComponent, InquiryRegFormComponent],
  imports: [
    RegFormPagesRoutingModule,
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
export class RegFormPagesModule { }
