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
import { RouterLink } from '@angular/router';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import {
  DataTableAction,
  DataTableColumn,
  DataTableSortEvent,
} from '../../../../shared/ui/data-table/data-table.models';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import { PortalPayslipsService } from '../../data-access/portal-payslips.service';
import { formatCurrencyThb } from '../../models/portal-dashboard.models';
import { PayslipDto } from '../../models/portal-ess.models';

@Component({
  selector: 'app-payslips-page',
  standalone: true,
  imports: [
    RouterLink,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
  ],
  templateUrl: './payslips-page.component.html',
  styleUrl: './payslips-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayslipsPageComponent implements OnInit {
  private readonly portalPayslipsService = inject(PortalPayslipsService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly destroyRef = inject(DestroyRef);

  readonly payslips = signal<PayslipDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly searchQuery = signal('');
  readonly sortBy = signal<string | null>('periodCode');
  readonly sortDirection = signal<SortDirection | null>('desc');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly selectedPayslip = signal<PayslipDto | null>(null);

  readonly columns: readonly DataTableColumn[] = [
    { key: 'periodCode', label: 'งวดเงินเดือน', sortable: true, isMono: true },
    { key: 'grossIncome', label: 'รายได้รวม', align: 'right', isNumeric: true, sortable: true },
    { key: 'ssoEmployeeAmount', label: 'ประกันสังคม', align: 'right', isNumeric: true },
    { key: 'withholdingTaxAmount', label: 'ภาษี ภ.ง.ด.1', align: 'right', isNumeric: true },
    { key: 'totalDeductions', label: 'หักรวม', align: 'right', isNumeric: true },
    { key: 'netPay', label: 'เงินสุทธิ', align: 'right', isNumeric: true, sortable: true },
    { key: 'isLocked', label: 'สถานะงวด' },
  ];

  readonly tableActions: readonly DataTableAction<PayslipDto>[] = [
    {
      key: 'view',
      label: 'ดูสลิปเงินเดือน',
      onClick: (row) => this.selectedPayslip.set(row),
    },
  ];

  readonly filteredPayslips = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    if (query.length === 0) {
      return this.payslips();
    }
    return this.payslips().filter(
      (p) => p.periodCode.toLowerCase().includes(query) || p.employeeName.toLowerCase().includes(query),
    );
  });

  readonly sortedPayslips = computed(() => {
    const list = [...this.filteredPayslips()];
    const activeSort = this.sortBy();
    const dir = this.sortDirection();
    if (!activeSort || !dir) {
      return list;
    }
    const factor = dir === 'asc' ? 1 : -1;
    list.sort((a, b) => {
      switch (activeSort) {
        case 'periodCode':
          return a.periodCode.localeCompare(b.periodCode) * factor;
        case 'grossIncome':
          return (a.grossIncome - b.grossIncome) * factor;
        case 'netPay':
          return (a.netPay - b.netPay) * factor;
        default:
          return 0;
      }
    });
    return list;
  });

  readonly pagedPayslips = computed(() => {
    const start = (this.page() - 1) * Math.max(1, this.pageSize());
    return this.sortedPayslips().slice(start, start + Math.max(1, this.pageSize()));
  });

  ngOnInit(): void {
    this.loadPayslips();
  }

  loadPayslips(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.portalPayslipsService
      .getMyPayslips()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.payslips.set(data);
          this.page.set(1);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.payslips.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดสลิปเงินเดือนได้'));
          this.isLoading.set(false);
        },
      });
  }

  onSortChange(event: DataTableSortEvent): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.page.set(1);
  }

  formatMoney(amount: number): string {
    return formatCurrencyThb(amount);
  }

  printPayslip(): void {
    window.print();
  }
}
