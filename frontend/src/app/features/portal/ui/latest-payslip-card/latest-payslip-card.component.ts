import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  LatestPayslipSnapshotDto,
  formatCurrencyThb,
  formatDateOnlyThai,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-latest-payslip-card',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './latest-payslip-card.component.html',
  styleUrl: './latest-payslip-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LatestPayslipCardComponent {
  readonly payslip = input.required<LatestPayslipSnapshotDto>();
  readonly isRevealed = signal<boolean>(false);

  protected toggleReveal(): void {
    this.isRevealed.set(!this.isRevealed());
  }

  protected formatPayDate(dateText: string): string {
    return formatDateOnlyThai(dateText);
  }

  protected formatAmount(amount: number): string {
    if (!this.isRevealed()) {
      return '••••••';
    }
    return `${formatCurrencyThb(amount)} ฿`;
  }
}
