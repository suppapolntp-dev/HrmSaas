import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableAction, DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import {
  formatCurrencyThb,
  mapPayrollStatusLabel,
  mapPayrollStatusTone,
} from '../../../portal/models/portal-dashboard.models';
import { PayslipDto } from '../../../portal/models/portal-ess.models';
import { PayrollService } from '../../data-access/payroll.service';
import { PayrollPeriodDto } from '../../models/payroll.models';

@Component({
  selector: 'app-payroll-run-page',
  standalone: true,
  imports: [
    RouterLink,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './payroll-run-page.component.html',
  styleUrl: './payroll-run-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayrollRunPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly payrollService = inject(PayrollService);
  private readonly authSessionStore = inject(AuthSessionStore);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly destroyRef = inject(DestroyRef);

  readonly periodId = signal<string>('');
  readonly currentPeriod = signal<PayrollPeriodDto | null>(null);
  readonly payslips = signal<PayslipDto[]>([]);
  readonly selectedPayslip = signal<PayslipDto | null>(null);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly isCalculating = signal(false);
  readonly isLockDialogOpen = signal(false);
  readonly isLocking = signal(false);

  readonly searchQuery = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly columns: readonly DataTableColumn[] = [
    { key: 'employeeCode', label: 'รหัส', isMono: true },
    { key: 'employeeName', label: 'พนักงาน' },
    { key: 'grossIncome', label: 'รายได้รวม', align: 'right', isNumeric: true },
    { key: 'ssoEmployeeAmount', label: 'ประกันสังคม', align: 'right', isNumeric: true },
    { key: 'withholdingTaxAmount', label: 'ภาษี ภ.ง.ด.1', align: 'right', isNumeric: true },
    { key: 'totalDeductions', label: 'หักรวม', align: 'right', isNumeric: true },
    { key: 'netPay', label: 'รับสุทธิ', align: 'right', isNumeric: true },
    { key: 'isLocked', label: 'สถานะ' },
  ];

  readonly tableActions: readonly DataTableAction<PayslipDto>[] = [
    { key: 'detail', label: 'ดูรายละเอียดรายการ', onClick: (row) => this.selectedPayslip.set(row) },
  ];

  readonly totals = computed(() => {
    let gross = 0;
    let ssoAndTax = 0;
    let net = 0;
    for (const ps of this.payslips()) {
      gross += ps.grossIncome;
      ssoAndTax += ps.ssoEmployeeAmount + ps.withholdingTaxAmount;
      net += ps.netPay;
    }
    return {
      count: this.payslips().length,
      grossText: formatCurrencyThb(gross),
      ssoAndTaxText: formatCurrencyThb(ssoAndTax),
      netText: formatCurrencyThb(net),
    };
  });

  readonly filteredPayslips = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.payslips();
    }
    return this.payslips().filter(
      (p) =>
        p.employeeCode.toLowerCase().includes(q) || p.employeeName.toLowerCase().includes(q),
    );
  });

  readonly isPeriodLocked = computed(() => {
    const p = this.currentPeriod();
    if (p) {
      return p.status === 'LOCKED';
    }
    const items = this.payslips();
    return items.length > 0 && items.every((i) => i.isLocked);
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('periodId') ?? '';
    this.periodId.set(id);
    this.loadRunData();
  }

  loadRunData(): void {
    const id = this.periodId();
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const periods$ = this.authSessionStore.hasPermission('payroll.period.manage')
      ? this.payrollService.getPeriods()
      : of<PayrollPeriodDto[]>([]);

    forkJoin({
      periods: periods$,
      payslips: this.payrollService.getPayslips(id),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          const matchedPeriod = res.periods.find((p) => p.id === id) ?? null;
          this.currentPeriod.set(matchedPeriod);
          this.payslips.set(res.payslips);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.currentPeriod.set(null);
          this.payslips.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดข้อมูลคำนวณเงินเดือนได้'));
          this.isLoading.set(false);
        },
      });
  }

  calculatePayroll(): void {
    const id = this.periodId();
    if (!id) {
      return;
    }
    this.isCalculating.set(true);
    this.errorMessage.set(null);
    this.feedbackSuccess.set(null);

    this.payrollService
      .calculatePeriod(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.isCalculating.set(false);
          this.feedbackSuccess.set(
            `คำนวณเงินเดือนงวด ${summary.periodCode} สำเร็จ (${summary.processedEmployees} คน, ยอดสุทธิรวม ${formatCurrencyThb(summary.totalNetPay)})`,
          );
          this.loadRunData();
        },
        error: (err: unknown) => {
          this.isCalculating.set(false);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถคำนวณเงินเดือนได้'));
        },
      });
  }

  confirmLockPeriod(): void {
    const id = this.periodId();
    if (!id) {
      return;
    }
    this.isLocking.set(true);

    this.payrollService
      .lockPeriod(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (locked) => {
          this.isLocking.set(false);
          this.isLockDialogOpen.set(false);
          this.currentPeriod.set(locked);
          this.feedbackSuccess.set(`ล็อกงวดเงินเดือน ${locked.periodCode} เรียบร้อยแล้ว`);
          this.loadRunData();
        },
        error: (err: unknown) => {
          this.isLocking.set(false);
          this.isLockDialogOpen.set(false);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถล็อกงวดเงินเดือนได้'));
        },
      });
  }

  formatMoney(amount: number): string {
    return formatCurrencyThb(amount);
  }

  getPeriodStatusLabel(status: string): string {
    return mapPayrollStatusLabel(status);
  }

  getPeriodStatusTone(status: string) {
    return mapPayrollStatusTone(status);
  }
}
