import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  ApiURL = environment.ApiURL;

  constructor(private http: HttpClient) { }

  getStudentEducationList() {
    return this.http.get(this.ApiURL + "Student/StudentEducation/Name");
  }

  saveStudentMarkSheet(formData: any) {
    return this.http.post(this.ApiURL + "Student", formData);
  }

  getStudentMarkSheet(data: any) {
    return this.http.post(this.ApiURL + "Student/GetList", data);
  }

  getStudentMarkSheetById(id: number) {
    return this.http.get(this.ApiURL + "Student/" + id);
  }

  deleteAttachment(id: number) {
    return this.http.delete(this.ApiURL + "Student/" + id);
  }

  deleteStudentMarkSheet(id: number) {
    return this.http.delete(this.ApiURL + "Student/Student/" + id);
  }

  getStudentExportToExcel(filterData: any): Observable<any> {
    return this.http.post(this.ApiURL + "Student/ExportStudentList", filterData, {
      responseType: "arraybuffer",
    });
  }

  printStandardInvoice(data: any): Observable<any> {
    return this.http.post(this.ApiURL + "Student/ExportPDF", data, {
      responseType: 'blob'
    });
  }
}
