import { Component, Input, OnInit } from '@angular/core';
import { AdministrationService } from 'app/pages/admin/_services/administration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { SystemPermissionFormComponent } from './system-permission-form/system-permission-form.component';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { swalDelete } from 'app/shared/data/global-constant';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'system-permission-modal',
  templateUrl: './system-permission-modal.component.html',
  styleUrls: ['./system-permission-modal.component.scss']
})
export class SystemPermissionModalComponent implements OnInit {
  @Input() menuId: number;
  @Input() menuTitle: string;

  dataList: any[] = [];
  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private administrationService: AdministrationService,
    private modalService: NgbModal,
    public activeModal: NgbActiveModal
  ) { }

  ngOnInit(): void {
    this.getDataList();
  }

  getDataList() {
    this.spinner.show()
    this.administrationService.getPermissionByMenuId(this.menuId).pipe(finalize(() => this.spinner.hide())).subscribe(
      (result) => {
        if (result) {
          this.dataList = result;
        }
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }

  onClick_AddEdit(permissionId: number) {
    const modalRef = this.modalService.open(SystemPermissionFormComponent, {
      centered: true,
      backdrop: "static",
    });
    modalRef.componentInstance.menuId = this.menuId;
    modalRef.componentInstance.permissionId = permissionId;
    modalRef.componentInstance.onPermission_Emit.subscribe((data) => {
      if (data != null) {
        this.getDataList();
      }
    });
  }

  onClick_Delete(permissionId: any) {
    swalDelete.fire()
      .then((res) => {
        if (res.value) {
          this.spinner.show();
          this.administrationService.deletePermission(permissionId).subscribe(
            (result) => {
              this.spinner.hide();
              if (result) {
                this.getDataList();
                this.toastr.success("Permission deleted successfully");
              }
            },
            (error) => {
              this.spinner.hide();
              this.toastr.error(error, "Failed to delete a record");
            }
          );
        }
      });
  }
}
