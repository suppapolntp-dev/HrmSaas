import { StatusTone } from '../../../shared/ui/status-indicator/status-indicator.component';

export interface MyTodayAttendanceDto {
  workDate: string;
  branchName: string;
  branchTimezone: string;
  dayType: string;
  shiftDefinitionId: string | null;
  shiftCode: string | null;
  shiftName: string | null;
  shiftStartTime: string | null;
  shiftEndTime: string | null;
  isOvernight: boolean;
  actualInUtc: string | null;
  actualOutUtc: string | null;
  workedMinutes: number;
  lateMinutes: number;
  earlyOutMinutes: number;
  otMinutes: number;
  attendanceStatus: string;
  isLockedByPayroll: boolean;
}

export interface AttendanceDailySummaryDto {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  branchId: string;
  departmentId: string;
  workDate: string;
  shiftDefinitionId: string | null;
  shiftName: string | null;
  actualInUtc: string | null;
  actualOutUtc: string | null;
  workedMinutes: number;
  lateMinutes: number;
  earlyOutMinutes: number;
  otMinutes: number;
  attendanceStatus: string;
  isLockedByPayroll: boolean;
}

export interface CheckInOutRequest {
  latitude: number | null;
  longitude: number | null;
  source: string;
}

export interface TimeAdjustmentRequest {
  workDate: string;
  requestedInUtc: string;
  requestedOutUtc: string;
  reason: string;
}

export interface TimeAdjustmentCreatedDto {
  approvalRequestId: string;
  status: string;
  submittedAt: string;
}

export interface MyLeaveBalanceDto {
  id: string;
  leaveTypeId: string;
  leaveTypeCode: string;
  leaveTypeName: string;
  quotaYear: number;
  carriedOverDays: number;
  entitledDays: number;
  usedDays: number;
  pendingDays: number;
  remainingDays: number;
}

export interface MyUpcomingRosterDto {
  id: string;
  workDate: string;
  dayType: string;
  shiftDefinitionId: string | null;
  shiftCode: string | null;
  shiftName: string | null;
  startTime: string | null;
  endTime: string | null;
  isOvernight: boolean;
}

export interface ApprovalRequestDto {
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
}

export interface DecideApprovalRequest {
  action: 'APPROVE' | 'REJECT';
  comment: string | null;
}

export interface MyDashboardRequestItemDto {
  id: string;
  requestType: string;
  currentStepOrder: number;
  status: string;
  payloadSummary: string;
  submittedAt: string;
}

export interface LatestPayslipSnapshotDto {
  payslipId: string;
  payrollPeriodId: string;
  periodCode: string;
  payDate: string;
  grossIncome: number;
  totalDeductions: number;
  netPay: number;
  isLocked: boolean;
}

export interface MyDashboardSummaryDto {
  recentMissingPunchDate: string | null;
  recentRequests: MyDashboardRequestItemDto[];
  latestPayslip: LatestPayslipSnapshotDto | null;
}

export interface TeamExceptionItemDto {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  attendanceStatus: string;
  lateMinutes: number;
  actualInUtc: string | null;
}

export interface WhoIsOutItemDto {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
}

export interface TeamPulseDto {
  workDate: string;
  totalScheduledCount: number;
  presentCount: number;
  lateCount: number;
  missingPunchCount: number;
  absentCount: number;
  onLeaveCount: number;
  exceptions: TeamExceptionItemDto[];
  whoIsOut: WhoIsOutItemDto[];
}

export interface DepartmentHeadcountDto {
  departmentId: string;
  departmentName: string;
  activeCount: number;
  probationCount: number;
}

export interface LifecycleAlertDto {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  alertType: string;
  dueDate: string;
  daysRemaining: number;
}

export interface HrOverviewDto {
  activeCount: number;
  probationCount: number;
  newHiresThisMonthCount: number;
  departmentBreakdown: DepartmentHeadcountDto[];
  lifecycleAlerts: LifecycleAlertDto[];
}

