import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { StudentService } from 'app/pages/admin/_services/student.service';
import { StudentShakhList } from 'app/shared/data/global-constant';
import { environment } from 'environments/environment';
import { FileItem, FileUploader } from 'ng2-file-upload';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'student-marksheet-form',
  templateUrl: './student-marksheet-form.component.html',
  styleUrls: ['./student-marksheet-form.component.scss']
})
export class StudentMarksheetFormComponent implements OnInit {
  form: FormGroup;
  familyNameList = StudentShakhList;
  educationList: any[] = [];
  uploader: FileUploader;
  attachmentList: any[] = [];
  currentYear: number = new Date().getFullYear();

  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private fb: FormBuilder,
    private studentService: StudentService,
    private router: Router,
  ) {
  }

  createUploader(targetList: any[]): FileUploader {
    const uploader = new FileUploader({
      url: environment.ApiURL + 'Attachment',
      disableMultipart: false,
      method: 'post',
      itemAlias: '',
    });

    uploader.onAfterAddingFile = (fileItem: FileItem) => {
      fileItem.withCredentials = false;
      targetList.push({
        fileName: fileItem.file.name,
        documentType: this.getFileExtension(fileItem.file.name),
        file: fileItem._file,
        isNew: true,
        fileItem: fileItem,
        fileUrl: null,
        description: ''
      });
    };
    return uploader;
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      mobile: [null, Validators.required],
      familyName: [null, Validators.required],
      familyNameGu: [null],
      studentName: [null],
      studentNameGU: [null],
      fatherName: [null],
      fatherNameGU: [null],
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

    this.uploader = this.createUploader(this.attachmentList);
    this.uploader.onAfterAddingFile = (fileItem) => {
      const fileObj = {
        fileUrl: fileItem._file,
        fileName: fileItem.file.name,
        description: ''
      };
      this.attachmentList.push(fileObj);
    };
    this.getStudentEducationList();
    // Setup validation for name fields
    this.setupAtLeastOneRequired('studentName', 'studentNameGU');
    this.setupAtLeastOneRequired('fatherName', 'fatherNameGU');

    // Setup validation for percentage, SGPA, and CGPA fields
    this.setupAtLeastOneRequiredMultiple(['percentage', 'sgpa', 'cgpa']);
  }

  setupAtLeastOneRequired(control1: string, control2: string) {
    const c1 = this.form.get(control1);
    const c2 = this.form.get(control2);
    if (!c1 || !c2) return;

    // Initially no validators
    c1.clearValidators();
    c2.clearValidators();

    const updateValidators = () => {
      const c1HasValue = c1.value && c1.value.trim() !== '';
      const c2HasValue = c2.value && c2.value.trim() !== '';

      // If both are empty, require at least one
      if (!c1HasValue && !c2HasValue) {
        c1.setValidators([Validators.required]);
        c2.setValidators([Validators.required]);
      } else {
        c1.clearValidators();
        c2.clearValidators();
      }

      c1.updateValueAndValidity({ emitEvent: false });
      c2.updateValueAndValidity({ emitEvent: false });
    };

    // Initial validation setup
    updateValidators();

    // Update validators when either control changes
    c1.valueChanges.subscribe(() => updateValidators());
    c2.valueChanges.subscribe(() => updateValidators());
  }

  setupAtLeastOneRequiredMultiple(controlNames: string[]) {
    const controls = controlNames.map(name => this.form.get(name)).filter(Boolean);
    if (controls.length === 0) return;

    const updateValidators = () => {
      const hasAnyValue = controls.some(control =>
        control.value !== null && control.value.toString().trim() !== ''
      );

      controls.forEach(control => {
        if (!hasAnyValue) {
          control.setValidators([Validators.required]);
        } else {
          control.clearValidators();
        }
        control.updateValueAndValidity({ emitEvent: false });
      });
    };

    // Initial validation setup
    updateValidators();

    // Update validators when any control changes
    controls.forEach(control => {
      control.valueChanges.subscribe(() => updateValidators());
    });
  }

  showMarksError(): boolean {
    const percentage = this.form.get('percentage');
    const sgpa = this.form.get('sgpa');
    const cgpa = this.form.get('cgpa');

    const anyTouched = percentage?.touched || sgpa?.touched || cgpa?.touched;
    const allEmpty = !percentage?.value && !sgpa?.value && !cgpa?.value;

    return anyTouched && allEmpty;
  }

  getStudentEducationList() {
    this.spinner.show();
    this.studentService.getStudentEducationList().pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        this.educationList = res;
      },
      error: (err) => {
        this.toastr.error('Error while fetching education list');
      }
    });
  }

  getFileExtension(fileName?: string): string {
    if (!fileName || !fileName.includes('.')) return '';
    return fileName.split('.').pop()?.toLowerCase() || '';
  }

  onFamilyChange(e: any) {
    if (e) {
      this.form.get('familyNameGu')?.setValue(this.familyNameList.find(item => item.name === e.name).nameGU);
    }
  }

  removeAttachment(item: any): void {
    const index = this.attachmentList.indexOf(item);
    if (index > -1) {
      this.attachmentList.splice(index, 1);
    }
  }

  onClick_Submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning('માન્ય ફોર્મ વિગતો દાખલ કરો.');
      return;
    }
    if (this.attachmentList.length < 1) {
      this.toastr.warning('ઓછામાં ઓછી એક ફાઇલ જરૂરી છે.');
      return;
    }
    this.spinner.show();
    const formData = new FormData();
    formData.append('mobile', this.form.get('mobile')?.value);
    formData.append('familyName', this.form.get('familyName')?.value);
    formData.append('familyNameGu', this.form.get('familyNameGu')?.value || '');
    formData.append('studentName', this.form.get('studentName')?.value);
    formData.append('studentNameGU', this.form.get('studentNameGU')?.value || '');
    formData.append('fatherName', this.form.get('fatherName')?.value);
    formData.append('fatherNameGU', this.form.get('fatherNameGU')?.value || '');
    formData.append('schoolName', this.form.get('schoolName')?.value);
    formData.append('education', this.form.get('education')?.value);
    formData.append('percentage', this.form.get('percentage')?.value || '');
    formData.append('sgpa', this.form.get('sgpa')?.value || '');
    formData.append('cgpa', this.form.get('cgpa')?.value || '');

    this.attachmentList.forEach((item, index) => {
      if (item.fileUrl) {
        formData.append(`Attachments[${index}].File`, item.fileUrl);
        formData.append(`Attachments[${index}].Description`, item.description || '');
        formData.append(`Attachments[${index}].ReferenceType`, 'Student');
      }
    });
    this.studentService.saveStudentMarkSheet(formData).pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        this.toastr.success('ફોર્મ સફળતાપૂર્વક સબમિટ થઇ ગયું .');
        this.router.navigate(['/home/confirmation/', + res]);
        this.form.reset();
        this.attachmentList = [];
        this.uploader.clearQueue();
      },
      error: (err) => {
        this.toastr.error('ફોર્મ સબમિટ કરતી વખતે ભૂલ આવી.');
      }
    });
  }
};
