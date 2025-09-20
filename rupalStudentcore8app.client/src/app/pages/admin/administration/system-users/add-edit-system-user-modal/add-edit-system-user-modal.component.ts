import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AdministrationService } from 'app/pages/admin/_services/administration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'add-edit-system-user-modal',
  templateUrl: './add-edit-system-user-modal.component.html',
  styleUrls: ['./add-edit-system-user-modal.component.scss']
})
export class AddEditSystemUserModalComponent implements OnInit {
  @Input() id: number;
  @Output() onUser_Emit: EventEmitter<boolean> = new EventEmitter();

  form: FormGroup;

  roleList: any[] = [];
  showPassword = false;

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
    private administrationService: AdministrationService,
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      roleName: [null, Validators.compose([Validators.required])],
      fullName: [null, Validators.compose([Validators.required])],
      fullNameAr: [null],
      userName: [null, Validators.compose([Validators.required])],
      email: [null, Validators.compose([Validators.required, Validators.email])],
      phoneNumber: [null, Validators.compose([Validators.required, Validators.minLength(9), Validators.maxLength(15)])],
      password: [null],
    });
    if (this.id > 0) {
      this.getUserById();
    }
    this.getRoleList();
  }

  getRoleList() {
    this.spinner.show('modalspin');
    this.administrationService.getRoleList().pipe(finalize(() => { this.spinner.hide('modalspin') })).subscribe(
      (result) => {
        this.roleList = result;
      },
      (error) => {
        this.roleList = [];
        this.toastr.error(error, 'Failed to get role list.');
      }
    );
  }

  getUserById() {
    this.spinner.show('modalspin');
    this.administrationService.getUserById(this.id).pipe(finalize(() => { this.spinner.hide('modalspin') })).subscribe(
      (result) => {
        if (result) {
          this.form.patchValue({
            roleName: result.roleName,
            fullName: result.fullName,
            fullNameAr: result.fullNameAr,
            userName: result.userName,
            email: result.email,
            phoneNumber: result.phoneNumber,
            status: result.status
          });
        } else {
          this.toastr.error('Failed to get user details.');
        }
      },
      (error) => {
        this.toastr.error(error, 'Failed to get user details.');
      }
    );
  }

  onClick_TogglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit_Form() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Enter valid form details.");
      return;
    }
    const data = {
      id: this.id,
      roleName: this.form.get('roleName')?.value,
      fullName: this.form.get('fullName')?.value,
      fullNameAr: this.form.get('fullNameAr')?.value,
      userName: this.form.get('userName')?.value,
      email: this.form.get('email')?.value,
      phoneNumber: this.form.get('phoneNumber')?.value,
      password: this.form.get('password')?.value,
    }
    this.spinner.show("modalspin")
    if (this.id > 0) {
      this.administrationService.editUser(data).pipe(finalize(() => { this.spinner.hide("modalspin") })).subscribe({
        next: (res: any) => {
          this.toastr.success("User updated successfully.");
          this.onUser_Emit.emit(res);
          this.activeModal.close();
        },
        error: (error: any) => {
          this.toastr.error(error?.error?.message || "Failed to update user.");
        }
      });
    }
    else {
      this.administrationService.addUser(data).pipe(finalize(() => { this.spinner.hide("modalspin") })).subscribe({
        next: (res: any) => {
          this.toastr.success("User added successfully.");
          this.onUser_Emit.emit(res);
          this.activeModal.close();
        },
        error: (error: any) => {
          this.toastr.error(error?.error?.message || "Failed to add user.");
        }
      });
    }
  }
}
