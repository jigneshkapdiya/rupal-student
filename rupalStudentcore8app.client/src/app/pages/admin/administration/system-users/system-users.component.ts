import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AddEditSystemUserModalComponent } from './add-edit-system-user-modal/add-edit-system-user-modal.component';
import { PageSize, PageSizeList, swalDelete } from 'app/shared/data/global-constant';
import { AdministrationService } from '../../_services/administration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import swal from "sweetalert2";
import { ChangePasswordModalComponent } from './change-password-modal/change-password-modal.component';

@Component({
  selector: 'system-users',
  templateUrl: './system-users.component.html',
  styleUrls: ['./system-users.component.scss']
})
export class SystemUsersComponent implements OnInit {
  form: FormGroup;
  roleList: any[] = [];
  dataList: any[] = [];
  arabic: boolean = false;

  page = 1;
  pageSize = PageSize;
  pageSizeList = PageSizeList;
  totalRecords: number = 0;

  constructor(
    private modalService: NgbModal,
    private formBuilder: FormBuilder,
    private administrationService: AdministrationService,
    private spinner: NgxSpinnerService,
    public toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      roleId: [null],
      searchText: [null],
    });
    this.getDataList();
    this.getRoleList();
  }

  getRoleList() {
    this.spinner.show();
    this.administrationService.getRoleList().pipe(finalize(() => { this.spinner.hide(); })).subscribe(
      (result) => {
        this.roleList = result;
      },
      (error) => {
        this.roleList = [];
        this.toastr.error(error, 'Failed to get role list.');
      }
    );
  }

  getDataList() {
    const filterData = {
      page: this.page,
      pageSize: this.pageSize,
      searchText: this.form.get('searchText').value || '',
      roleId: this.form.get('roleId').value || 0,
    };
    this.spinner.show();
    this.administrationService.getUserList(filterData).pipe(finalize(() => this.spinner.hide())).subscribe(
      (result) => {
        if (result) {
          this.dataList = result.dataList;
          this.totalRecords = result.totalRecord;
        } else {
          this.dataList = [];
          this.totalRecords = 0;
        }
      },
      (error) => {
        this.spinner.hide();
        this.dataList = [];
        this.totalRecords = 0;
        this.toastr.error(error, "Failed to get dataList");
      }
    );
  }

  onClick_AddEdit(id: number) {
    const modalRef = this.modalService.open(AddEditSystemUserModalComponent, {
      centered: true,
      backdrop: "static",
      size: "lg"
    });
    modalRef.componentInstance.id = id;
    modalRef.componentInstance.onUser_Emit.subscribe((data) => {
      if (data != null) {
        this.getDataList();
      }
    });
  }

  onClick_Status(event: Event, id: any, status: boolean) {
    const checkbox = event.target as HTMLInputElement;
    const newStatus = checkbox.checked;

    swal.fire({
      title: "Are you sure?",
      icon: "warning",
      text: newStatus ? "You want to active user" : "You want to inactive user",
      confirmButtonText: newStatus ? "Yes, Active it" : "Yes, Inactive it",
      cancelButtonText: "No, Keep it",
      showCancelButton: true,
      showCloseButton: true,
      customClass: {
        confirmButton: "btn btn-success m-1",
        cancelButton: "btn btn-secondary m-1",
      },
      buttonsStyling: false,
    }).then((result) => {
      if (result.isConfirmed) {
        this.spinner.show();
        this.administrationService.activeInactiveStatus(id).subscribe(
          (res) => {
            this.spinner.hide();
            if (res) {
              this.toastr.success(
                newStatus ? "User is active successfully." : "User is inactive successfully.",
                "Success"
              );
              this.getDataList();
            } else {
              // rollback if API returns false
              checkbox.checked = status;
            }
          },
          (error) => {
            this.spinner.hide();
            this.toastr.error(error, "Failed to update status");
            // rollback on error
            checkbox.checked = status;
          }
        );
      } else {
        // rollback if cancelled
        checkbox.checked = status;
      }
    });
  }

  onClick_Disable2FA(id: number, ActivationStatus: boolean) {
    const data = {
      status: ActivationStatus,
      id: id
    };
    swal.fire({
      title: "Are you sure?",
      icon: "warning",
      text: data.status ? "You want to disable 2FA" : "You want to enable 2FA",
      confirmButtonText: data.status ? "Yes, Disable it" : "Yes, Enable it",
      cancelButtonText: "No, Keep it",
      showCancelButton: true,
      showCloseButton: true,
      customClass: {
        confirmButton: "btn btn-success m-1",
        cancelButton: "btn btn-secondary m-1",
      },
      buttonsStyling: false,
    }).then((result) => {
      if (result.isConfirmed) {
        this.spinner.show();
        this.administrationService.user2FA(id).pipe(finalize(() => this.spinner.hide())).subscribe(
          (result) => {
            if (result) {
              this.toastr.success(
                data.status ? "Two-Factor Authenticator is disabled successfully." : "Two-Factor Authenticator is enabled successfully.", "Success"
              );
              this.getDataList();
            }
          },
          (error) => {
            this.spinner.hide();
            this.toastr.error(error, "Failed to update Two-Factor Authenticator");
          }
        );
      }
    });
  }

  onClick_ChangePassword(id: any) {
    const modalRef = this.modalService.open(ChangePasswordModalComponent, {
      centered: true,
      backdrop: "static",
    });
    modalRef.componentInstance.id = id;
    modalRef.componentInstance.onUser_Emit.subscribe((data) => {
      if (data != null) {
        this.getDataList();
      }
    });
  }

  onClick_Submit() {
    this.getDataList();
  }

  onClearFilter() {
    this.form.reset();
    this.page = 0;
    this.getDataList();
  }

  getStartIndex(): number {
    return this.totalRecords === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  getEndIndex(): number {
    const endIndex = this.page * this.pageSize;
    return endIndex > this.totalRecords ? this.totalRecords : endIndex;
  }

  onClick_PageChange(e) {
    this.page = e;
    this.getDataList();
  }

  onChange_PageSize() {
    this.getDataList();
  }

  onClick_Delete(userId: any) {
    swalDelete.fire()
      .then((res) => {
        if (res.value) {
          this.spinner.show();
          this.administrationService.deleteUser(userId).pipe(finalize(() => this.spinner.hide())).subscribe(
            (result) => {
              if (result) {
                this.getDataList();
                this.toastr.success("User deleted successfully.");
              }
            },
            (error) => {
              this.toastr.error(error, "Failed to delete a record");
            }
          );
        }
      });
  }
}
