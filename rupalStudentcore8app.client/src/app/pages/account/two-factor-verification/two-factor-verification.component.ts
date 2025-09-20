import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from 'app/shared/auth/auth.service';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';

// Interface
export interface IAlert {
  type: 'success' | 'danger' | 'warning' | 'info';
  title?: string;
  message: string;
}

export interface ITwoFactorRequest {
  userId: string;
  twoFactorToken: string;
  twoFactorCode: string;
}

@Component({
  selector: 'two-factor-verification',
  templateUrl: './two-factor-verification.component.html',
  styleUrls: ['./two-factor-verification.component.scss']
})
export class TwoFactorVerificationComponent implements OnInit, OnDestroy {
  @ViewChild('otp0') otp0!: ElementRef<HTMLInputElement>;
  @ViewChild('otp1') otp1!: ElementRef<HTMLInputElement>;
  @ViewChild('otp2') otp2!: ElementRef<HTMLInputElement>;
  @ViewChild('otp3') otp3!: ElementRef<HTMLInputElement>;
  @ViewChild('otp4') otp4!: ElementRef<HTMLInputElement>;
  @ViewChild('otp5') otp5!: ElementRef<HTMLInputElement>;

  public form!: FormGroup;
  public alert = {} as IAlert;

  private readonly destroy$ = new Subject<void>();
  public readonly isLoading$ = new BehaviorSubject<boolean>(false);

  constructor(
    private readonly router: Router,
    private readonly accountService: AccountService,
    private readonly authService: AuthService,
    private readonly toastr: ToastrService,
    private readonly formBuilder: FormBuilder,
  ) { }

  ngOnInit(): void {
    this.initializeForm();
    this.validateTwoFactorState();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm(): void {
    this.form = this.formBuilder.group({
      twoFactorCode: ['', [
        Validators.required,
        Validators.pattern(/^\d{6}$/),
        Validators.minLength(6),
        Validators.maxLength(6)
      ]]
    });
  }

  private validateTwoFactorState(): void {
    const twoFactorToken = this.authService.getTwoFactorData();
    if (!twoFactorToken) {
      this.showAlert('warning', 'Session Expired!', 'Please login again to continue.');
      setTimeout(() => this.router.navigate(['/auth/login']), 2000);
    }
  }

  public closeAlert(): void {
    this.alert = {} as IAlert;
  }

  public onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.showAlert('warning', 'Validation Error!', 'Please enter a valid 6-digit verification code.');
      return;
    }
    const twoFactorData = this.authService.getTwoFactorData();

    const formData: ITwoFactorRequest = {
      userId: twoFactorData.userId || '',
      twoFactorToken: twoFactorData.token || '',
      twoFactorCode: this.form.get('twoFactorCode')?.value
    };

    if (!formData.userId || !formData.twoFactorToken) {
      this.showAlert('danger', 'Session Error!', 'Invalid session. Please login again.');
      this.router.navigate(['/auth/login']);
      return;
    }

    this.isLoading$.next(true);

    this.accountService.login2FA(formData)
      .pipe(
        finalize(() => this.isLoading$.next(false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (result: any) => this.handleVerificationSuccess(result),
        error: (error) => this.handleVerificationError(error)
      });
  }



  private handleVerificationSuccess(result: any): void {
    if (!result?.succeeded) {
      this.showAlert('danger', 'Verification Failed!', result?.message || 'Invalid verification code');
      this.toastr.error(result?.message || 'Invalid verification code', 'Verification Failed');
      return;
    }

    if (result.accessToken && result.refreshToken) {
      this.authService.setAccessToken(result.accessToken);
      this.authService.setRefreshToken(result.refreshToken);
      this.authService.removeTwoFactorData();

      this.showAlert('success', 'Verification Success!', 'You are being redirected to dashboard.');
      this.toastr.success('Two-factor authentication completed successfully!', 'Success');

      setTimeout(() => this.router.navigate(['/dashboard']), 1500);
      return;
    }

    this.showAlert('danger', 'Verification Failed!', 'Invalid response from server');
  }

  private handleVerificationError(error: any): void {
    console.error('2FA Verification Error:', error);
    const errorMessage = error || 'Verification failed. Please try again.';
    this.showAlert('danger', 'Verification Failed!', errorMessage);
    this.toastr.error(errorMessage, 'Verification Failed');
  }

  private showAlert(type: IAlert['type'], title: string, message: string): void {
    this.alert = { type, title, message };
  }

