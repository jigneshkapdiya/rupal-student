import { Component, OnInit } from '@angular/core';
import { StudentService } from 'app/pages/admin/_services/student.service';
import { StudentStatusList } from 'app/shared/data/global-constant';
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
  filteredDataList: any[] = [];
  searchText: string = '';
  selectedStatus: string = '';
  statusList = StudentStatusList;

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
        this.filteredDataList = [...this.dataList]; // Initialize filtered list
        this.applyFilters(); // Apply any existing filters
      },
      error: (err: any) => {
        this.spinner.hide();
        this.toastr.error(err, "Failed to get student inquiry list");
      }
    });
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  onStatusChange(): void {
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchText = '';
    this.applyFilters();
  }

  clearAllFilters(): void {
    this.searchText = '';
    this.selectedStatus = '';
    this.applyFilters();
  }

  private applyFilters(): void {
    let filtered = [...this.dataList];

    // Apply search filter
    if (this.searchText && this.searchText.trim()) {
      const searchTerm = this.searchText.toLowerCase().trim();
      filtered = filtered.filter(item => {
        return (
          (item.studentName && item.studentName.toLowerCase().includes(searchTerm)) ||
          (item.studentNameGu && item.studentNameGu.toLowerCase().includes(searchTerm)) ||
          (item.fatherName && item.fatherName.toLowerCase().includes(searchTerm)) ||
          (item.fatherNameGu && item.fatherNameGu.toLowerCase().includes(searchTerm)) ||
          (item.familyName && item.familyName.toLowerCase().includes(searchTerm)) ||
          (item.familyNameGu && item.familyNameGu.toLowerCase().includes(searchTerm)) ||
          (item.formNumber && item.formNumber.toLowerCase().includes(searchTerm)) ||
          (item.education && item.education.toLowerCase().includes(searchTerm)) ||
          (item.schoolName && item.schoolName.toLowerCase().includes(searchTerm))
        );
      });
    }
    // Apply status filter
    if (this.selectedStatus && this.selectedStatus.trim()) {
      filtered = filtered.filter(item => item.status === this.selectedStatus);
    }
    // Update the filtered data list
    if (!this.selectedStatus) {
      this.selectedStatus = null;
    }
    this.filteredDataList = filtered;
  }
}
