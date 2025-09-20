import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { InquiryComponent } from "./inquiry/inquiry.component";
import { EditStudentComponent } from "./edit-student/edit-student.component";

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
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentRoutingModule { }
