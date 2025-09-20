import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  ApiURL = environment.ApiURL;

  constructor(private http: HttpClient) { }

  getUserById(): Observable<any> {
    return this.http.get(this.ApiURL + 'Profile', {
    });
  }

  editUser(formData: any): Observable<any> {
    return this.http.put(this.ApiURL + 'Profile', formData, {
    });
  }

  changePassword(data: any): Observable<any> {
    return this.http.post(this.ApiURL + 'Profile/ChangePassword', data, {
    });
  }

  disableTFA(userId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Profile/DisableTFA/" + userId);
  }
}
