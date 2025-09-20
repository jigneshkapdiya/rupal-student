import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';

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
    return this.http.post(this.ApiURL + "StudentMarkSheet/GetList", data);
  }

  getStudentMarkSheetById(id: number) {
    return this.http.get(this.ApiURL + "StudentMarkSheet/" + id);
  }
}
