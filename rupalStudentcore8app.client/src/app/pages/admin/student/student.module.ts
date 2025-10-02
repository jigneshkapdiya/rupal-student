import { NgModule } from "@angular/core";
import { StudentRoutingModule } from "./student-routing.module";
import { InquiryComponent } from "./inquiry/inquiry.component";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule, NgbPaginationModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { SharedModule } from "app/shared/shared.module";
import { NgxSpinnerModule } from "ngx-spinner";
import { ToastrModule } from "ngx-toastr";
import { EditStudentComponent } from './edit-student/edit-student.component';
import { FileUploadModule } from "ng2-file-upload";
import { Inquiry2Component } from './inquiry2/inquiry2.component';

@NgModule({
  declarations: [InquiryComponent, EditStudentComponent, Inquiry2Component],
  imports: [StudentRoutingModule,
    NgbModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgSelectModule,
    SharedModule,
    NgbPaginationModule,
    NgxSpinnerModule,
    ToastrModule,
    FileUploadModule
  ],
})
export class StudentModule { }
