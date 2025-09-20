import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { StudentMarksheetFormComponent } from "./student-marksheet-form/student-marksheet-form.component";
import { ConfirmStudentMarksheetComponent } from "./confirm-student-marksheet/confirm-student-marksheet.component";
import { ViewMarkSheetDetailsComponent } from "./view-mark-sheet-details/view-mark-sheet-details.component";


const routes: Routes = [
  {
    path: "",
    children: [
      {
        path: '',
        component: StudentMarksheetFormComponent,
        data: {
          title: 'Student Marksheet Form',
        }
      },
      {
        path: 'confirmation/:id',
        component: ConfirmStudentMarksheetComponent,
        data: {
          title: 'Student Marksheet Confirmation',
        }
      },
      {
        path: 'view/:id',
        component: ViewMarkSheetDetailsComponent,
        data: {
          title: 'Student Marksheet View',
        }
      },
    ],
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HomePagesRoutingModule { }
