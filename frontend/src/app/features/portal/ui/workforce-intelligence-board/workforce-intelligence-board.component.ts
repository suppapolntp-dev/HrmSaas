import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { StatusTone } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  ApprovalRequestDto,
  HrOverviewDto,
  PayrollReadinessDto,
  TeamPulseDto,
  formatCurrencyThb,
  formatDateOnlyThai,
  formatUtcDateTime,
  formatUtcTime,
  mapAttendanceStatusLabel,
  mapAttendanceStatusTone,
  mapLifecycleAlertLabel,
  mapPayrollStatusLabel,
  mapPayrollStatusTone,
  mapRequestTypeLabel,
  parseApprovalPayloadSummary,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-workforce-intelligence-board',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './workforce-intelligence-board.component.html',
  styleUrl: './workforce-intelligence-board.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkforceIntelligenceBoardComponent {
  readonly teamPulse = input<TeamPulseDto | null>(null);
  readonly hrOverview = input<HrOverviewDto | null>(null);
  readonly payrollReadiness = input<PayrollReadinessDto | null>(null);
  readonly pendingApprovals = input<ApprovalRequestDto[]>([]);

  readonly isPayrollAmountRevealed = signal<boolean>(false);
  readonly activeBottomTab = signal<'ATTENDANCE' | 'APPROVALS' | 'ALERTS'>('ATTENDANCE');
  readonly activeAttendanceFilter = signal<'ALL' | 'EXCEPTIONS' | 'ON_LEAVE'>('ALL');

  protected readonly totalHeadcount = computed(() => {
    const hr = this.hrOverview();
    if (!hr) {
      return 0;
    }
    return hr.activeCount + hr.probationCount;
  });

  protected readonly attendanceRatePercent = computed(() => {
    const pulse = this.teamPulse();
    if (!pulse || pulse.totalScheduledCount <= 0) {
      return 0;
    }
    const attended = pulse.presentCount + pulse.lateCount;
    return Math.min(100, Math.round((attended / pulse.totalScheduledCount) * 100));
  });

  protected readonly onTimeRatePercent = computed(() => {
    const pulse = this.teamPulse();
    if (!pulse || pulse.totalScheduledCount <= 0) {
      return 0;
    }
    return Math.min(100, Math.round((pulse.presentCount / pulse.totalScheduledCount) * 100));
  });

  protected readonly lateRatePercent = computed(() => {
    const pulse = this.teamPulse();
    if (!pulse || pulse.totalScheduledCount <= 0) {
      return 0;
    }
    return Math.min(100, Math.round((pulse.lateCount / pulse.totalScheduledCount) * 100));
  });

  protected readonly absenceRatePercent = computed(() => {
    const pulse = this.teamPulse();
    if (!pulse || pulse.totalScheduledCount <= 0) {
      return 0;
    }
    const off = pulse.absentCount + pulse.missingPunchCount + pulse.onLeaveCount;
    return Math.min(100, Math.round((off / pulse.totalScheduledCount) * 100));
  });

  protected readonly totalExceptionsCount = computed(() => {
    const pulse = this.teamPulse();
    if (!pulse) {
      return 0;
    }
    return pulse.lateCount + pulse.missingPunchCount + pulse.absentCount;
  });

  protected readonly weekDaysStrip = computed(() => {
    const today = new Date();
    const dayLabels = ['อา', 'จ', 'อ', 'พ', 'พฤ', 'ศ', 'ส'];
    const result: Array<{ dayNum: number; dayName: string; isToday: boolean }> = [];

    for (let offset = -2; offset <= 4; offset++) {
      const d = new Date(today);
      d.setDate(today.getDate() + offset);
      result.push({
        dayNum: d.getDate(),
        dayName: dayLabels[d.getDay()],
        isToday: offset === 0,
      });
    }
    return result;
  });

  protected readonly departmentChartPoints = computed(() => {
    const hr = this.hrOverview();
    if (!hr || hr.departmentBreakdown.length === 0) {
      return [];
    }

    const total = Math.max(1, this.totalHeadcount());
    return hr.departmentBreakdown.map((dept) => {
      const count = dept.activeCount + dept.probationCount;
      const pct = Math.min(100, Math.round((count / total) * 100));
      return {
        departmentId: dept.departmentId,
        departmentName: dept.departmentName,
        activeCount: dept.activeCount,
        probationCount: dept.probationCount,
        totalCount: count,
        percent: pct,
      };
    });
  });

  protected readonly filteredAttendanceRows = computed(() => {
    const pulse = this.teamPulse();
    if (!pulse) {
      return [];
    }

    const filter = this.activeAttendanceFilter();
    const rows: Array<{
      employeeId: string;
      employeeCode: string;
      employeeName: string;
      contextLabel: string;
      checkInText: string;
      status: string;
      statusLabel: string;
      statusTone: StatusTone;
      detailText: string;
      severity: 'danger' | 'warning' | 'info';
    }> = [];

    if (filter === 'ALL' || filter === 'EXCEPTIONS') {
      for (const item of pulse.exceptions) {
        let detailText = 'รอตรวจสอบ';
        if (item.lateMinutes > 0) {
          detailText = `สาย ${item.lateMinutes} นาที`;
        } else if (item.attendanceStatus === 'MISSING_PUNCH') {
          detailText = 'ไม่พบเวลาเข้า-ออก';
        }

        rows.push({
          employeeId: item.employeeId,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          contextLabel: 'บันทึกเวลาผิดปกติ',
          checkInText: item.actualInUtc ? formatUtcTime(item.actualInUtc) : '—',
          status: item.attendanceStatus,
          statusLabel: mapAttendanceStatusLabel(item.attendanceStatus),
          statusTone: mapAttendanceStatusTone(item.attendanceStatus),
          detailText,
          severity: item.attendanceStatus === 'ABSENT' ? 'danger' : 'warning',
        });
      }
    }

    if (filter === 'ALL' || filter === 'ON_LEAVE') {
      for (const person of pulse.whoIsOut) {
        rows.push({
          employeeId: person.employeeId,
          employeeCode: person.employeeCode,
          employeeName: person.employeeName,
          contextLabel: person.departmentName,
          checkInText: '—',
          status: 'ON_LEAVE',
          statusLabel: 'ลางานวันนี้',
          statusTone: 'info',
          detailText: 'อนุมัติแล้ว',
          severity: 'info',
        });
      }
    }

    return rows;
  });

  protected togglePayrollAmount(): void {
    this.isPayrollAmountRevealed.set(!this.isPayrollAmountRevealed());
  }

  protected setBottomTab(tab: 'ATTENDANCE' | 'APPROVALS' | 'ALERTS'): void {
    this.activeBottomTab.set(tab);
  }

  protected setAttendanceFilter(filter: 'ALL' | 'EXCEPTIONS' | 'ON_LEAVE'): void {
    this.activeAttendanceFilter.set(filter);
  }

  protected formatCutoffDays(days: number | null | undefined): string {
    if (days === null || days === undefined) {
      return '—';
    }
    if (days < 0) {
      return `เกิน ${Math.abs(days)} วัน`;
    }
    if (days === 0) {
      return 'ตัดรอบวันนี้';
    }
    return `อีก ${days} วัน`;
  }

  protected formatMaskedAmount(amount: number | null | undefined): string {
    if (amount === null || amount === undefined) {
      return 'ยังไม่คำนวณ';
    }
    if (!this.isPayrollAmountRevealed()) {
      return '••••••';
    }
    return `${formatCurrencyThb(amount)} ฿`;
  }

  protected getAlertLabel(alertType: string): string {
    return mapLifecycleAlertLabel(alertType);
  }

  protected formatDate(dateText: string | null | undefined): string {
    return formatDateOnlyThai(dateText ?? null);
  }

  protected formatSubmittedAt(isoText: string): string {
    return formatUtcDateTime(isoText);
  }

  protected getRequestTypeLabel(requestType: string): string {
    return mapRequestTypeLabel(requestType);
  }

  protected getRequestSummary(payloadSummary: string): string {
    return parseApprovalPayloadSummary(payloadSummary);
  }

  protected getPayrollStatusLabel(status: string | null | undefined): string {
    return mapPayrollStatusLabel(status ?? null);
  }

  protected getPayrollStatusTone(status: string | null | undefined): StatusTone {
    return mapPayrollStatusTone(status ?? null);
  }
}
