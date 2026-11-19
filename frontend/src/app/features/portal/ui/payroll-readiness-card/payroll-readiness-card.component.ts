import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { StatusTone } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  PayrollReadinessDto,
  formatCurrencyThb,
  formatDateOnlyThai,
  mapPayrollStatusLabel,
  mapPayrollStatusTone,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-payroll-readiness-card',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './payroll-readiness-card.component.html',
  styleUrl: './payroll-readiness-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayrollReadinessCardComponent {
  readonly readiness = input.required<PayrollReadinessDto>();
  readonly isRevealed = signal<boolean>(false);

  protected toggleReveal(): void {
    this.isRevealed.set(!this.isRevealed());
  }

  protected getStatusLabel(status: string | null): string {
    return mapPayrollStatusLabel(status);
  }

  protected getStatusTone(status: string | null): StatusTone {
    return mapPayrollStatusTone(status);
  }

  protected formatDate(dateText: string | null): string {
    return formatDateOnlyThai(dateText);
  }

  protected formatAmount(amount: number | null): string {
    if (amount === null) {
      return 'ยังไม่คำนวณ';
    }
    if (!this.isRevealed()) {
      return '••••••';
    }
    return `${formatCurrencyThb(amount)} ฿`;
  }

  protected formatCutoffCountdown(days: number | null): string {
    if (days === null) {
      return '—';
    }
    if (days < 0) {
      return `เลยกำหนดตัดรอบ ${Math.abs(days)} วัน`;
    }
    if (days === 0) {
      return 'ตัดรอบวันนี้';
    }
    return `อีก ${days} วัน`;
  }
}
