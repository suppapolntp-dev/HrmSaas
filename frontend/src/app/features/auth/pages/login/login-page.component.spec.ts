import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { ApiToggleService } from '../../../../core/http/api-toggle.service';
import { apiToggleInterceptor } from '../../../../core/http/api-toggle.interceptor';
import { problemDetailsInterceptor } from '../../../../core/http/problem-details.interceptor';
import { LoginPageComponent } from './login-page.component';

describe('LoginPageComponent', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  let authSessionStore: AuthSessionStore;
  let apiToggleService: ApiToggleService;

  beforeEach(async () => {
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([apiToggleInterceptor, problemDetailsInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    authSessionStore = TestBed.inject(AuthSessionStore);
    apiToggleService = TestBed.inject(ApiToggleService);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should post credentials and navigate to dashboard for single-company user', () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    const component = fixture.componentInstance as unknown as {
      loginForm: { setValue: (v: { email: string; password: string }) => void };
      onSubmit: () => void;
    };
    fixture.detectChanges();

    component.loginForm.setValue({
      email: 'hr@demo.hrmsaas.local',
      password: 'Password123!',
    });
    component.onSubmit();

    const req = httpMock.expectOne('/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      email: 'hr@demo.hrmsaas.local',
      password: 'Password123!',
    });

    req.flush({
      userId: 'u-1',
      tenantId: 't-1',
      email: 'hr@demo.hrmsaas.local',
      employeeId: 'e-1',
      employeeCode: 'EMP-0002',
      fullName: 'พิมพ์ชนก รักษ์คน',
      activeCompanyId: 'c-1',
      activeBranchId: 'b-1',
      permissions: ['employee.read'],
      companies: [
        {
          companyId: 'c-1',
          code: 'DEMO',
          legalName: 'Demo Co., Ltd.',
          branches: [{ branchId: 'b-1', code: 'BKK', name: 'สำนักงานใหญ่' }],
        },
      ],
    });

    expect(authSessionStore.isAuthenticated()).toBe(true);
    expect(router.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
  });

  it('should toggle sign-up mode, post registration, and navigate to dashboard', () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    const component = fixture.componentInstance as unknown as {
      switchMode: (signUp: boolean) => void;
      registerForm: { setValue: (v: { name: string; email: string; password: string }) => void };
      onRegisterSubmit: () => void;
    };
    fixture.detectChanges();

    component.switchMode(true);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('#container')?.classList.contains('right-panel-active')).toBe(true);

    component.registerForm.setValue({
      name: 'กาย ทดสอบ',
      email: 'guy@demo.hrmsaas.local',
      password: 'Password123!',
    });
    component.onRegisterSubmit();

    const req = httpMock.expectOne('/api/v1/auth/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      name: 'กาย ทดสอบ',
      email: 'guy@demo.hrmsaas.local',
      password: 'Password123!',
    });

    req.flush({
      userId: 'u-2',
      tenantId: 't-1',
      email: 'guy@demo.hrmsaas.local',
      employeeId: 'e-2',
      employeeCode: 'EMP-0007',
      fullName: 'กาย ทดสอบ',
      activeCompanyId: 'c-1',
      activeBranchId: 'b-1',
      permissions: ['portal.attendance.read'],
      companies: [
        {
          companyId: 'c-1',
          code: 'DEMO',
          legalName: 'Demo Co., Ltd.',
          branches: [{ branchId: 'b-1', code: 'BKK', name: 'สำนักงานใหญ่' }],
        },
      ],
    });

    expect(authSessionStore.isAuthenticated()).toBe(true);
    expect(router.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
  });

  it('should display 503 error banner when API toggle is OFF', () => {
    apiToggleService.toggleApi();
    expect(apiToggleService.isApiEnabled()).toBe(false);

    const fixture = TestBed.createComponent(LoginPageComponent);
    const component = fixture.componentInstance as unknown as {
      loginForm: { setValue: (v: { email: string; password: string }) => void };
      onSubmit: () => void;
    };
    fixture.detectChanges();

    component.loginForm.setValue({
      email: 'hr@demo.hrmsaas.local',
      password: 'Password123!',
    });
    component.onSubmit();
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.error-banner')?.textContent).toContain('API: OFF');
    httpMock.expectNone('/api/v1/auth/login');
  });
});
