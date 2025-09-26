import { Component, OnInit } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { DashboardService } from '../_services/dashboard.service';

@Component({
  selector: 'dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  studentCountList: any;
  educationList: any;
  allStudents: number = 0;
  approvedStudents: number = 0;
  rejectedStudents: number = 0;
  pendingStudents: number = 0;

  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private dashboardService: DashboardService,
  ) { }

  ngOnInit(): void {
    this.getStudentEducationList();
    this.getDashboardList();
  }

  getStudentEducationList() {
    this.spinner.show();
    this.dashboardService.getStudentEducationCount().pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        this.studentCountList = res;
      },
      error: (err) => {
        this.toastr.error(err, 'Error while fetching education list');
      }
    });
  }

  getDashboardList() {
    this.spinner.show();
    this.dashboardService.getDashboardList().pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        this.allStudents = res.allStudents;
        this.approvedStudents = res.approvedStudents;
        this.rejectedStudents = res.rejectedStudents;
        this.pendingStudents = res.pendingStudents;
      },
      error: (err) => {
        this.toastr.error(err, 'Error while fetching student list');
      }
    });
  }
}
