import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StudentShakhList } from 'app/shared/data/global-constant';
import { FileUploader } from 'ng2-file-upload';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { StudentService } from '../../_services/student.service';
import { ActivatedRoute } from '@angular/router';
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
    private activatedRoute: ActivatedRoute
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

  onClick_Submit() { }
}
