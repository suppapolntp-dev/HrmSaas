import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { StatusIndicatorComponent, StatusTone } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  MyUpcomingRosterDto,
  formatDateOnlyThai,
  formatTimeOnly,
  mapDayTypeLabel,
  mapDayTypeTone,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-upcoming-roster-table',
  standalone: true,
  imports: [StatusIndicatorComponent],
  templateUrl: './upcoming-roster-table.component.html',
  styleUrl: './upcoming-roster-table.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpcomingRosterTableComponent {
  readonly rosters = input.required<readonly MyUpcomingRosterDto[]>();

  protected formatDate(workDate: string): string {
    return formatDateOnlyThai(workDate);
  }

  protected getDayTypeLabel(dayType: string): string {
    return mapDayTypeLabel(dayType);
  }

  protected getDayTypeTone(dayType: string): StatusTone {
    return mapDayTypeTone(dayType);
  }

  protected formatShiftTime(row: MyUpcomingRosterDto): string {
    if (!row.startTime || !row.endTime) {
      return '—';
    }
    const start = formatTimeOnly(row.startTime);
    const end = formatTimeOnly(row.endTime);
    if (row.isOvernight) {
      return `${start} - ${end} (+1 วัน)`;
    }
    return `${start} - ${end}`;
  }
}
