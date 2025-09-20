import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { PageSize, PageSizeList } from 'app/shared/data/global-constant';
import { StudentService } from '../../_services/student.service';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'inquiry',
  templateUrl: './inquiry.component.html',
  styleUrls: ['./inquiry.component.scss']
})
export class InquiryComponent implements OnInit {
  form: FormGroup;
  statusList: any[] = [];
  dataList: any[] = [];

  totalRecord: number = 0;
  page: number = 1;
  pageSize = PageSize;
  pageSizeList = PageSizeList;

  constructor(private formBuilder: FormBuilder,
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
      pageSize: this.pageSize
    };
    this.spinner.show();
    this.studentService.getStudentMarkSheet(data).pipe(finalize(() => this.spinner.hide()))
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

}
