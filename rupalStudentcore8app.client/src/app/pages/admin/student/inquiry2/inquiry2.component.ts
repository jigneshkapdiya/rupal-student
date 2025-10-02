import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { StudentStatusList, PageSize, PageSizeList } from 'app/shared/data/global-constant';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { StudentService } from '../../_services/student.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'inquiry2',
  templateUrl: './inquiry2.component.html',
  styleUrls: ['./inquiry2.component.scss']
})
export class Inquiry2Component implements OnInit {
  form: FormGroup;
  statusList = StudentStatusList;
  dataList: any[] = [];

  totalRecord: number = 0;
  page: number = 1;
  pageSize = PageSize;
  pageSizeList = PageSizeList;

  isAscending: boolean = true;
  sortBy: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private studentService: StudentService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService
  ) { }


  ngOnInit(): void {
    this.form = this.formBuilder.group({
      searchText: [null],
      status: [null],
    });
    this.getList();
  }

  getList() {
    const data = {
      searchText: this.form.get('searchText')?.value,
      status: this.form.get('status')?.value,
      page: this.page,
      pageSize: this.pageSize,
    };
    this.spinner.show();
    this.studentService.getStudentMarkSheet2(data).pipe(finalize(() => this.spinner.hide()))
      .subscribe((res: any) => {
        if (res) {
          this.dataList = res.dataList;
          this.totalRecord = res.totalRecord;
        } else {
          this.toastr.error("Failed to get data.");
        }
      }, (err) => {
        this.toastr.error(err, "Failed to get data.");
      });
  }

  onFilter() {
    this.getList();
  }

  onClearFilter() {
    this.form.reset();
    this.getList();
  }

  onPageChange(e) {
    this.page = e;
    this.getList();
  }

  onClick_PageChange(e: any) {
    this.page = e;
    this.getList();
  }

  onChange_PageSize() {
    this.getList();
  }

  getStartIndex(): number {
    return this.totalRecord === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  getEndIndex(): number {
    const endIndex = this.page * this.pageSize;
    return endIndex > this.totalRecord ? this.totalRecord : endIndex;
  }

  exportToExcel() {
    const data = {
      searchText: this.form.get('searchText')?.value,
      status: this.form.get('status')?.value,
      page: this.page,
      pageSize: this.pageSize,
    };
    this.spinner.show();
    this.studentService.getStudentExportToExcel2(data).pipe(finalize(() => this.spinner.hide()))
      .subscribe((res: any) => {
        const blob = new Blob([res], {
          type: "application/xlsx",
        });
        const anchor = window.document.createElement("a");
        anchor.href = window.URL.createObjectURL(blob);
        anchor.download = "Student.xlsx";
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(anchor.href);
      }, (err) => {
        this.toastr.error(err, "Failed to download.");
      });
  }
}
