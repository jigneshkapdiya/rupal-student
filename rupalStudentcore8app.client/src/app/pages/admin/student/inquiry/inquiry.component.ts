import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { PageSize, PageSizeList, StudentStatusList, swalDelete } from 'app/shared/data/global-constant';
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
  statusList = StudentStatusList;
  dataList: any[] = [];

  totalRecord: number = 0;
  page: number = 1;
  pageSize = PageSize;
  pageSizeList = PageSizeList;

  isAscending: boolean = true;
  sortBy: string = '';

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

  sortData(column: string) {
    if (this.sortBy === column) {
      this.isAscending = !this.isAscending;
    } else {
      this.sortBy = column;
      this.isAscending = true;
    }
    this.getList();
  }

  getList() {
    const data = {
      searchText: this.form.get('searchText')?.value,
      status: this.form.get('status')?.value,
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,  // Column to sort by
      isAscending: this.isAscending,
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

  onDelete(id: number) {
    swalDelete.fire().then((result) => {
      if (result.value) {
        this.spinner.show();
        this.studentService.deleteStudentMarkSheet(id).pipe(finalize(() => this.spinner.hide()))
          .subscribe((res: any) => {
            if (res) {
              this.toastr.success("Deleted successfully.");
              this.getList();
            } else {
              this.toastr.error(res || "Failed to delete.");
            }
          }, (err) => {
            this.toastr.error(err, "Failed to delete.");
          });
      } else {
        this.spinner.hide();
      }
    });
  }

  exportToExcel() {
    const data = {
      searchText: this.form.get('searchText')?.value,
      status: this.form.get('status')?.value,
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,  // Column to sort by
      isAscending: this.isAscending,
    };
    this.spinner.show();
    this.studentService.getStudentExportToExcel(data).pipe(finalize(() => this.spinner.hide()))
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

  exportToPdf() {
    const data = {
      searchText: this.form.get('searchText')?.value,
      status: this.form.get('status')?.value,
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,  // Column to sort by
      isAscending: this.isAscending,
    };
    this.spinner.show();
    this.studentService.printStandardInvoice(data).pipe(finalize(() => this.spinner.hide()))
      .subscribe((res: any) => {
        const blob = new Blob([res], {
          type: "application/pdf",
        });
        const anchor = window.document.createElement("a");
        anchor.href = window.URL.createObjectURL(blob);
        anchor.download = "Student.pdf";
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(anchor.href);
      }, (err) => {
        this.toastr.error(err, "Failed to download.");
      });
  }
}
