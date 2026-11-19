import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  MyTodayAttendanceDto,
  formatDateOnlyThai,
  formatMinutesToDuration,
  formatTimeOnly,
  formatUtcTime,
  mapAttendanceStatusLabel,
  mapAttendanceStatusTone,
  mapDayTypeLabel,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-attendance-checkin-card',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './attendance-checkin-card.component.html',
  styleUrl: './attendance-checkin-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AttendanceCheckinCardComponent {
  readonly attendance = input.required<MyTodayAttendanceDto>();
  readonly currentTimeText = input.required<string>();
  readonly isSubmitting = input<boolean>(false);
  readonly actionError = input<string | null>(null);

  readonly checkInClick = output<void>();
  readonly checkOutClick = output<void>();

  protected readonly formattedWorkDate = computed(() => {
    return formatDateOnlyThai(this.attendance().workDate);
  });

  protected readonly statusLabel = computed(() => {
    const item = this.attendance();
    if (item.lateMinutes > 0 && item.attendanceStatus.toUpperCase() === 'LATE') {
      return `มาสาย ${item.lateMinutes} นาที`;
    }
    return mapAttendanceStatusLabel(item.attendanceStatus);
  });

  protected readonly statusTone = computed(() => {
    return mapAttendanceStatusTone(this.attendance().attendanceStatus);
  });

  protected readonly dayTypeLabel = computed(() => {
    return mapDayTypeLabel(this.attendance().dayType);
  });

  protected readonly shiftWindowText = computed(() => {
    const item = this.attendance();
    if (!item.shiftStartTime || !item.shiftEndTime) {
      return '—';
    }
    const start = formatTimeOnly(item.shiftStartTime);
    const end = formatTimeOnly(item.shiftEndTime);
    if (item.isOvernight) {
      return `${start} - ${end} (+1 วัน)`;
    }
    return `${start} - ${end}`;
  });

  protected readonly actualInText = computed(() => {
    const item = this.attendance();
    return formatUtcTime(item.actualInUtc, item.branchTimezone);
  });

  protected readonly actualOutText = computed(() => {
    const item = this.attendance();
    return formatUtcTime(item.actualOutUtc, item.branchTimezone);
  });

  protected readonly workedDurationText = computed(() => {
    const item = this.attendance();
    if (!item.actualInUtc) {
      return '—';
    }
    if (!item.actualOutUtc) {
      return 'กำลังปฏิบัติงาน';
    }
    return formatMinutesToDuration(item.workedMinutes);
  });

  protected readonly canCheckIn = computed(() => {
    const item = this.attendance();
    return !item.actualInUtc && !item.isLockedByPayroll;
  });

  protected readonly canCheckOut = computed(() => {
    const item = this.attendance();
    return Boolean(item.actualInUtc) && !item.actualOutUtc && !item.isLockedByPayroll;
  });

  protected onCheckIn(): void {
    if (this.isSubmitting() || !this.canCheckIn()) {
      return;
    }
    this.checkInClick.emit();
  }

  protected onCheckOut(): void {
    if (this.isSubmitting() || !this.canCheckOut()) {
      return;
    }
    this.checkOutClick.emit();
  }
}
