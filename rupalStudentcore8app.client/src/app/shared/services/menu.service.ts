import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  ApiURL = environment.ApiURL;

  constructor(private http: HttpClient, private authService: AuthService) { }

  getHeaders(): HttpHeaders {
    return new HttpHeaders({
      "Content-Type": "application/json",
      Authorization: "Bearer " + this.authService.getAccessToken(),
      Lang: this.authService.getLanguage(),
    });
  }

  getMenuList(): Observable<any> {
    return this.http.get(this.ApiURL + 'Permission/GetMenuList', { headers: this.getHeaders() });
  }
}
