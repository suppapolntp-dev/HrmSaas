import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { SelectCompanyPageComponent } from './select-company-page.component';

describe('SelectCompanyPageComponent', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  let authSessionStore: AuthSessionStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectCompanyPageComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    authSessionStore = TestBed.inject(AuthSessionStore);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    authSessionStore.setSession({
      userId: 'u-1',
      tenantId: 't-1',
      email: 'admin@demo.hrmsaas.local',
      employeeId: 'e-1',
      employeeCode: 'EMP-0001',
      fullName: 'สมชาย บริหารกิจ',
      activeCompanyId: 'c-1',
      activeBranchId: 'b-1',
      permissions: ['org.branch.manage'],
      companies: [
        {
          companyId: 'c-1',
          code: 'COMP-A',
          legalName: 'Company A Co., Ltd.',
          branches: [{ branchId: 'b-1', code: 'HQ', name: 'สำนักงานใหญ่' }],
        },
        {
          companyId: 'c-2',
          code: 'COMP-B',
          legalName: 'Company B Co., Ltd.',
          branches: [{ branchId: 'b-2', code: 'CNX', name: 'เชียงใหม่' }],
        },
      ],
    });
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should switch company context and navigate to portal dashboard', () => {
    const fixture = TestBed.createComponent(SelectCompanyPageComponent);
    const component = fixture.componentInstance as unknown as {
      onSelectCompany: (id: string) => void;
      onContinue: () => void;
    };
    fixture.detectChanges();

    component.onSelectCompany('c-2');
    component.onContinue();

    const req = httpMock.expectOne('/api/v1/auth/switch-context');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      companyId: 'c-2',
      branchId: 'b-2',
    });

    req.flush({
      ...authSessionStore.user()!,
      activeCompanyId: 'c-2',
      activeBranchId: 'b-2',
    });

    expect(authSessionStore.activeCompanyId()).toBe('c-2');
    expect(authSessionStore.activeBranchId()).toBe('b-2');
    expect(router.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
  });
});