export interface PayrollReadinessDto {
  periodId: string | null;
  periodCode: string | null;
  periodStart: string | null;
  periodEnd: string | null;
  attendanceCutoffDate: string | null;
  payDate: string | null;
  status: string | null;
  daysUntilCutoff: number | null;
  pendingApprovalsCount: number;
  missingPunchCount: number;
  totalGrossEstimate: number | null;
  totalNetEstimate: number | null;
}

export function formatTimeOnly(timeText: string | null): string {
  if (!timeText || timeText.trim().length === 0) {
    return '—';
  }
  const parts = timeText.split(':');
  if (parts.length >= 2) {
    return `${parts[0].padStart(2, '0')}:${parts[1].padStart(2, '0')}`;
  }
  return timeText;
}

export function formatUtcTime(isoText: string | null, timeZone = 'Asia/Bangkok'): string {
  if (!isoText) {
    return '—';
  }

  const parsed = new Date(isoText);
  if (Number.isNaN(parsed.getTime())) {
    return '—';
  }

  try {
    return new Intl.DateTimeFormat('th-TH', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone,
    }).format(parsed);
  } catch {
    return new Intl.DateTimeFormat('th-TH', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    }).format(parsed);
  }
}

export function formatDateOnlyThai(dateText: string | null): string {
  if (!dateText) {
    return '—';
  }

  const parts = dateText.split('-');
  if (parts.length !== 3) {
    return dateText;
  }

  const year = Number(parts[0]);
  const month = Number(parts[1]);
  const day = Number(parts[2]);
  if (Number.isNaN(year) || Number.isNaN(month) || Number.isNaN(day)) {
    return dateText;
  }

  const utcDate = new Date(Date.UTC(year, month - 1, day));
  return new Intl.DateTimeFormat('th-TH', {
    weekday: 'short',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    timeZone: 'UTC',
  }).format(utcDate);
}

export function formatUtcDateTime(isoText: string | null, timeZone = 'Asia/Bangkok'): string {
  if (!isoText) {
    return '—';
  }

  const parsed = new Date(isoText);
  if (Number.isNaN(parsed.getTime())) {
    return '—';
  }

  try {
    return new Intl.DateTimeFormat('th-TH', {
      day: '2-digit',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone,
    }).format(parsed);
  } catch {
    return parsed.toISOString();
  }
}

export function formatMinutesToDuration(totalMinutes: number): string {
  if (totalMinutes <= 0) {
    return '0 ชม. 00 นาที';
  }
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  return `${hours} ชม. ${String(minutes).padStart(2, '0')} นาที`;
}

export function mapAttendanceStatusLabel(status: string): string {
  switch (status.toUpperCase()) {
    case 'NOT_CHECKED_IN':
      return 'ยังไม่ลงเวลาเข้างาน';
    case 'PRESENT':
      return 'ลงเวลาปกติ';
    case 'LATE':
      return 'มาสาย';
    case 'EARLY_OUT':
      return 'ออกก่อนเวลา';
    case 'ABSENT':
      return 'ขาดงาน';
    case 'ON_LEAVE':
      return 'ลางาน';
    default:
      return status;
  }
}

export function mapAttendanceStatusTone(status: string): StatusTone {
  switch (status.toUpperCase()) {
    case 'PRESENT':
      return 'success';
    case 'LATE':
    case 'EARLY_OUT':
      return 'warning';
    case 'ABSENT':
      return 'danger';
    case 'ON_LEAVE':
      return 'info';
    default:
      return 'neutral';
  }
}

export function mapDayTypeLabel(dayType: string): string {
  switch (dayType.toUpperCase()) {
    case 'WORKDAY':
      return 'วันทำงาน';
    case 'OFFDAY':
      return 'วันหยุดประจำสัปดาห์';
    case 'HOLIDAY':
      return 'วันหยุดนักขัตฤกษ์';
    case 'UNSCHEDULED':
      return 'ยังไม่ได้จัดกะ';
    default:
      return dayType;
  }
}

