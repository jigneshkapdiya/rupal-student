import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from 'app/shared/auth/auth.service';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  ApiURL = environment.ApiURL;

  constructor(
    private http: HttpClient, private authService: AuthService
  ) { }

  login(formData: any): Observable<any> {
    return this.http.post(this.ApiURL + 'auth/login', formData);
  }

  login2FA(twoFactorData: any): Observable<any> {
    return this.http.post(this.ApiURL + 'auth/login-2fa', twoFactorData);
  }

  resend2FA_OTP(twoFactorData: any): Observable<any> {
    return this.http.post(this.ApiURL + 'auth/resend-2fa-otp', twoFactorData);
  }

  sendVerificationLink(email: any): Observable<any> {
    return this.http.post(this.ApiURL + 'Account/ForgetPassword', email);
  }

  resetPassword(formData: any): Observable<any> {
    return this.http.post(this.ApiURL + 'Account/ResetPassword', formData);
  }

  signUp(userData: any): Observable<any> {
    return this.http.post(this.ApiURL + "Account/Register", userData, {
    });
  }

  verifyOTP(otpData: any): Observable<any> {
    return this.http.post(this.ApiURL + 'Account/VerifyOTP', otpData)
  }

  resendOTP(): Observable<any> {
    return this.http.post(this.ApiURL + 'Account/ResendOTP', {});
  }
}
