import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { InquiryComponent } from "./inquiry/inquiry.component";
import { EditStudentComponent } from "./edit-student/edit-student.component";
import { Inquiry2Component } from "./inquiry2/inquiry2.component";

const routes: Routes = [
  {
    path: "",
    children: [
      {
        path: "inquiry",
        component: InquiryComponent,
        data: {
          title: "Student Inquiry",
        }
      },
      {
        path: "edit/:id",
        component: EditStudentComponent,
        data: {
          title: "Edit Student",
        }
      },
      {
        path: "inquiry2",
        component: Inquiry2Component,
        data: {
          title: "Student Inquiry 2",
        }
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentRoutingModule { }
