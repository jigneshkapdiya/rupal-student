import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from 'app/shared/auth/auth.service';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { ForgotPasswordModalComponent } from './forgot-password-modal/forgot-password-modal.component';
import { BehaviorSubject, Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { PASSWORD_PATTERN } from 'app/shared/data/global-constant';

// Interface
export interface IAlert {
  type: 'success' | 'danger' | 'warning' | 'info';
  title?: string;
  message: string;
}

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  public form!: FormGroup;
  public alert = {} as IAlert;
  public showPassword = false;

  private unsubscribe: Subscription[] = [];
  public isLoading$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor(
    private router: Router,
    private modalService: NgbModal,
    private accountService: AccountService,
    private authService: AuthService,
    private toastr: ToastrService,
    private formBuilder: FormBuilder,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
      rememberMe: [false],
    });
  }

  ngOnDestroy(): void {
    this.unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  get f() {
    return this.form.controls;
  }

  public closeAlert(): void {
    this.alert = {} as IAlert;
  }

  public togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  public onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.showAlert('warning', 'Validation Error!', 'Please fill in all required fields correctly.');
      return;
    }

    const formData = {
      username: this.form.get('username')?.value,
      password: this.form.get('password')?.value,
    };

    this.isLoading$.next(true);

    const loginSub = this.accountService.login(formData)
      .pipe(finalize(() => this.isLoading$.next(false)))
      .subscribe({
        next: (result: any) => {
          this.handleLoginSuccess(result);
          this.cdr.detectChanges(); // 🔑 Force Angular to refresh bindings
        },
        error: (error) => this.handleLoginError(error)
      });

    this.unsubscribe.push(loginSub);
  }

  private handleLoginSuccess(result: any): void {
    if (!result?.succeeded) {
      this.showAlert('danger', 'Login Failed!', result?.message || 'Invalid response from server');
      return;
    }

    // Handle Two-Factor Authentication Required
    if (result.userId && result.requiresTwoFactor && result.twoFactorToken) {
      this.authService.setTwoFactorData(result.userId, result.twoFactorToken);
      this.showAlert('success', 'Login Success!', 'Please complete two-factor authentication to continue.');
      this.toastr.success('Please complete two-factor authentication to continue.', 'Login Success');
      this.router.navigate(['/auth/verify-2fa']);
      return;
    }

    // Handle Normal Login (Two-Factor Disabled)
    if (result.accessToken && result.refreshToken) {
      this.authService.setAccessToken(result.accessToken);
      this.authService.setRefreshToken(result.refreshToken);
      this.showAlert('success', 'Login Success!', 'You are being redirected to dashboard.');
      this.toastr.success('You are being redirected to dashboard.', 'Login Success');
      this.router.navigate(['/dashboard']);
      return;
    }

    // Handle unexpected response
    this.showAlert('danger', 'Login Failed!', 'Invalid response from server');
  }

  private handleLoginError(error: any): void {
    console.log('handleLoginError', error);
    const errorMessage = error?.error?.message || error?.error || error?.message || 'Login failed. Please try again.';
    this.showAlert('danger', 'Login Failed!', errorMessage);
    this.toastr.error(errorMessage, 'Login Failed');
  }

  private showAlert(type: IAlert['type'], title: string, message: string): void {
    this.alert = { type, title, message };
  }

  public openModal(): void {
    const modalRef = this.modalService.open(ForgotPasswordModalComponent, {
      centered: true,
      backdrop: 'static',
    });
  }
}
