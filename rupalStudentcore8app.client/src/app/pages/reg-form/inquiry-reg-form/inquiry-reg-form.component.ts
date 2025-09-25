import { Component, OnInit } from '@angular/core';
import { StudentService } from 'app/pages/admin/_services/student.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'inquiry-reg-form',
  templateUrl: './inquiry-reg-form.component.html',
  styleUrls: ['./inquiry-reg-form.component.scss']
})
export class InquiryRegFormComponent implements OnInit {
  dataList: any[] = [];

  constructor(
    private studentService: StudentService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.getDataList();
  }

  getDataList() {
    this.spinner.show();
    this.studentService.getStudentInquiryList().pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        this.dataList = res || [];
      },
      error: (err: any) => {
        this.spinner.hide();
        this.toastr.error(err, "Failed to get student inquiry list");
      }
    });
  }

}