export function mapDayTypeTone(dayType: string): StatusTone {
  switch (dayType.toUpperCase()) {
    case 'WORKDAY':
      return 'info';
    case 'OFFDAY':
      return 'neutral';
    case 'HOLIDAY':
      return 'warning';
    default:
      return 'neutral';
  }
}

export function mapRequestTypeLabel(requestType: string): string {
  switch (requestType.toUpperCase()) {
    case 'LEAVE':
      return 'คำขอลา';
    case 'OVERTIME':
      return 'คำขอ OT';
    case 'TIME_ADJUSTMENT':
      return 'ขอแก้ไขเวลา';
    default:
      return requestType;
  }
}

export function parseApprovalPayloadSummary(payloadSummary: string): string {
  if (!payloadSummary || payloadSummary.trim().length === 0) {
    return '—';
  }

  try {
    const parsed = JSON.parse(payloadSummary) as Record<string, unknown>;
    if (typeof parsed['note'] === 'string' && parsed['note'].trim().length > 0) {
      return parsed['note'];
    }
    if (typeof parsed['reason'] === 'string' && parsed['reason'].trim().length > 0) {
      const startDate = typeof parsed['startDate'] === 'string' ? parsed['startDate'] : null;
      const endDate = typeof parsed['endDate'] === 'string' ? parsed['endDate'] : null;
      if (startDate && endDate) {
        return `${startDate} ถึง ${endDate} — ${parsed['reason']}`;
      }
      const workDate = typeof parsed['workDate'] === 'string' ? parsed['workDate'] : null;
      if (workDate) {
        return `${workDate} — ${parsed['reason']}`;
      }
      return parsed['reason'];
    }
  } catch {
    // Plain text summary fallback
  }

  return payloadSummary;
}

export function formatCurrencyThb(amount: number | null | undefined): string {
  if (amount === null || amount === undefined || Number.isNaN(amount)) {
    return '—';
  }
  return new Intl.NumberFormat('th-TH', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function mapApprovalStatusLabel(status: string): string {
  switch (status.toUpperCase()) {
    case 'PENDING':
      return 'รออนุมัติ';
    case 'APPROVED':
      return 'อนุมัติแล้ว';
    case 'REJECTED':
      return 'ไม่อนุมัติ';
    case 'CANCELLED':
      return 'ยกเลิกแล้ว';
    default:
      return status;
  }
}

export function mapApprovalStatusTone(status: string): StatusTone {
  switch (status.toUpperCase()) {
    case 'APPROVED':
      return 'success';
    case 'PENDING':
      return 'warning';
    case 'REJECTED':
      return 'danger';
    default:
      return 'neutral';
  }
}

export function mapPayrollStatusLabel(status: string | null): string {
  switch ((status ?? '').toUpperCase()) {
    case 'DRAFT':
      return 'ร่าง (Draft)';
    case 'CALCULATED':
      return 'คำนวณแล้ว';
    case 'APPROVED':
      return 'อนุมัติงวดแล้ว';
    case 'LOCKED':
      return 'ปิดงวดถาวร';
    default:
      return status ?? '—';
  }
}

export function mapPayrollStatusTone(status: string | null): StatusTone {
  switch ((status ?? '').toUpperCase()) {
    case 'APPROVED':
      return 'success';
    case 'CALCULATED':
      return 'warning';
    case 'DRAFT':
    case 'LOCKED':
    default:
      return 'neutral';
  }
}

export function mapLifecycleAlertLabel(alertType: string): string {
  switch (alertType.toUpperCase()) {
    case 'PROBATION_END':
      return 'ครบกำหนดทดลองงาน';
    case 'CONTRACT_EXPIRY':
      return 'สัญญาจ้างใกล้สิ้นสุด';
    default:
      return alertType;
  }
}
