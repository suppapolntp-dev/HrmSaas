import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { StatusTone } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  TeamPulseDto,
  formatDateOnlyThai,
  formatUtcTime,
  mapAttendanceStatusLabel,
  mapAttendanceStatusTone,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-team-pulse-card',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './team-pulse-card.component.html',
  styleUrl: './team-pulse-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamPulseCardComponent {
  readonly pulse = input.required<TeamPulseDto>();
  protected readonly ringCircumference = 2 * Math.PI * 26;

  protected readonly attendanceRatePercent = computed(() => {
    const data = this.pulse();
    if (data.totalScheduledCount <= 0) {
      return 0;
    }
    const attended = data.presentCount + data.lateCount;
    return Math.min(100, Math.round((attended / data.totalScheduledCount) * 100));
  });

  protected readonly ringStrokeDashoffset = computed(() => {
    const ratio = this.attendanceRatePercent() / 100;
    return this.ringCircumference * (1 - ratio);
  });

  protected formatWorkDate(dateText: string): string {
    return formatDateOnlyThai(dateText);
  }

  protected formatCheckInTime(isoText: string | null): string {
    return formatUtcTime(isoText);
  }

  protected getStatusLabel(status: string): string {
    return mapAttendanceStatusLabel(status);
  }

  protected getStatusTone(status: string): StatusTone {
    return mapAttendanceStatusTone(status);
  }
}
