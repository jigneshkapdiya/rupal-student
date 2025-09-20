import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AdministrationService } from 'app/pages/admin/_services/administration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'add-edit-system-roles',
  templateUrl: './add-edit-system-roles.component.html',
  styleUrls: ['./add-edit-system-roles.component.scss']
})
export class AddEditSystemRolesComponent implements OnInit {
  @Input() roleId: number;
  @Output() onRole_Emit: EventEmitter<boolean> = new EventEmitter();

  form: FormGroup;

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
    private administrationService: AdministrationService,
    public toastr: ToastrService,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      name: ["", Validators.compose([Validators.required])],
    });
    if (this.roleId > 0) {
      this.getRoleById();
    }
  }

  getRoleById() {
    this.spinner.show("modalspin");
    this.administrationService.getRoleById(this.roleId).pipe(finalize(() => this.spinner.hide("modalspin"))).subscribe(
      (result) => {
        if (result) {
          this.form.patchValue({
            name: result.name,
          });
        }
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }

  onSubmit_Form() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Enter valid details");
      return;
    }
    const data = {
      id: this.roleId,
      name: this.form.get('name')?.value,
    }
    this.spinner.show("modalspin");
    if (this.roleId > 0) {
      this.administrationService.editRole(data).pipe(finalize(() => this.spinner.hide("modalspin"))).subscribe(
        (result) => {
          if (result) {
            this.toastr.success("Role updated successfully");
            this.onRole_Emit.emit(true);
            this.activeModal.close();
          }
        },
        (error) => {
          this.toastr.error(error);
        }
      );
    } else {
      this.administrationService.addRole(data).pipe(finalize(() => this.spinner.hide("modalspin"))).subscribe(
        (result) => {
          if (result) {
            this.toastr.success("Role added successfully");
            this.onRole_Emit.emit(true);
            this.activeModal.close();
          }
        },
        (error) => {
          this.toastr.error(error);
        }
      );
    }
  }
}
