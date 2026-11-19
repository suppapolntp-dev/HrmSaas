import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  DepartmentHeadcountDto,
  HrOverviewDto,
  formatDateOnlyThai,
  mapLifecycleAlertLabel,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-hr-overview-card',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './hr-overview-card.component.html',
  styleUrl: './hr-overview-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HrOverviewCardComponent {
  readonly overview = input.required<HrOverviewDto>();

  protected readonly totalHeadcount = computed(() => {
    const data = this.overview();
    return data.activeCount + data.probationCount;
  });

  protected getDeptTotal(dept: DepartmentHeadcountDto): number {
    return dept.activeCount + dept.probationCount;
  }

  protected getDeptBarPercent(dept: DepartmentHeadcountDto): number {
    const total = this.totalHeadcount();
    if (total <= 0) {
      return 0;
    }
    return Math.min(100, Math.round((this.getDeptTotal(dept) / total) * 100));
  }

  protected getAlertLabel(alertType: string): string {
    return mapLifecycleAlertLabel(alertType);
  }

  protected formatDueDate(dateText: string): string {
    return formatDateOnlyThai(dateText);
  }
}
