import { HttpErrorResponse } from '@angular/common/http';
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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableAction, DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import {
  formatDateOnlyThai,
  mapPayrollStatusLabel,
  mapPayrollStatusTone,
} from '../../../portal/models/portal-dashboard.models';
import { PayrollService } from '../../data-access/payroll.service';
import { PayrollPeriodDto } from '../../models/payroll.models';

@Component({
  selector: 'app-payroll-periods-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './payroll-periods-page.component.html',
  styleUrl: './payroll-periods-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayrollPeriodsPageComponent implements OnInit {
  private readonly payrollService = inject(PayrollService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly periods = signal<PayrollPeriodDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly searchQuery = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isDrawerOpen = signal(false);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly periodForm = this.formBuilder.nonNullable.group({
    periodCode: ['', [Validators.required, Validators.maxLength(30)]],
    periodStart: ['', [Validators.required]],
    periodEnd: ['', [Validators.required]],
    attendanceCutoffDate: ['', [Validators.required]],
    payDate: ['', [Validators.required]],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'periodCode', label: 'รหัสงวด', isMono: true },
    { key: 'periodStart', label: 'วันเริ่มต้น', isMono: true },
    { key: 'periodEnd', label: 'วันสิ้นสุด', isMono: true },
    { key: 'attendanceCutoffDate', label: 'ตัดรอบเวลา', isMono: true },
    { key: 'payDate', label: 'วันจ่ายเงินเดือน', isMono: true },
    { key: 'status', label: 'สถานะงวด' },
  ];

  readonly tableActions: readonly DataTableAction<PayrollPeriodDto>[] = [
    {
      key: 'run',
      label: 'เปิดหน้าคำนวณ',
      onClick: (row) => void this.router.navigate(['/payroll/run', row.id]),
    },
  ];

  readonly filteredPeriods = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.periods();
    }
    return this.periods().filter((p) => p.periodCode.toLowerCase().includes(q));
  });

  ngOnInit(): void {
    this.loadPeriods();
  }

  loadPeriods(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.payrollService
      .getPeriods()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.periods.set(data);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.periods.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดงวดเงินเดือนได้'));
          this.isLoading.set(false);
        },
      });
  }

  openCreateDrawer(): void {
    this.formError.set(null);
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const lastDay = new Date(year, now.getMonth() + 1, 0).getDate();

    this.periodForm.reset({
      periodCode: `${year}-${month}`,
      periodStart: `${year}-${month}-01`,
      periodEnd: `${year}-${month}-${String(lastDay).padStart(2, '0')}`,
      attendanceCutoffDate: `${year}-${month}-25`,
      payDate: `${year}-${month}-${String(lastDay).padStart(2, '0')}`,
    });
    this.isDrawerOpen.set(true);
  }

  submitPeriod(): void {
    if (this.periodForm.invalid) {
      this.periodForm.markAllAsTouched();
      return;
    }

    const raw = this.periodForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);

    this.payrollService
      .createPeriod({
        periodCode: raw.periodCode.trim().toUpperCase(),
        periodStart: raw.periodStart,
        periodEnd: raw.periodEnd,
        attendanceCutoffDate: raw.attendanceCutoffDate,
        payDate: raw.payDate,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (created) => {
          this.isSubmitting.set(false);
          this.isDrawerOpen.set(false);
          this.feedbackSuccess.set(`สร้างงวดเงินเดือน ${created.periodCode} เรียบร้อยแล้ว`);
          this.loadPeriods();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
            const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.periodForm);
            if (unmapped.length > 0) {
              this.formError.set(unmapped[0]);
              return;
            }
          }
          this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถสร้างงวดเงินเดือนได้'));
        },
      });
  }

  openRunPage(row: PayrollPeriodDto): void {
    void this.router.navigate(['/payroll/run', row.id]);
  }

  formatDate(d: string): string {
    return formatDateOnlyThai(d);
  }

  getStatusLabel(status: string): string {
    return mapPayrollStatusLabel(status);
  }

  getStatusTone(status: string) {
    return mapPayrollStatusTone(status);
  }
}
