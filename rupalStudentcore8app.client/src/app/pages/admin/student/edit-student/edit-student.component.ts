import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StudentShakhList } from 'app/shared/data/global-constant';
import { FileUploader } from 'ng2-file-upload';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { StudentService } from '../../_services/student.service';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'edit-student',
  templateUrl: './edit-student.component.html',
  styleUrls: ['./edit-student.component.scss']
})
export class EditStudentComponent implements OnInit {
  form: FormGroup;
  familyNameList = StudentShakhList;
  educationList: any[] = [];
  uploader: FileUploader;
  attachmentList: any[] = [];
  studentAttachmentList: any[] = [];
  studentId: number;


  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private fb: FormBuilder,
    private studentService: StudentService,
    private activatedRoute: ActivatedRoute,
    private router: Router
  ) {
    if (this.activatedRoute.snapshot.params) {
      this.studentId = Number(this.activatedRoute.snapshot.params.id) || 0;
    } else {
      this.studentId = 0;
    }
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      mobile: [null, Validators.required],
      familyName: [null, Validators.required],
      familyNameGu: [null],
      studentName: [null],
      studentNameGu: [null],
      fatherName: [null],
      fatherNameGu: [null],
      schoolName: [null, Validators.required],
      education: [null, Validators.required],
      percentage: [null, [
        Validators.min(0),
        Validators.max(2),
        Validators.pattern(/^(100(\.0{0,2})?|\d{1,2}(\.\d{1,2})?)$/),
      ]],
      sgpa: [null, [
        Validators.min(0),
        Validators.max(10),
        Validators.pattern(/^(10(\.0{0,2})?|\d{1}(\.\d{1,2})?)$/)
      ]],
      cgpa: [null, [
        Validators.min(0),
        Validators.max(10),
        Validators.pattern(/^(10(\.0{0,2})?|\d{1}(\.\d{1,2})?)$/)
      ]]
    });
    if (this.studentId > 0) {
      this.getById();
    }
  }

  showMarksError(): boolean {
    const percentage = this.form.get('percentage');
    const sgpa = this.form.get('sgpa');
    const cgpa = this.form.get('cgpa');

    const anyTouched = percentage?.touched || sgpa?.touched || cgpa?.touched;
    const allEmpty = !percentage?.value && !sgpa?.value && !cgpa?.value;

    return anyTouched && allEmpty;
  }

  onFamilyChange(e: any) {
    if (e) {
      this.form.get('familyNameGu')?.setValue(this.familyNameList.find(item => item.name === e.name).nameGU);
    }
  }

  getById() {
    this.spinner.show();
    this.studentService.getStudentMarkSheetById(this.studentId).pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        if (res) {
          this.form.patchValue({
            mobile: res.mobile,
            familyName: res.familyName,
            familyNameGu: res.familyNameGu,
            studentName: res.studentName,
            studentNameGu: res.studentNameGu,
            fatherName: res.fatherName,
            fatherNameGu: res.fatherNameGu,
            schoolName: res.schoolName,
            education: res.education,
            percentage: res.percentage,
            sgpa: res.sgpa,
            cgpa: res.cgpa
          });
          this.studentAttachmentList = res.attachmentList || [];
        }
      },
      error: (err: any) => {
        this.toastr.error(err, "Failed to get student details");
      }
    });
  }

  removeAttachment(item: any): void {
    console.log('item', item);
    const index = this.attachmentList.indexOf(item);
    if (index > -1) {
      this.attachmentList.splice(index, 1);
    }
  }

  onClick_Submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Enter valid form details.");
      return;
    }
    const formData = new FormData();
    formData.append('mobile', this.form.get('mobile')?.value);
    formData.append('familyName', this.form.get('familyName')?.value);
    formData.append('familyNameGu', this.form.get('familyNameGu')?.value || '');
    formData.append('studentName', this.form.get('studentName')?.value || '');
    formData.append('studentNameGU', this.form.get('studentNameGU')?.value || '');
    formData.append('fatherName', this.form.get('fatherName')?.value || '');
    formData.append('fatherNameGU', this.form.get('fatherNameGU')?.value || '');
    formData.append('schoolName', this.form.get('schoolName')?.value || '');
    formData.append('education', this.form.get('education')?.value || '');
    formData.append('educationGu', this.form.get('educationGu')?.value || '');
    formData.append('percentage', this.form.get('percentage')?.value || '');
    formData.append('sgpa', this.form.get('sgpa')?.value || '');
    formData.append('cgpa', this.form.get('cgpa')?.value || '');
    formData.append('id', this.studentId.toString());
    this.spinner.show();
    this.studentService.saveStudentMarkSheet(formData).pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        if (res) {
          this.toastr.success("Student details updated successfully.");
          this.router.navigate(['/reg-form/view/', res]);
        } else {
          this.toastr.error(res.message || "Failed to update student details.");
        }
      },
      error: (err: any) => {
        this.toastr.error(err || "Failed to update student details.");
      }
    });
  }
}
