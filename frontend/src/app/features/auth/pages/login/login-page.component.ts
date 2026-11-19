import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthMeResponse } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { extractProblemDetails } from '../../../../core/http/http-error.helper';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly isSignUpMode = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly showLoginPassword = signal(false);
  protected readonly showRegisterPassword = signal(false);
  protected readonly errorTitle = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly infoMessage = signal<string | null>(null);

  protected readonly loginForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected readonly registerForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected switchMode(signUpMode: boolean): void {
    this.isSignUpMode.set(signUpMode);
    this.clearFeedback();
  }

  protected onToggleLoginPassword(): void {
    this.showLoginPassword.set(!this.showLoginPassword());
  }

  protected onToggleRegisterPassword(): void {
    this.showRegisterPassword.set(!this.showRegisterPassword());
  }

  protected onSocialNotice(provider: string): void {
    this.errorTitle.set(null);
    this.errorMessage.set(null);
    this.infoMessage.set(`การเข้าสู่ระบบผ่าน ${provider} จะเปิดให้บริการในเวอร์ชันถัดไป กรุณาใช้อีเมลของระบบแทน`);
  }

  protected onForgotPassword(): void {
    this.errorTitle.set(null);
    this.errorMessage.set(null);
    this.infoMessage.set('ระบบรีเซ็ตรหัสผ่านอัตโนมัติกำลังพัฒนา กรุณาติดต่อฝ่ายบุคคล (HR) หรือผู้ดูแลระบบ');
  }

  protected onSubmit(): void {
    this.clearFeedback();

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    const { email, password } = this.loginForm.getRawValue();

    this.authService
      .login({ email: email.trim(), password })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (me) => this.handleAuthSuccess(me),
        error: (err: unknown) => this.handleAuthError(err, this.loginForm),
      });
  }

  protected onRegisterSubmit(): void {
    this.clearFeedback();

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    const { name, email, password } = this.registerForm.getRawValue();

    this.authService
      .register({
        name: name.trim(),
        email: email.trim(),
        password,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (me) => this.handleAuthSuccess(me),
        error: (err: unknown) => this.handleAuthError(err, this.registerForm),
      });
  }

  private handleAuthSuccess(me: AuthMeResponse): void {
    this.isSubmitting.set(false);
    if (!me.activeCompanyId || me.companies.length > 1) {
      void this.router.navigate(['/auth/select-company']);
      return;
    }

    void this.router.navigate(['/portal/dashboard']);
  }

  private handleAuthError(err: unknown, targetForm: FormGroup): void {
    this.isSubmitting.set(false);
    const problem = extractProblemDetails(err);
    const unmapped = applyProblemDetailsErrors(problem, targetForm);

    this.errorTitle.set(problem.title);
    if (unmapped.length > 0) {
      this.errorMessage.set(unmapped.join(' '));
    } else {
      this.errorMessage.set(problem.detail ?? 'กรุณาตรวจสอบข้อมูลที่กรอกแล้วลองใหม่อีกครั้ง');
    }
  }

  private clearFeedback(): void {
    this.errorTitle.set(null);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
  }
}
