import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AdministrationService } from '../../_services/administration.service';
import { AddEditSystemMenuModalComponent } from './add-edit-system-menu-modal/add-edit-system-menu-modal.component';
import { SystemPermissionModalComponent } from './system-permission-modal/system-permission-modal.component';
import { swalDelete } from 'app/shared/data/global-constant';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'system-menu',
  templateUrl: './system-menu.component.html',
  styleUrls: ['./system-menu.component.scss']
})
export class SystemMenuComponent implements OnInit {
  form: FormGroup;

  dataList: any[] = [];
  parentMenuList: any[] = [];
  addPermission = true;
  editPermission = true;
  deletePermission = true;

  constructor(
    private administrationService: AdministrationService,
    private formBuilder: FormBuilder,
    private spinner: NgxSpinnerService,
    public toastr: ToastrService,
    private modalService: NgbModal,
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      parentId: [null],
    });
    this.form.get('parentId').valueChanges.subscribe(() => {
      this.getDataList();
    });
    this.getDataList();
    this.getParentMenus();
  }

  getDataList() {
    this.spinner.show();
    this.administrationService.getFilteredMenuList(this.form.get('parentId').value || 0).subscribe(
      (result) => {
        this.spinner.hide();
        this.dataList = result;
      },
      (error) => {
        this.spinner.hide();
        this.dataList = [];
        this.toastr.error(error);
      }
    );
  }

  getParentMenus() {
    this.administrationService.getParentMenus().subscribe(
      (result) => {
        this.parentMenuList = result;
      },
      (error) => {
        this.toastr.error(error);
        this.dataList = [];
      }
    );
  }

  onClick_AddEdit(menuId: number) {
    const modalRef = this.modalService.open(AddEditSystemMenuModalComponent, {
      centered: true,
      backdrop: "static",
    });
    modalRef.componentInstance.menuId = menuId;
    modalRef.componentInstance.parentId = this.form.get('parentId').value;
    modalRef.componentInstance.onMenu_Emit.subscribe((data) => {
      if (data != null) {
        this.getDataList();
        this.getParentMenus();
      }
    });
  }

  onClick_Permission(menuId: number, menuTitle: string) {
    const modalRef = this.modalService.open(SystemPermissionModalComponent, {
      centered: false,
      backdrop: "static",
      size: 'lg'
    });
    modalRef.componentInstance.menuId = menuId;
    modalRef.componentInstance.menuTitle = menuTitle;
  }

  onClick_Delete(menuId: any) {
    swalDelete.fire()
      .then((res) => {
        if (res.value) {
          this.spinner.show();
          this.administrationService.deleteMenu(menuId).pipe(finalize(() => this.spinner.hide())).subscribe(
            (result) => {
              if (result) {
                this.getDataList();
                this.getParentMenus();
                this.toastr.success('Menu deleted successfully');
              }
            },
            (error) => {
              this.toastr.error(error, 'Error deleting menu');
            }
          );
        }
      });
  }

  moveUp(menuData, index) {
    if (index > 0) {
      const formData = {
        menuId: menuData.id,
        parentId: menuData.parentId,
        newOrder: menuData.orderNo - 1,
        prevOrder: menuData.orderNo,
      };
      this.administrationService.changeOrder(formData).subscribe(
        (result) => {
          if (result) {
            this.getDataList();
          }
        },
        (error) => {
          this.toastr.error(error);
        }
      );
    }
  }

  moveDown(menuData, index) {
    if (index + 1 < this.dataList.length) {
      const formData = {
        menuId: menuData.id,
        parentId: menuData.parentId,
        newOrder: menuData.orderNo + 1,
        prevOrder: menuData.orderNo,
      };

      this.administrationService.changeOrder(formData).subscribe(
        (result) => {
          if (result) {
            this.getDataList();
          }
        },
        (error) => {
          this.toastr.error(error);
        }
      );
    }
  }
}
