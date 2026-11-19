import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { apiToggleInterceptor } from '../../../../core/http/api-toggle.interceptor';
import { ApiToggleService } from '../../../../core/http/api-toggle.service';
import { PortalDashboardPageComponent } from './portal-dashboard-page.component';

describe('PortalDashboardPageComponent', () => {
  let httpMock: HttpTestingController;
  let authSessionStore: AuthSessionStore;
  let apiToggleService: ApiToggleService;

  beforeEach(async () => {
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [PortalDashboardPageComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([apiToggleInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    authSessionStore = TestBed.inject(AuthSessionStore);
    apiToggleService = TestBed.inject(ApiToggleService);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  function setDemoSession(permissions: string[]): void {
    authSessionStore.setSession({
      userId: 'u-1',
      tenantId: 't-1',
      email: 'operation@demo.hrmsaas.local',
      employeeId: 'e-1',
      employeeCode: 'EMP-0005',
      fullName: 'กิตติศักดิ์ คุมสาขา',
      activeCompanyId: 'c-1',
      activeBranchId: 'b-1',
      permissions,
      companies: [
        {
          companyId: 'c-1',
          code: 'COMP-HQ',
          legalName: 'บริษัท เอชอาร์เอ็ม ซาส เดโม จำกัด',
          branches: [{ branchId: 'b-1', code: 'BR-BKK', name: 'สำนักงานใหญ่ กรุงเทพฯ' }],
        },
      ],
    });
  }

  it('should load employee portal sections without approvals when user lacks workflow.approve', () => {
    setDemoSession([
      'portal.attendance.read',
      'portal.leave.read',
      'portal.overtime.read',
      'portal.payslip.read',
    ]);

    const fixture = TestBed.createComponent(PortalDashboardPageComponent);
    fixture.detectChanges();

    const todayReq = httpMock.expectOne('/api/v1/attendance/my-today');
    const rosterReq = httpMock.expectOne('/api/v1/rosters/my-upcoming?days=7');
    const leaveReq = httpMock.expectOne('/api/v1/leaves/my-balances');
    const summaryReq = httpMock.expectOne('/api/v1/dashboard/my-summary');
    httpMock.expectNone('/api/v1/approvals/pending');
    httpMock.expectNone('/api/v1/dashboard/team-pulse');
    httpMock.expectNone('/api/v1/dashboard/hr-overview');
    httpMock.expectNone('/api/v1/dashboard/payroll-readiness');

    todayReq.flush({
      workDate: '2026-11-19',
      branchName: 'สำนักงานใหญ่ กรุงเทพฯ',
      branchTimezone: 'Asia/Bangkok',
      dayType: 'WORKDAY',
      shiftDefinitionId: 's-1',
      shiftCode: 'SHIFT-DAY-09',
      shiftName: 'กะเช้าปกติ',
      shiftStartTime: '09:00:00',
      shiftEndTime: '18:00:00',
      isOvernight: false,
      actualInUtc: null,
      actualOutUtc: null,
      workedMinutes: 0,
      lateMinutes: 0,
      earlyOutMinutes: 0,
      otMinutes: 0,
      attendanceStatus: 'NOT_CHECKED_IN',
      isLockedByPayroll: false,
    });

    rosterReq.flush([
      {
        id: 'r-1',
        workDate: '2026-11-19',
        dayType: 'WORKDAY',
        shiftDefinitionId: 's-1',
        shiftCode: 'SHIFT-DAY-09',
        shiftName: 'กะเช้าปกติ',
        startTime: '09:00:00',
        endTime: '18:00:00',
        isOvernight: false,
      },
    ]);

    leaveReq.flush([
      {
        id: 'lb-1',
        leaveTypeId: 'lt-1',
        leaveTypeCode: 'ANNUAL',
        leaveTypeName: 'ลาพักร้อนประจำปี',
        quotaYear: 2026,
        carriedOverDays: 2,
        entitledDays: 6,
        usedDays: 1,
        pendingDays: 0,
        remainingDays: 7,
      },
    ]);

    summaryReq.flush({
      recentMissingPunchDate: null,
      recentRequests: [],
      latestPayslip: {
        payslipId: 'ps-1',
        payrollPeriodId: 'pp-1',
        periodCode: '2026-11',
        payDate: '2026-11-30',
        grossIncome: 45000,
        totalDeductions: 2250,
        netPay: 42750,
        isLocked: true,
      },
    });

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('ลงเวลาเข้างาน (Check-in)');
    expect(compiled.textContent).toContain('ลาพักร้อนประจำปี');
    expect(compiled.textContent).toContain('กะเช้าปกติ');
    expect(compiled.textContent).toContain('สลิปเงินเดือนล่าสุด');
    expect(compiled.textContent).toContain('••••••');
    expect(compiled.textContent).not.toContain('รายการคำขอรออนุมัติ');
    expect(compiled.textContent).not.toContain('ภาพรวมทีมและองค์กร');
  });

  it('should enter error state with zero fallback data when API toggle is turned OFF', () => {
    setDemoSession([
      'portal.attendance.read',
      'portal.leave.read',
      'workflow.approve',
    ]);

    const fixture = TestBed.createComponent(PortalDashboardPageComponent);
    fixture.detectChanges();

    httpMock.expectOne('/api/v1/attendance/my-today').flush({
      workDate: '2026-11-19',
      branchName: 'สำนักงานใหญ่ กรุงเทพฯ',
      branchTimezone: 'Asia/Bangkok',
      dayType: 'WORKDAY',
      shiftDefinitionId: 's-1',
      shiftCode: 'SHIFT-DAY-09',
      shiftName: 'กะเช้าปกติ',
      shiftStartTime: '09:00:00',
      shiftEndTime: '18:00:00',
      isOvernight: false,
      actualInUtc: null,
      actualOutUtc: null,
      workedMinutes: 0,
      lateMinutes: 0,
      earlyOutMinutes: 0,
      otMinutes: 0,
      attendanceStatus: 'NOT_CHECKED_IN',
      isLockedByPayroll: false,
    });
    httpMock.expectOne('/api/v1/rosters/my-upcoming?days=7').flush([]);
    httpMock.expectOne('/api/v1/leaves/my-balances').flush([]);
    httpMock.expectOne('/api/v1/dashboard/my-summary').flush({
      recentMissingPunchDate: null,
      recentRequests: [],
      latestPayslip: null,
    });
    httpMock.expectOne('/api/v1/approvals/pending').flush([]);
    fixture.detectChanges();

    // Flip API kill-switch OFF: effect() triggers reload and apiToggleInterceptor returns 503
    apiToggleService.toggleApi();
    fixture.detectChanges();

    httpMock.expectNone('/api/v1/attendance/my-today');
    httpMock.expectNone('/api/v1/rosters/my-upcoming?days=7');
    httpMock.expectNone('/api/v1/leaves/my-balances');
    httpMock.expectNone('/api/v1/approvals/pending');
    httpMock.expectNone('/api/v1/dashboard/my-summary');

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('API ถูกปิดสำหรับการทดสอบ (API: OFF)');
    expect(compiled.textContent).not.toContain('กะเช้าปกติ');
  });

  it('should submit check-in and approve pending workflow item for manager', () => {
    setDemoSession([
      'portal.attendance.read',
      'portal.leave.read',
      'workflow.approve',
    ]);

    const fixture = TestBed.createComponent(PortalDashboardPageComponent);
    const component = fixture.componentInstance as unknown as {
      onCheckIn: () => Promise<void>;
      onOpenApproveDialog: (item: {
        id: string;
        requesterEmployeeId: string;
        requesterEmployeeCode: string;
        requesterFullName: string;
        requestType: string;
        referenceId: string;
        currentStepOrder: number;
        status: string;
        payloadSummary: string;
        submittedAt: string;
        completedAt: string | null;
      }) => void;
      onConfirmApprovalDecision: () => void;
    };
    fixture.detectChanges();

    httpMock.expectOne('/api/v1/attendance/my-today').flush({
      workDate: '2026-11-19',
      branchName: 'สำนักงานใหญ่ กรุงเทพฯ',
      branchTimezone: 'Asia/Bangkok',
      dayType: 'WORKDAY',
      shiftDefinitionId: null,
      shiftCode: null,
      shiftName: null,
      shiftStartTime: null,
      shiftEndTime: null,
      isOvernight: false,
      actualInUtc: null,
      actualOutUtc: null,
      workedMinutes: 0,
      lateMinutes: 0,
      earlyOutMinutes: 0,
      otMinutes: 0,
      attendanceStatus: 'NOT_CHECKED_IN',
      isLockedByPayroll: false,
    });
    httpMock.expectOne('/api/v1/rosters/my-upcoming?days=7').flush([]);
    httpMock.expectOne('/api/v1/leaves/my-balances').flush([]);
    httpMock.expectOne('/api/v1/dashboard/my-summary').flush({
      recentMissingPunchDate: null,
      recentRequests: [],
      latestPayslip: null,
    });

    const pendingItem = {
      id: 'app-1',
      requesterEmployeeId: 'emp-6',
      requesterEmployeeCode: 'EMP-0006',
      requesterFullName: 'ณัฐพงษ์ ตั้งใจงาน',
      requestType: 'LEAVE',
      referenceId: 'ref-1',
      currentStepOrder: 1,
      status: 'PENDING',
      payloadSummary: '{"note":"ขอลาพักร้อน 1 วัน"}',
      submittedAt: '2026-11-19T02:00:00Z',
      completedAt: null,
    };
    httpMock.expectOne('/api/v1/approvals/pending').flush([pendingItem]);
    fixture.detectChanges();

    component.onOpenApproveDialog(pendingItem);
    component.onConfirmApprovalDecision();

    const decideReq = httpMock.expectOne('/api/v1/approvals/app-1/decide');
    expect(decideReq.request.method).toBe('POST');
    expect(decideReq.request.body).toEqual({ action: 'APPROVE', comment: null });
    decideReq.flush({ ...pendingItem, status: 'APPROVED' });

    const refreshApprovals = httpMock.expectOne('/api/v1/approvals/pending');
    refreshApprovals.flush([]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('ไม่มีคำขอรออนุมัติในขณะนี้');
  });

  it('should render team and org overview tab when user has management permissions', () => {
    setDemoSession([
      'portal.attendance.read',
      'attendance.log.read',
      'employee.read',
      'payroll.period.manage',
    ]);

    const fixture = TestBed.createComponent(PortalDashboardPageComponent);
    const component = fixture.componentInstance as unknown as {
      setActiveTab: (tab: 'MY_WORKSPACE' | 'TEAM_ORG') => void;
    };
    fixture.detectChanges();

    httpMock.expectOne('/api/v1/attendance/my-today').flush(null);
    httpMock.expectOne('/api/v1/rosters/my-upcoming?days=7').flush([]);
    httpMock.expectOne('/api/v1/dashboard/my-summary').flush({
      recentMissingPunchDate: null,
      recentRequests: [],
      latestPayslip: null,
    });
    httpMock.expectOne('/api/v1/dashboard/team-pulse').flush({
      workDate: '2026-11-19',
      totalScheduledCount: 12,
      presentCount: 9,
      lateCount: 1,
      missingPunchCount: 1,
      absentCount: 0,
      onLeaveCount: 1,
      exceptions: [],
      whoIsOut: [],
    });
    httpMock.expectOne('/api/v1/dashboard/hr-overview').flush({
      activeCount: 25,
      probationCount: 3,
      newHiresThisMonthCount: 2,
      departmentBreakdown: [],
      lifecycleAlerts: [],
    });
    httpMock.expectOne('/api/v1/dashboard/payroll-readiness').flush({
      periodId: 'pp-1',
      periodCode: '2026-11',
      periodStart: '2026-11-01',
      periodEnd: '2026-11-30',
      attendanceCutoffDate: '2026-11-25',
      payDate: '2026-11-30',
      status: 'DRAFT',
      daysUntilCutoff: 6,
      pendingApprovalsCount: 2,
      missingPunchCount: 1,
      totalGrossEstimate: null,
      totalNetEstimate: null,
    });

    component.setActiveTab('TEAM_ORG');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('บุคลากรทั้งหมด');
    expect(compiled.textContent).toContain('สรุปเวลาทำงานและแผนก');
    expect(compiled.textContent).toContain('งวดเงินเดือนล่าสุด');
    expect(compiled.textContent).toContain('รายการติดตามงานและสถานะทีม');
  });
});
