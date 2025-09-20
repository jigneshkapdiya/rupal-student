import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AdministrationService } from 'app/pages/admin/_services/administration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'system-permission-form',
  templateUrl: './system-permission-form.component.html',
  styleUrls: ['./system-permission-form.component.scss']
})
export class SystemPermissionFormComponent implements OnInit {
  @Input() menuId: number;
  @Input() permissionId: number;
  @Output() onPermission_Emit: EventEmitter<boolean> = new EventEmitter();

  form: FormGroup;

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private administrationService: AdministrationService
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      name: [null, Validators.compose([Validators.required])],
      description: [null, Validators.compose([Validators.required])],
      descriptionAr: [null, Validators.compose([Validators.required])],
    });
    if (this.permissionId > 0) {
      this.getPermissionById();
    }
  }

  get f() {
    return this.form.controls;
  }

  getPermissionById() {
    this.spinner.show('modalspin');
    this.administrationService.getPermissionById(this.permissionId).pipe(finalize(() => this.spinner.hide('modalspin'))).subscribe(
      (result) => {
        if (result) {
          this.form.patchValue({
            name: result.name,
            description: result.description,
            descriptionAr: result.descriptionAr
          });
        }
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }

  onSubmit_Permission() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Please enter valid form details");
      return;
    }
    const menuData = {
      id: this.permissionId,
      menuId: this.menuId,
      name: this.form.get('name').value,
      description: this.form.get('description').value,
      descriptionAr: this.form.get('descriptionAr').value,
    };
    this.spinner.show('modalspin');
    this.administrationService.addEditPermission(menuData).pipe(finalize(() => this.spinner.hide('modalspin'))).subscribe(
      (result) => {
        if (result) {
          if (this.permissionId > 0) {
            this.toastr.success("Permission updated successfully");
          } else {
            this.toastr.success("Permission saved successfully");
          }
          this.resetForm();
          this.onPermission_Emit.emit(result);
          this.activeModal.close();
        } else {
          this.toastr.error("Failed to save data");
        }
      },
      (error) => {
        this.toastr.error(error, "Failed to save data");
      }
    );
  }

  resetForm() {
    this.form.reset();
  }
}
