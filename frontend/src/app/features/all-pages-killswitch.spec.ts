import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { apiToggleInterceptor } from '../core/http/api-toggle.interceptor';
import { ApiToggleService } from '../core/http/api-toggle.service';
import { DailyAttendanceLogsPageComponent } from './attendance/pages/daily-logs/daily-logs-page.component';
import { RosterMatrixPageComponent } from './attendance/pages/roster-matrix/roster-matrix-page.component';
import { ShiftPatternsPageComponent } from './attendance/pages/shift-patterns/shift-patterns-page.component';
import { EmployeeDetailPageComponent } from './employees/pages/employee-detail/employee-detail-page.component';
import { EmployeeListPageComponent } from './employees/pages/employee-list/employee-list-page.component';
import { EmployeeOnboardingPageComponent } from './employees/pages/employee-onboarding/employee-onboarding-page.component';
import { CompaniesBranchesPageComponent } from './organization/pages/companies-branches/companies-branches-page.component';
import { DepartmentsPositionsPageComponent } from './organization/pages/departments-positions/departments-positions-page.component';
import { PayrollPeriodsPageComponent } from './payroll/pages/payroll-periods/payroll-periods-page.component';
import { PayrollRunPageComponent } from './payroll/pages/payroll-run/payroll-run-page.component';
import { LeavesPageComponent } from './portal/pages/leaves/leaves-page.component';
import { OvertimePageComponent } from './portal/pages/overtime/overtime-page.component';
import { PayslipsPageComponent } from './portal/pages/payslips/payslips-page.component';
import { AuditLogsPageComponent } from './settings/pages/audit-logs/audit-logs-page.component';
import { RolesPermissionsPageComponent } from './settings/pages/roles-permissions/roles-permissions-page.component';
import { ApprovalInboxPageComponent } from './workflows/pages/approval-inbox/approval-inbox-page.component';
import { ApprovalRoutesPageComponent } from './workflows/pages/approval-routes/approval-routes-page.component';
import { LeavePoliciesPageComponent } from './workflows/pages/leave-policies/leave-policies-page.component';

interface KillSwitchTarget {
  name: string;
  component: Type<unknown>;
}

const ALL_NEW_PAGES: readonly KillSwitchTarget[] = [
  { name: 'LeavesPageComponent', component: LeavesPageComponent },
  { name: 'OvertimePageComponent', component: OvertimePageComponent },
  { name: 'PayslipsPageComponent', component: PayslipsPageComponent },
  { name: 'CompaniesBranchesPageComponent', component: CompaniesBranchesPageComponent },
  { name: 'DepartmentsPositionsPageComponent', component: DepartmentsPositionsPageComponent },
  { name: 'EmployeeListPageComponent', component: EmployeeListPageComponent },
  { name: 'EmployeeDetailPageComponent', component: EmployeeDetailPageComponent },
  { name: 'EmployeeOnboardingPageComponent', component: EmployeeOnboardingPageComponent },
  { name: 'ShiftPatternsPageComponent', component: ShiftPatternsPageComponent },
  { name: 'RosterMatrixPageComponent', component: RosterMatrixPageComponent },
  { name: 'DailyAttendanceLogsPageComponent', component: DailyAttendanceLogsPageComponent },
  { name: 'ApprovalInboxPageComponent', component: ApprovalInboxPageComponent },
  { name: 'LeavePoliciesPageComponent', component: LeavePoliciesPageComponent },
  { name: 'ApprovalRoutesPageComponent', component: ApprovalRoutesPageComponent },
  { name: 'PayrollPeriodsPageComponent', component: PayrollPeriodsPageComponent },
  { name: 'PayrollRunPageComponent', component: PayrollRunPageComponent },
  { name: 'RolesPermissionsPageComponent', component: RolesPermissionsPageComponent },
  { name: 'AuditLogsPageComponent', component: AuditLogsPageComponent },
];

describe('All 18 New Pages — API: OFF Kill-Switch Verification', () => {
  let httpMock: HttpTestingController;
  let apiToggleService: ApiToggleService;

  beforeEach(async () => {
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([apiToggleInterceptor])),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'emp-1', periodId: 'period-1' }),
            },
          },
        },
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    apiToggleService = TestBed.inject(ApiToggleService);
    apiToggleService.toggleApi(); // API: OFF
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  for (const target of ALL_NEW_PAGES) {
    it(`${target.name} enters clean Error State with zero fallback data when API is OFF`, () => {
      const fixture = TestBed.createComponent(target.component);
      fixture.detectChanges();

      const instance = fixture.componentInstance as { errorMessage: () => string | null };
      expect(instance.errorMessage()).toContain('ระบบจำลองสถานะขาดการเชื่อมต่อ API');
    });
  }

  it('LeavesPageComponent loads real balances and requests when API is ON', () => {
    apiToggleService.toggleApi(); // back ON

    const fixture = TestBed.createComponent(LeavesPageComponent);
    fixture.detectChanges();

    const balReq = httpMock.expectOne('/api/v1/leaves/my-balances');
    const reqReq = httpMock.expectOne('/api/v1/leaves/my-requests');

    balReq.flush([
      {
        id: 'b-1',
        leaveTypeId: 'lt-1',
        leaveTypeCode: 'ANNUAL',
        leaveTypeName: 'ลาพักร้อน',
        quotaYear: 2026,
        carriedOverDays: 0,
        entitledDays: 10,
        usedDays: 2,
        pendingDays: 1,
        remainingDays: 7,
      },
    ]);
    reqReq.flush([]);

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('ลาพักร้อน');
    expect(compiled.textContent).toContain('คงเหลือ 7 วัน');
  });

  it('AuditLogsPageComponent loads paged audit entries when API is ON', () => {
    apiToggleService.toggleApi(); // back ON

    const fixture = TestBed.createComponent(AuditLogsPageComponent);
    fixture.detectChanges();

    const req = httpMock.expectOne((r) => r.url === '/api/v1/audit-logs');
    req.flush({
      items: [
        {
          id: 'log-1',
          companyId: 'c-1',
          actorUserId: 'u-1',
          entityName: 'Employee',
          entityId: 'e-1',
          action: 'UPDATE',
          oldValues: '{"status":"PROBATION"}',
          newValues: '{"status":"ACTIVE"}',
          ipAddress: '127.0.0.1',
          correlationId: 'corr-1',
          occurredAt: '2026-11-20T02:00:00Z',
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Employee');
    expect(compiled.textContent).toContain('UPDATE');
  });
});
