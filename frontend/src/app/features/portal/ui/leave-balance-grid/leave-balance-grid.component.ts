import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MyLeaveBalanceDto } from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-leave-balance-grid',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './leave-balance-grid.component.html',
  styleUrl: './leave-balance-grid.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LeaveBalanceGridComponent {
  readonly balances = input.required<readonly MyLeaveBalanceDto[]>();

  protected formatDays(days: number): string {
    if (Number.isInteger(days)) {
      return String(days);
    }
    return days.toFixed(1);
  }

  protected getTotalAvailable(item: MyLeaveBalanceDto): number {
    return item.carriedOverDays + item.entitledDays;
  }

  protected getProgressWidthPercent(item: MyLeaveBalanceDto): number {
    const total = this.getTotalAvailable(item);
    if (total <= 0) {
      return 0;
    }
    const ratio = (item.remainingDays / total) * 100;
    if (ratio < 0) {
      return 0;
    }
    if (ratio > 100) {
      return 100;
    }
    return Math.round(ratio);
  }
}
