import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { StudentSemesterList, StudentShakhList, StudentStatusList } from 'app/shared/data/global-constant';
import { environment } from 'environments/environment';
import { FileItem, FileUploader } from 'ng2-file-upload';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize, mapTo } from 'rxjs/operators';
import swal from "sweetalert2";
import { StudentService } from '../../_services/student.service';

@Component({
  selector: 'edit-student',
  templateUrl: './edit-student.component.html',
  styleUrls: ['./edit-student.component.scss']
})
export class EditStudentComponent implements OnInit {
  form: FormGroup;
  familyNameList = StudentShakhList;
  semesterList = StudentSemesterList
  educationList: any[] = [];
  uploader: FileUploader;;
  attachmentList: any[] = [];
  studentAttachmentList: any[] = [];
  studentId: number;
  status: any;
  formNumber: string; // Static form number for now
  statusList = StudentStatusList;
  isGeneratingSequence: boolean = false;

  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private fb: FormBuilder,
    private studentService: StudentService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
  ) {
    if (this.activatedRoute.snapshot.params) {
      this.studentId = Number(this.activatedRoute.snapshot.params.id) || 0;
    } else {
      this.studentId = 0;
    }
    this.getStudentEducationList();
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

      const allowedExtensions = ['pdf', 'jpg', 'jpeg', 'png'];
      const fileExtension = this.getFileExtension(fileItem.file.name);

      if (!allowedExtensions.includes(fileExtension)) {
        this.toastr.warning('ફક્ત PDF, JPG, JPEG અને PNG ફાઇલોની જ મંજૂરી છે.');
        uploader.removeFromQueue(fileItem);
        return;
      }
      // Add to attachment list for display
      const newAttachment = {
        id: Date.now(), // Temporary ID for new files
        fileName: fileItem.file.name,
        documentType: this.getFileExtension(fileItem.file.name),
        file: fileItem._file,
        isNew: true,
        fileItem: fileItem,
        fileUrl: null,
        description: '',
        referenceType: 'Document'
      };
      targetList.push(newAttachment);
    };

    // Handle file validation errors from ng2-file-upload
    uploader.onWhenAddingFileFailed = (item: any, filter: any, options: any) => {
      switch (filter.name) {
        case 'fileType':
          this.toastr.warning(
            'ફક્ત PDF, JPG, JPEG અને PNG ફાઇલોની જ મંજૂરી છે.'
          );
          break;
        case 'mimeType':
          this.toastr.warning(
            'ફક્ત PDF અને છબી ફાઇલોની જ મંજૂરી છે.'
          );
          break;
        default:
          this.toastr.warning(
            'ફાઇલ અપલોડ કરવામાં ભૂલ આવી.'
          );
      }
    };

    return uploader;
  }

  getFileExtension(fileName?: string): string {
    if (!fileName || !fileName.includes('.')) return '';
    return fileName.split('.').pop()?.toLowerCase() || '';
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
      educationGu: [null],
      percentage: [null],
      semester: [null],
      grade: [null],
      sgpa: [null, [
        Validators.min(0),
        Validators.max(10),
        Validators.pattern(/^(10(\.0{0,2})?|\d{1}(\.\d{1,2})?)$/)
      ]],
      cgpa: [null, [
        Validators.min(0),
        Validators.max(10),
        Validators.pattern(/^(10(\.0{0,2})?|\d{1}(\.\d{1,2})?)$/)
      ]],
      sequenceNumber: [null], // Sequence number for approved students
      status: [null], // Status field
      description: [null] // Description field
    });

    // Add conditional validation and auto-generation for sequence number
    this.form.get('isApproved')?.valueChanges.subscribe(isApproved => {
      const sequenceNumberControl = this.form.get('sequenceNumber');
      if (isApproved) {
        // Auto-generate sequence number if not already set
        if (!sequenceNumberControl?.value) {
          this.generateSequenceNumber();
        }
        sequenceNumberControl?.setValidators([Validators.required, Validators.min(1)]);
      } else {
        sequenceNumberControl?.clearValidators();
        sequenceNumberControl?.setValue(null);
      }
      sequenceNumberControl?.updateValueAndValidity();
    });

    this.uploader = this.createUploader(this.attachmentList);
    if (this.studentId > 0) {
      this.getById();
    }
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

  onEducationChange(e: any) {
    if (e) {
      this.form.get('educationGu')?.setValue(this.educationList.find(item => item.name === e.name).nameGu);
    }
  }

  isValidFileType(fileName: string): boolean {
    const allowedExtensions = ['pdf', 'jpg', 'jpeg', 'png'];
    const fileExtension = this.getFileExtension(fileName);
    return allowedExtensions.includes(fileExtension);
  }

  showMarksError(): boolean {
    const percentage = this.form.get('percentage');
    const sgpa = this.form.get('sgpa');
    const cgpa = this.form.get('cgpa');

    const anyTouched = percentage?.touched || sgpa?.touched || cgpa?.touched;
    const allEmpty = !percentage?.value && !sgpa?.value && !cgpa?.value;

    return anyTouched && allEmpty;
  }

  shouldShowSemester(): boolean {
    const education = this.form.get('education')?.value;
    if (!education) return false;

    // List of education values that are Standard 12 or below
    const schoolLevelEducations = ['KG',
      'Standard 1', 'Standard 2', 'Standard 3', 'Standard 4', 'Standard 5',
      'Standard 6', 'Standard 7', 'Standard 8', 'Standard 9', 'Standard 10',
      'Standard 11', 'Standard 12'
    ];

    if (schoolLevelEducations.includes(education)) {
      this.form.get('semester')?.setValue(null);
    }
    // Show semester only if education is NOT in the school level list
    return !schoolLevelEducations.includes(education);

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
          this.formNumber = res.formNumber; // Static for now if not provided
          let sequenceNumber = null;
          if (res.sequenceNumber !== null && res.sequenceNumber !== undefined && res.sequenceNumber !== '') {
            sequenceNumber = Number(res.sequenceNumber);
            if (isNaN(sequenceNumber)) {
              sequenceNumber = null;
            }
          }
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
            cgpa: res.cgpa,
            semester: res.semester,
            sequenceNumber: sequenceNumber,
            status: res.status,
            description: res.description,
            grade: res.grade
          });
          this.status = res.status;
          this.studentAttachmentList = res.attachmentList || [];
        }
      },
      error: (err: any) => {
        this.toastr.error(err, "Failed to get student details");
      }
    });
  }

  removeAttachment(id: any) {
    swal
      .fire({
        title: "Are You Sure?",
        text: "You will not be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, delete it!",
        cancelButtonText: "Cancel",
        customClass: {
          confirmButton: "btn btn-danger m-1",
          cancelButton: "btn btn-secondary m-1",
        },
        buttonsStyling: false,
      })
      .then((result) => {
        if (result.isConfirmed) {
          this.spinner.show();
          this.studentService.deleteAttachment(id).pipe(finalize(() => this.spinner.hide())).subscribe({
            next: (res: any) => {
              if (res) {
                this.toastr.success("Attachment deleted successfully.");
                this.studentAttachmentList = this.studentAttachmentList.filter(x => x.id !== id);
              } else {
                this.toastr.error(res || "Failed to delete attachment.");
              }
            },
            error: (err: any) => {
              this.toastr.error(err || "Failed to delete attachment.");
            }
          });
        }
      });
  }

  onClick_Submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toastr.warning("Enter valid form details.");
      return;
    }

    const formData = new FormData();

    // Main student fields matching StudentViewModel
    formData.append('Id', this.studentId.toString());
    formData.append('Mobile', this.form.get('mobile')?.value || '');
    formData.append('FamilyName', this.form.get('familyName')?.value || '');
    formData.append('FamilyNameGu', this.form.get('familyNameGu')?.value || '');
    formData.append('StudentName', this.form.get('studentName')?.value || '');
    formData.append('StudentNameGu', this.form.get('studentNameGu')?.value || '');
    formData.append('FatherName', this.form.get('fatherName')?.value || '');
    formData.append('FatherNameGu', this.form.get('fatherNameGu')?.value || '');
    formData.append('SchoolName', this.form.get('schoolName')?.value || '');
    formData.append('Education', this.form.get('education')?.value || '');
    formData.append('EducationGu', this.form.get('educationGu')?.value || '');
    formData.append('Percentage', this.form.get('percentage')?.value || '');
    formData.append('Sgpa', this.form.get('sgpa')?.value || '');
    formData.append('Cgpa', this.form.get('cgpa')?.value || '');
    formData.append('Semester', this.form.get('semester')?.value || '');
    formData.append('SequenceNumber', this.form.get('sequenceNumber')?.value || '');
    formData.append('Status', this.form.get('status')?.value || '');
    formData.append('Description', this.form.get('description')?.value || '');
    formData.append('Grade', this.form.get('grade')?.value || '');
    let attachmentIndex = 0;
    this.studentAttachmentList.forEach((item) => {
      formData.append(`Attachments[${attachmentIndex}].FileName`, item.fileName || '');
      formData.append(`Attachments[${attachmentIndex}].FileUrl`, item.fileUrl || '');
      formData.append(`Attachments[${attachmentIndex}].ReferenceType`, item.referenceType || 'Student');
      formData.append(`Attachments[${attachmentIndex}].Description`, item.description || '');
      attachmentIndex++;
    });

    // Add new attachments (from attachmentList)
    this.attachmentList.forEach((item) => {
      if (item.file) {
        formData.append(`Attachments[${attachmentIndex}].File`, item.file, item.fileName);
        formData.append(`Attachments[${attachmentIndex}].FileName`, item.fileName || '');
        formData.append(`Attachments[${attachmentIndex}].ReferenceType`, 'Student');
        formData.append(`Attachments[${attachmentIndex}].Description`, item.description || '');
        attachmentIndex++;
      }
    });
    this.spinner.show();
    this.studentService.saveStudentMarkSheet(formData).pipe(finalize(() => this.spinner.hide())).subscribe({
      next: (res: any) => {
        if (res) {
          this.toastr.success("Student details updated successfully.");
          this.router.navigate(['/student/inquiry']);
          this.attachmentList = [];
          if (this.studentId > 0) {
            this.getById();
          }
        } else {
          this.toastr.error(res, "Failed to update student details.");
        }
      },
      error: (err: any) => {
        this.toastr.error(err, "Failed to update student details.");
      }
    });
  }

  getCurrentDate(): string {
    return new Date().toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  openFile(event: Event, fileUrl: string, fileName: string): void {
    event.preventDefault();
    if (fileUrl) {
      window.open(fileUrl, '_blank');
    } else {
      this.toastr.warning('File URL not available for preview');
    }
  }

  removeNewAttachment(item: any): void {
    const index = this.attachmentList.findIndex(att => att.id === item.id);
    if (index > -1) {
      // Remove from uploader queue if it exists
      if (item.fileItem) {
        this.uploader.removeFromQueue(item.fileItem);
      }
      // Remove from attachmentList
      this.attachmentList.splice(index, 1);
      this.toastr.success('Attachment removed successfully');
    }
  }

  generateSequenceNumber(): void {
    this.isGeneratingSequence = true;

    // Use timestamp-based sequence generation for simplicity
    setTimeout(() => {
      const now = new Date();
      const year = now.getFullYear();

      // Generate sequence: YYYY + current date/time based number
      // Format: YYYY + MMDD + sequential number based on time
      const dateStr = (now.getMonth() + 1).toString().padStart(2, '0') +
        now.getDate().toString().padStart(2, '0');
      const timeStr = now.getHours().toString().padStart(2, '0') +
        now.getMinutes().toString().padStart(2, '0');

      const sequenceNumber = Number(`${year}${dateStr}${timeStr.slice(-2)}`);

      this.form.get('sequenceNumber')?.setValue(sequenceNumber);
      this.toastr.success(`Sequence number ${sequenceNumber} generated successfully`);
      this.isGeneratingSequence = false;
    }, 500); // Small delay to show loading state
  }
}
