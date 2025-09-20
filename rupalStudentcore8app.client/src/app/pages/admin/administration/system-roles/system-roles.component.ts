import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AdministrationService } from '../../_services/administration.service';
import { finalize } from 'rxjs/operators';
import { AddEditSystemRolesComponent } from './add-edit-system-roles/add-edit-system-roles.component';
import { swalDelete } from 'app/shared/data/global-constant';

@Component({
  selector: 'system-roles',
  templateUrl: './system-roles.component.html',
  styleUrls: ['./system-roles.component.scss']
})
export class SystemRolesComponent implements OnInit {
  roleList: any[] = [];

  constructor(
    private administrationService: AdministrationService,
    private modalService: NgbModal,
    public toastr: ToastrService,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit(): void {
    this.getRoleList();
  }

  getRoleList() {
    this.spinner.show();
    this.administrationService.getRoleList().pipe(finalize(() => this.spinner.hide())).subscribe(
      (result) => {
        if (result) {
          this.roleList = result;
        }
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }

  onClick_AddEdit(id: any) {
    const modalRef = this.modalService.open(AddEditSystemRolesComponent, {
      centered: true,
      backdrop: "static",
    });
    modalRef.componentInstance.roleId = id;
    modalRef.componentInstance.onRole_Emit.subscribe((data) => {
      if (data != null) {
        this.getRoleList();
      }
    });
  }

  onClick_Delete(roleId: any) {
    swalDelete.fire()
      .then((res) => {
        if (res.value) {
          this.spinner.show();
          this.administrationService.deleteRole(roleId).pipe(finalize(() => this.spinner.hide())).subscribe(
            (result) => {
              if (result) {
                this.getRoleList();
                this.toastr.success("Role deleted successfully");
              }
            },
            (error) => {
              this.toastr.error(error);
            }
          );
        }
      });
  }
}
