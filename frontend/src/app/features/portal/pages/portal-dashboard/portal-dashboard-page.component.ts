import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { ApiToggleService } from '../../../../core/http/api-toggle.service';
import { extractProblemDetails } from '../../../../core/http/http-error.helper';
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { PortalDashboardService } from '../../data-access/portal-dashboard.service';
import {
  ApprovalRequestDto,
  CheckInOutRequest,
  HrOverviewDto,
  MyDashboardSummaryDto,
  MyLeaveBalanceDto,
  MyTodayAttendanceDto,
  MyUpcomingRosterDto,
  PayrollReadinessDto,
  TeamPulseDto,
  formatDateOnlyThai,
  mapRequestTypeLabel,
} from '../../models/portal-dashboard.models';
import { AttendanceCheckinCardComponent } from '../../ui/attendance-checkin-card/attendance-checkin-card.component';
import { LatestPayslipCardComponent } from '../../ui/latest-payslip-card/latest-payslip-card.component';
import { LeaveBalanceGridComponent } from '../../ui/leave-balance-grid/leave-balance-grid.component';
import { MyRecentRequestsPanelComponent } from '../../ui/my-recent-requests-panel/my-recent-requests-panel.component';
import { PendingApprovalsPanelComponent } from '../../ui/pending-approvals-panel/pending-approvals-panel.component';
import { UpcomingRosterTableComponent } from '../../ui/upcoming-roster-table/upcoming-roster-table.component';
import { WorkforceIntelligenceBoardComponent } from '../../ui/workforce-intelligence-board/workforce-intelligence-board.component';