  public resendCode(): void {
    const twoFactorData = this.authService.getTwoFactorData();

    const formData: ITwoFactorRequest = {
      userId: twoFactorData.userId || '',
      twoFactorToken: twoFactorData.token || '',
      twoFactorCode: '' // Not needed for resend
    };

    if (!formData.userId || !formData.twoFactorToken) {
      this.showAlert('danger', 'Session Error!', 'Invalid session. Please login again.');
      this.router.navigate(['/auth/login']);
      return;
    }

    this.isLoading$.next(true);

    this.accountService.resend2FA_OTP(formData)
      .pipe(
        finalize(() => this.isLoading$.next(false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (result: any) => {
          if (result?.succeeded) {
            this.showAlert('success', 'Code Resent!', 'A new verification code has been sent to your device.');
            this.toastr.success('A new verification code has been sent to your device.', 'Code Resent');
          } else {
            this.showAlert('warning', 'Resend Failed!', result?.message || 'Failed to resend verification code.');
            this.toastr.warning(result?.message || 'Failed to resend verification code.', 'Resend Failed');
          }
        },
        error: (error) => {
          console.error('2FA Resend Error:', error);
          const errorMessage = error || 'Failed to resend verification code. Please try again.';
          this.showAlert('danger', 'Resend Failed!', errorMessage);
          this.toastr.error(errorMessage, 'Resend Failed');
        }
      });
  }

  /**
   * Handles OTP input for individual digit boxes
   */
  public onOtpInput(event: any, index: number): void {
    const input = event.target;
    const value = input.value;

    // Skip validation if input contains multiple characters (from paste)
    if (value.length > 1) {
      return; // Let paste handler deal with it
    }

    // Only allow numeric input for single character
    if (value && !/^\d$/.test(value)) {
      input.value = '';
      return;
    }

    // Update the form control with combined OTP
    this.updateOtpFormControl();

    // Auto-focus next input
    if (value && index < 5) {
      const nextInput = document.querySelector(`input.otp-input:nth-child(${index + 2})`) as HTMLInputElement;
      if (nextInput) {
        nextInput.focus();
      }
    }
  }

  /**
   * Handles keydown events for OTP inputs
   */
  public onOtpKeyDown(event: KeyboardEvent, index: number): void {
    const input = event.target as HTMLInputElement;

    // Allow paste operations (Ctrl+V)
    if (event.ctrlKey && event.key === 'v') {
      return; // Let paste handler deal with it
    }

    // Handle backspace
    if (event.key === 'Backspace') {
      if (!input.value && index > 0) {
        // Move to previous input if current is empty
        const prevInput = document.querySelector(`input.otp-input:nth-child(${index})`) as HTMLInputElement;
        if (prevInput) {
          prevInput.focus();
        }
      }
    }
    // Handle arrow keys
    else if (event.key === 'ArrowLeft' && index > 0) {
      const prevInput = document.querySelector(`input.otp-input:nth-child(${index})`) as HTMLInputElement;
      if (prevInput) {
        prevInput.focus();
      }
    }
    else if (event.key === 'ArrowRight' && index < 5) {
      const nextInput = document.querySelector(`input.otp-input:nth-child(${index + 2})`) as HTMLInputElement;
      if (nextInput) {
        nextInput.focus();
      }
    }
    // Prevent non-numeric input (but allow paste and other control keys)
    else if (!/^\d$/.test(event.key) && !['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'Control'].includes(event.key)) {
      event.preventDefault();
    }
  }

  /**
   * Handles paste event for OTP inputs
   */
  public onOtpPaste(event: ClipboardEvent): void {
    event.preventDefault();
    console.log('onOtpPaste');
    const pastedData = event.clipboardData?.getData('text') || '';
    const digits = pastedData.replace(/\D/g, '').slice(0, 6);
    console.log('pastedData', pastedData);
    console.log('digits', digits);
    if (digits.length > 0) {
      // Fill the OTP inputs with pasted digits using template references
      const inputs = [
        this.otp0?.nativeElement,
        this.otp1?.nativeElement,
        this.otp2?.nativeElement,
        this.otp3?.nativeElement,
        this.otp4?.nativeElement,
        this.otp5?.nativeElement
      ];

      for (let i = 0; i < 6; i++) {
        if (inputs[i]) {
          inputs[i].value = digits[i] || '';
        }
      }

      // Update form control
      this.updateOtpFormControl();

      // Focus the next empty input or the last one
      const nextEmptyIndex = digits.length < 6 ? digits.length : 5;
      if (inputs[nextEmptyIndex]) {
        inputs[nextEmptyIndex].focus();
      }
    }
  }

  /**
   * Updates the form control with the combined OTP value
   */
  private updateOtpFormControl(): void {
    const inputs = [
      this.otp0?.nativeElement,
      this.otp1?.nativeElement,
      this.otp2?.nativeElement,
      this.otp3?.nativeElement,
      this.otp4?.nativeElement,
      this.otp5?.nativeElement
    ];

    let otpValue = '';
    for (let i = 0; i < 6; i++) {
      if (inputs[i]) {
        otpValue += inputs[i].value || '';
      }
    }
    this.form.patchValue({ twoFactorCode: otpValue });
    this.form.get('twoFactorCode')?.markAsTouched();
  }
}
