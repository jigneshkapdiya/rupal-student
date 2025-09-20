import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { StudentService } from 'app/pages/admin/_services/student.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'confirm-student-marksheet',
  templateUrl: './confirm-student-marksheet.component.html',
  styleUrls: ['./confirm-student-marksheet.component.scss']
})
export class ConfirmStudentMarksheetComponent implements OnInit {
  formNumber: string = '';
  studentId: number;
  studentDetails: any;

  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private studentService: StudentService,
    private activatedRoute: ActivatedRoute,
  ) {
    if (this.activatedRoute.snapshot.params) {
      this.studentId = Number(this.activatedRoute.snapshot.params.id) || 0;
    } else {
      this.studentId = 0;
    }
    this.getById();
  }

  ngOnInit(): void {
  }

  getById() {
    this.spinner.show();
    this.studentService.getStudentMarkSheetById(this.studentId).pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        if (res) {
          this.formNumber = res.formNumber;
          this.studentDetails = res;
        }
      },
      error: (err: any) => {
        this.toastr.error(err, "Failed to get student details");
      }
    });
  }


}