@Component({
  selector: 'app-portal-dashboard-page',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    StateContainerComponent,
    ConfirmDialogComponent,
    FormFieldComponent,
    AttendanceCheckinCardComponent,
    LeaveBalanceGridComponent,
    UpcomingRosterTableComponent,
    PendingApprovalsPanelComponent,
    LatestPayslipCardComponent,
    MyRecentRequestsPanelComponent,
    WorkforceIntelligenceBoardComponent,
  ],
  templateUrl: './portal-dashboard-page.component.html',
  styleUrl: './portal-dashboard-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PortalDashboardPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly dashboardService = inject(PortalDashboardService);
  protected readonly authSessionStore = inject(AuthSessionStore);
  private readonly apiToggleService = inject(ApiToggleService);

  // Clock state
  readonly currentTimeText = signal<string>(this.buildClockText());

  // Section 1: Today attendance & check-in/out
  readonly isAttendanceLoading = signal<boolean>(false);
  readonly attendance = signal<MyTodayAttendanceDto | null>(null);
  readonly attendanceError = signal<string | null>(null);
  readonly isSubmittingPunch = signal<boolean>(false);
  readonly punchError = signal<string | null>(null);

  // Section 2: Leave balances
  readonly isLeaveLoading = signal<boolean>(false);
  readonly leaveBalances = signal<MyLeaveBalanceDto[]>([]);
  readonly leaveError = signal<string | null>(null);

  // Section 3: Upcoming 7-day shift roster
  readonly isRosterLoading = signal<boolean>(false);
  readonly upcomingRosters = signal<MyUpcomingRosterDto[]>([]);
  readonly rosterError = signal<string | null>(null);

  // Section 4: Pending approvals (Manager / Approver)
  readonly isApprovalsLoading = signal<boolean>(false);
  readonly pendingApprovals = signal<ApprovalRequestDto[]>([]);
  readonly approvalsError = signal<string | null>(null);
  readonly submittingApprovalId = signal<string | null>(null);
  readonly approvalActionError = signal<string | null>(null);

  // Active dashboard layer tab
  readonly activeTab = signal<'MY_WORKSPACE' | 'TEAM_ORG'>('MY_WORKSPACE');

  // Section 5: My summary (recent requests + latest payslip + missing punch alert)
  readonly isMySummaryLoading = signal<boolean>(false);
  readonly mySummary = signal<MyDashboardSummaryDto | null>(null);
  readonly mySummaryError = signal<string | null>(null);

  // Section 6: Team pulse (MSS / Operation)
  readonly isTeamPulseLoading = signal<boolean>(false);
  readonly teamPulse = signal<TeamPulseDto | null>(null);
  readonly teamPulseError = signal<string | null>(null);

  // Section 7: HR overview
  readonly isHrOverviewLoading = signal<boolean>(false);
  readonly hrOverview = signal<HrOverviewDto | null>(null);
  readonly hrOverviewError = signal<string | null>(null);

  // Section 8: Payroll readiness
  readonly isPayrollReadinessLoading = signal<boolean>(false);
  readonly payrollReadiness = signal<PayrollReadinessDto | null>(null);
  readonly payrollReadinessError = signal<string | null>(null);

  // Approval confirmation dialog state
  readonly dialogTarget = signal<ApprovalRequestDto | null>(null);
  readonly dialogAction = signal<'APPROVE' | 'REJECT'>('APPROVE');
  readonly dialogComment = signal<string>('');

  protected readonly canReadAttendance = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('portal.attendance.read');
  });

  protected readonly canReadLeaves = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('portal.leave.read');
  });

  protected readonly canReadOvertime = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('portal.overtime.read');
  });

  protected readonly canReadPayslips = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('portal.payslip.read');
  });

  protected readonly canApproveWorkflows = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('workflow.approve');
  });

  protected readonly canReadTeamPulse = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('attendance.log.read');
  });

  protected readonly canReadHrOverview = computed(() => {
    this.authSessionStore.permissions();
    return this.authSessionStore.hasPermission('employee.read');
  });

  protected readonly canReadPayrollReadiness = computed(() => {
    this.authSessionStore.permissions();
    return (
      this.authSessionStore.hasPermission('payroll.period.manage') ||
      this.authSessionStore.hasPermission('payroll.calculate')
    );
  });

  protected readonly hasTeamOrOrgView = computed(
    () => this.canReadTeamPulse() || this.canReadHrOverview() || this.canReadPayrollReadiness(),
  );

  protected readonly isTeamOrgLoading = computed(
    () =>
      (this.canReadTeamPulse() && this.isTeamPulseLoading()) ||
      (this.canReadHrOverview() && this.isHrOverviewLoading()) ||
      (this.canReadPayrollReadiness() && this.isPayrollReadinessLoading()),
  );

  protected readonly teamOrgError = computed(
    () => this.teamPulseError() ?? this.hrOverviewError() ?? this.payrollReadinessError(),
  );

  protected readonly isTeamOrgEmpty = computed(
    () => this.teamPulse() === null && this.hrOverview() === null && this.payrollReadiness() === null,
  );

  protected readonly employeeDisplayName = computed(() => {
    const user = this.authSessionStore.user();
    if (!user) {
      return 'พนักงาน';
    }
    if (user.fullName && user.fullName.trim().length > 0) {
      return user.fullName;
    }
    return user.email;
  });

  protected readonly activeBranchName = computed(() => {
    const companyId = this.authSessionStore.activeCompanyId();
    const branchId = this.authSessionStore.activeBranchId();
    const companies = this.authSessionStore.companies();
    for (const company of companies) {
      if (company.companyId !== companyId) {
        continue;
      }
      for (const branch of company.branches) {
        if (branch.branchId === branchId) {
          return branch.name;
        }
      }
      return company.legalName;
    }
    return '';
  });

  protected readonly dialogTitle = computed(() => {
    if (this.dialogAction() === 'APPROVE') {
      return 'ยืนยันการอนุมัติคำขอ';
    }
    return 'ยืนยันการไม่อนุมัติคำขอ';
  });

  protected readonly dialogMessage = computed(() => {
    const target = this.dialogTarget();
    if (!target) {
      return '';
    }
    const typeLabel = mapRequestTypeLabel(target.requestType);
    if (this.dialogAction() === 'APPROVE') {
      return `ต้องการอนุมัติ${typeLabel}ของ ${target.requesterFullName} (${target.requesterEmployeeCode}) ใช่หรือไม่?`;
    }
    return `ต้องการปฏิเสธ${typeLabel}ของ ${target.requesterFullName} (${target.requesterEmployeeCode}) ใช่หรือไม่?`;
  });

  constructor() {
    const clockInterval = setInterval(() => {
      this.currentTimeText.set(this.buildClockText());
    }, 1000);

    this.destroyRef.onDestroy(() => {
      clearInterval(clockInterval);
    });

    effect(() => {
      this.authSessionStore.activeCompanyId();
      this.authSessionStore.activeBranchId();
      this.apiToggleService.isApiEnabled();

      untracked(() => {
        this.loadAllSections();
      });
    });
  }

  protected setActiveTab(tab: 'MY_WORKSPACE' | 'TEAM_ORG'): void {
    this.activeTab.set(tab);
  }

  protected reloadTeamOrgSections(): void {
    if (this.canReadTeamPulse()) {
      this.loadTeamPulse();
    }
    if (this.canReadHrOverview()) {
      this.loadHrOverview();
    }
    if (this.canReadPayrollReadiness()) {
      this.loadPayrollReadiness();
    }
  }

  protected formatThaiDate(dateText: string): string {
    return formatDateOnlyThai(dateText);
  }

  protected loadAllSections(): void {
    this.punchError.set(null);
    this.approvalActionError.set(null);

    if (this.canReadAttendance()) {
      this.loadTodayAttendance();
      this.loadUpcomingRosters();
    }

    if (this.canReadLeaves()) {
      this.loadLeaveBalances();
    }

    if (this.canApproveWorkflows()) {
      this.loadPendingApprovals();
    }

    this.loadMySummary();

    if (this.canReadTeamPulse()) {
      this.loadTeamPulse();
    }

    if (this.canReadHrOverview()) {
      this.loadHrOverview();
    }

    if (this.canReadPayrollReadiness()) {
      this.loadPayrollReadiness();
    }
  }

  protected loadTodayAttendance(): void {
    this.isAttendanceLoading.set(true);
    this.attendanceError.set(null);
    this.attendance.set(null);

    this.dashboardService
      .getMyTodayAttendance()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.isAttendanceLoading.set(false);
          this.attendance.set(data);
        },
        error: (err: unknown) => {
          this.isAttendanceLoading.set(false);
          this.attendance.set(null);
          this.attendanceError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadLeaveBalances(): void {
    this.isLeaveLoading.set(true);
    this.leaveError.set(null);
    this.leaveBalances.set([]);

    this.dashboardService
      .getMyLeaveBalances()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.isLeaveLoading.set(false);
          this.leaveBalances.set(items);
        },
        error: (err: unknown) => {
          this.isLeaveLoading.set(false);
          this.leaveBalances.set([]);
          this.leaveError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadUpcomingRosters(): void {
    this.isRosterLoading.set(true);
    this.rosterError.set(null);
    this.upcomingRosters.set([]);

    this.dashboardService
      .getMyUpcomingRosters(7)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.isRosterLoading.set(false);
          this.upcomingRosters.set(items);
        },
        error: (err: unknown) => {
          this.isRosterLoading.set(false);
          this.upcomingRosters.set([]);
          this.rosterError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadPendingApprovals(): void {
    this.isApprovalsLoading.set(true);
    this.approvalsError.set(null);
    this.pendingApprovals.set([]);

    this.dashboardService
      .getPendingApprovals()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.isApprovalsLoading.set(false);
          this.pendingApprovals.set(items);
        },
        error: (err: unknown) => {
          this.isApprovalsLoading.set(false);
          this.pendingApprovals.set([]);
          this.approvalsError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadMySummary(): void {
    this.isMySummaryLoading.set(true);
    this.mySummaryError.set(null);
    this.mySummary.set(null);

    this.dashboardService
      .getMySummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.isMySummaryLoading.set(false);
          this.mySummary.set(data);
        },
        error: (err: unknown) => {
          this.isMySummaryLoading.set(false);
          this.mySummary.set(null);
          this.mySummaryError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadTeamPulse(): void {
    this.isTeamPulseLoading.set(true);
    this.teamPulseError.set(null);
    this.teamPulse.set(null);

    this.dashboardService
      .getTeamPulse()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.isTeamPulseLoading.set(false);
          this.teamPulse.set(data);
        },
        error: (err: unknown) => {
          this.isTeamPulseLoading.set(false);
          this.teamPulse.set(null);
          this.teamPulseError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadHrOverview(): void {
    this.isHrOverviewLoading.set(true);
    this.hrOverviewError.set(null);
    this.hrOverview.set(null);

    this.dashboardService
      .getHrOverview()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.isHrOverviewLoading.set(false);
          this.hrOverview.set(data);
        },
        error: (err: unknown) => {
          this.isHrOverviewLoading.set(false);
          this.hrOverview.set(null);
          this.hrOverviewError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected loadPayrollReadiness(): void {
    this.isPayrollReadinessLoading.set(true);
    this.payrollReadinessError.set(null);
    this.payrollReadiness.set(null);

    this.dashboardService
      .getPayrollReadiness()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.isPayrollReadinessLoading.set(false);
          this.payrollReadiness.set(data);
        },
        error: (err: unknown) => {
          this.isPayrollReadinessLoading.set(false);
          this.payrollReadiness.set(null);
          this.payrollReadinessError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected async onCheckIn(): Promise<void> {
    if (this.isSubmittingPunch()) {
      return;
    }

    this.isSubmittingPunch.set(true);
    this.punchError.set(null);
    const payload = await this.resolvePunchRequest();

    this.dashboardService
      .checkIn(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmittingPunch.set(false);
          this.loadTodayAttendance();
        },
        error: (err: unknown) => {
          this.isSubmittingPunch.set(false);
          this.punchError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected async onCheckOut(): Promise<void> {
    if (this.isSubmittingPunch()) {
      return;
    }

    this.isSubmittingPunch.set(true);
    this.punchError.set(null);
    const payload = await this.resolvePunchRequest();

    this.dashboardService
      .checkOut(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmittingPunch.set(false);
          this.loadTodayAttendance();
        },
        error: (err: unknown) => {
          this.isSubmittingPunch.set(false);
          this.punchError.set(this.formatProblemMessage(err));
        },
      });
  }

  protected onOpenApproveDialog(item: ApprovalRequestDto): void {
    this.dialogTarget.set(item);
    this.dialogAction.set('APPROVE');
    this.dialogComment.set('');
    this.approvalActionError.set(null);
  }

  protected onOpenRejectDialog(item: ApprovalRequestDto): void {
    this.dialogTarget.set(item);
    this.dialogAction.set('REJECT');
    this.dialogComment.set('');
    this.approvalActionError.set(null);
  }

  protected onCloseDialog(): void {
    this.dialogTarget.set(null);
    this.dialogComment.set('');
    this.approvalActionError.set(null);
  }

  protected onConfirmApprovalDecision(): void {
    const target = this.dialogTarget();
    if (!target || this.submittingApprovalId() !== null) {
      return;
    }

    const action = this.dialogAction();
    const trimmedComment = this.dialogComment().trim();
    const commentValue = trimmedComment.length > 0 ? trimmedComment : null;

    this.submittingApprovalId.set(target.id);
    this.approvalActionError.set(null);

    this.dashboardService
      .decideApproval(target.id, { action, comment: commentValue })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.submittingApprovalId.set(null);
          this.dialogTarget.set(null);
          this.dialogComment.set('');
          this.loadPendingApprovals();
        },
        error: (err: unknown) => {
          this.submittingApprovalId.set(null);
          this.approvalActionError.set(this.formatProblemMessage(err));
        },
      });
  }

  private formatProblemMessage(err: unknown): string {
    const problem = extractProblemDetails(err);
    if (problem.detail && problem.detail.trim().length > 0 && problem.detail !== problem.title) {
      return `${problem.title}: ${problem.detail}`;
    }
    return problem.title;
  }

  private resolvePunchRequest(): Promise<CheckInOutRequest> {
    if (typeof navigator === 'undefined' || !navigator.geolocation) {
      return Promise.resolve({ latitude: null, longitude: null, source: 'WEB' });
    }

    return new Promise((resolve) => {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: Number(position.coords.latitude.toFixed(6)),
            longitude: Number(position.coords.longitude.toFixed(6)),
            source: 'WEB',
          });
        },
        () => {
          resolve({ latitude: null, longitude: null, source: 'WEB' });
        },
        { timeout: 1500, maximumAge: 60000 },
      );
    });
  }

  private buildClockText(): string {
    return new Intl.DateTimeFormat('th-TH', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
    }).format(new Date());
  }
}
