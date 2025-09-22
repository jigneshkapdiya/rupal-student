import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { StudentMarkSheetComponent } from "./student-mark-sheet/student-mark-sheet.component";
import { ViewStudentMarkSheetComponent } from "./view-student-mark-sheet/view-student-mark-sheet.component";

const routes: Routes = [
  {
    path: "",
    children: [
      {
        path: '',
        component: StudentMarkSheetComponent,
        data: {
          title: 'Student Marksheet Form',
        }
      },
      {
        path: 'view/:id',
        component: ViewStudentMarkSheetComponent,
        data: {
          title: 'Student Marksheet Confirmation',
        }
      },
    ],
  }
];


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class RegFormPagesRoutingModule { }
