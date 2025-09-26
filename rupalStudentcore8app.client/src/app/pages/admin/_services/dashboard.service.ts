import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  ApiURL = environment.ApiURL;

  constructor(
    private http: HttpClient
  ) { }

  getDashboardList(): Observable<any> {
    return this.http.get(this.ApiURL + "Dashboard/GetDashboardList");
  }

  getStudentEducationCount(): Observable<any> {
    return this.http.get(this.ApiURL + "Dashboard/GetEducationWithStudentCount");
  }
}
