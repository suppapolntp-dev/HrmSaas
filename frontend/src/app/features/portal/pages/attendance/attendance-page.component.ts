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
import { RouterLink } from '@angular/router';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import {
  DataTableAction,
  DataTableColumn,
  DataTableSortEvent,
  RowAccentTone,
} from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import { PortalAttendanceService } from '../../data-access/portal-attendance.service';
import {
  AttendanceDailySummaryDto,
  formatDateOnlyThai,
  formatMinutesToDuration,
  formatUtcTime,
  mapAttendanceStatusLabel,
  mapAttendanceStatusTone,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-attendance-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './attendance-page.component.html',
  styleUrl: './attendance-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AttendancePageComponent implements OnInit {
  private readonly portalAttendanceService = inject(PortalAttendanceService);
  private readonly authSessionStore = inject(AuthSessionStore);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly records = signal<AttendanceDailySummaryDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly startDate = signal<string>(this.buildDefaultStartDate());
  readonly endDate = signal<string>(this.buildTodayDate());
  readonly statusFilter = signal<string>('');
  readonly searchQuery = signal<string>('');

  readonly sortBy = signal<string | null>('workDate');
  readonly sortDirection = signal<SortDirection | null>('desc');
  readonly page = signal<number>(1);
  readonly pageSize = signal<number>(this.appConfig.defaultPageSize);

  readonly isDrawerOpen = signal(false);
  readonly selectedRecord = signal<AttendanceDailySummaryDto | null>(null);
  readonly isSubmittingAdjustment = signal(false);
  readonly adjustmentError = signal<string | null>(null);
  readonly adjustmentSuccess = signal<string | null>(null);

  readonly adjustmentForm = this.formBuilder.nonNullable.group({
    workDate: ['', [Validators.required]],
    requestedInTime: ['09:00', [Validators.required]],
    requestedOutTime: ['18:00', [Validators.required]],
    reason: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'workDate', label: 'วันที่ทำงาน', sortable: true, isMono: true },
    { key: 'shiftName', label: 'กะงาน', sortable: true },
    { key: 'actualInUtc', label: 'เวลาเข้า', sortable: true, isMono: true, isNumeric: true },
    { key: 'actualOutUtc', label: 'เวลาออก', sortable: true, isMono: true, isNumeric: true },
    { key: 'workedMinutes', label: 'ชั่วโมงทำงาน', align: 'right', isNumeric: true, sortable: true },
    { key: 'lateMinutes', label: 'สาย / ออกก่อน', align: 'right', isNumeric: true, sortable: true },
    { key: 'attendanceStatus', label: 'สถานะ', sortable: true },
  ];

  readonly tableActions: readonly DataTableAction<AttendanceDailySummaryDto>[] = [
    {
      key: 'adjust',
      label: 'ขอแก้ไขเวลา',
      isVisible: (row) => !row.isLockedByPayroll,
      onClick: (row) => this.openAdjustmentDrawer(row),
    },
  ];

  readonly filteredRecords = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    const allRecords = this.records();

    if (query.length === 0) {
      return allRecords;
    }

    const matched: AttendanceDailySummaryDto[] = [];
    for (const item of allRecords) {
      const shiftLabel = (item.shiftName ?? '').toLowerCase();
      const statusLabel = mapAttendanceStatusLabel(item.attendanceStatus).toLowerCase();
      const dateText = formatDateOnlyThai(item.workDate).toLowerCase();

      if (
        item.workDate.toLowerCase().includes(query) ||
        dateText.includes(query) ||
        shiftLabel.includes(query) ||
        statusLabel.includes(query)
      ) {
        matched.push(item);
      }
    }
    return matched;
  });

  readonly sortedRecords = computed(() => {
    const list = [...this.filteredRecords()];
    const activeSortBy = this.sortBy();
    const activeDirection = this.sortDirection();

    if (!activeSortBy || !activeDirection) {
      return list;
    }

    const factor = activeDirection === 'asc' ? 1 : -1;
    list.sort((a, b) => {
      switch (activeSortBy) {
        case 'workDate':
          return a.workDate.localeCompare(b.workDate) * factor;
        case 'shiftName':
          return (a.shiftName ?? '').localeCompare(b.shiftName ?? '') * factor;
        case 'actualInUtc':
          return (a.actualInUtc ?? '').localeCompare(b.actualInUtc ?? '') * factor;
        case 'actualOutUtc':
          return (a.actualOutUtc ?? '').localeCompare(b.actualOutUtc ?? '') * factor;
        case 'workedMinutes':
          return (a.workedMinutes - b.workedMinutes) * factor;
        case 'lateMinutes':
          return ((a.lateMinutes + a.earlyOutMinutes) - (b.lateMinutes + b.earlyOutMinutes)) * factor;
        case 'attendanceStatus':
          return a.attendanceStatus.localeCompare(b.attendanceStatus) * factor;
        default:
          return 0;
      }
    });

    return list;
  });

  readonly pagedRecords = computed(() => {
    const all = this.sortedRecords();
    const currentPage = this.page();
    const size = Math.max(1, this.pageSize());
    const startIndex = (currentPage - 1) * size;
    return all.slice(startIndex, startIndex + size);
  });

  readonly summaryStats = computed(() => {
    const items = this.records();
    let totalDays = 0;
    let lateCount = 0;
    let exceptionCount = 0;
    let totalWorkedMinutes = 0;

    for (const item of items) {
      totalDays++;
      totalWorkedMinutes += item.workedMinutes;
      if (item.attendanceStatus === 'LATE') {
        lateCount++;
      }
      if (item.attendanceStatus === 'MISSING_PUNCH' || item.attendanceStatus === 'ABSENT') {
        exceptionCount++;
      }
    }

    return {
      totalDays,
      lateCount,
      exceptionCount,
      totalWorkedText: formatMinutesToDuration(totalWorkedMinutes),
    };
  });

  readonly activeBranchName = computed(() => {
    const companyId = this.authSessionStore.activeCompanyId();
    const branchId = this.authSessionStore.activeBranchId();
    const companies = this.authSessionStore.companies();
    for (const company of companies) {
      if (company.companyId !== companyId) {
        continue;
      }
      for (const branch of company.branches) {
        if (branch.branchId === branchId) {
          return branch.name;
        }
      }
      return company.legalName;
    }
    return '';
  });

  readonly rowAccentFn = (row: AttendanceDailySummaryDto): RowAccentTone => {
    if (row.attendanceStatus === 'MISSING_PUNCH' || row.attendanceStatus === 'ABSENT') {
      return 'danger';
    }
    if (row.attendanceStatus === 'LATE' || row.attendanceStatus === 'EARLY_OUT') {
      return 'warning';
    }
    return null;
  };

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.portalAttendanceService
      .getAttendanceHistory(this.startDate(), this.endDate(), this.statusFilter())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.records.set(data);
          this.page.set(1);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.records.set([]);
          this.errorMessage.set(this.extractErrorMessage(err, 'ไม่สามารถโหลดประวัติการลงเวลาได้'));
          this.isLoading.set(false);
        },
      });
  }

  onStartDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.startDate.set(input.value);
    this.loadHistory();
  }

  onEndDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.endDate.set(input.value);
    this.loadHistory();
  }

  onStatusFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.statusFilter.set(select.value);
    this.loadHistory();
  }

  resetFilters(): void {
    this.startDate.set(this.buildDefaultStartDate());
    this.endDate.set(this.buildTodayDate());
    this.statusFilter.set('');
    this.searchQuery.set('');
    this.loadHistory();
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query);
    this.page.set(1);
  }

  onSortChange(event: DataTableSortEvent): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.page.set(1);
  }

  onPageChange(nextPage: number): void {
    this.page.set(nextPage);
  }

  onPageSizeChange(nextSize: number): void {
    this.pageSize.set(nextSize);
    this.page.set(1);
  }

  openAdjustmentDrawer(record: AttendanceDailySummaryDto | null): void {
    this.selectedRecord.set(record);
    this.adjustmentError.set(null);

    const workDateValue = record ? record.workDate : this.buildTodayDate();
    const inTime = record && record.actualInUtc ? this.extractLocalTimeInput(record.actualInUtc) : '09:00';
    const outTime = record && record.actualOutUtc ? this.extractLocalTimeInput(record.actualOutUtc) : '18:00';

    this.adjustmentForm.reset({
      workDate: workDateValue,
      requestedInTime: inTime,
      requestedOutTime: outTime,
      reason: '',
    });
    this.isDrawerOpen.set(true);
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
    this.selectedRecord.set(null);
    this.adjustmentError.set(null);
  }

  submitTimeAdjustment(): void {
    if (this.adjustmentForm.invalid) {
      this.adjustmentForm.markAllAsTouched();
      return;
    }

    const raw = this.adjustmentForm.getRawValue();
    const requestedInIso = this.combineBangkokDateTimeToIso(raw.workDate, raw.requestedInTime);
    const requestedOutIso = this.combineBangkokDateTimeToIso(raw.workDate, raw.requestedOutTime);

    this.isSubmittingAdjustment.set(true);
    this.adjustmentError.set(null);
    this.adjustmentSuccess.set(null);

    this.portalAttendanceService
      .requestTimeAdjustment({
        workDate: raw.workDate,
        requestedInUtc: requestedInIso,
        requestedOutUtc: requestedOutIso,
        reason: raw.reason.trim(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmittingAdjustment.set(false);
          this.isDrawerOpen.set(false);
          this.adjustmentSuccess.set(
            `ส่งคำขอแก้ไขเวลาวันที่ ${formatDateOnlyThai(raw.workDate)} เข้าสู่สายอนุมัติแล้ว`,
          );
        },
        error: (err: unknown) => {
          this.isSubmittingAdjustment.set(false);
          if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
            const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.adjustmentForm);
            if (unmapped.length > 0) {
              this.adjustmentError.set(unmapped[0]);
              return;
            }
          }
          this.adjustmentError.set(this.extractErrorMessage(err, 'ไม่สามารถส่งคำขอแก้ไขเวลาได้'));
        },
      });
  }

  formatDate(dateText: string): string {
    return formatDateOnlyThai(dateText);
  }

  formatTime(isoText: string | null): string {
    return formatUtcTime(isoText);
  }

  formatDuration(minutes: number): string {
    return formatMinutesToDuration(minutes);
  }

  getStatusLabel(status: string): string {
    return mapAttendanceStatusLabel(status);
  }

  getStatusTone(status: string) {
    return mapAttendanceStatusTone(status);
  }

  private buildTodayDate(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private buildDefaultStartDate(): string {
    const now = new Date();
    now.setDate(now.getDate() - 29);
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private extractLocalTimeInput(isoText: string): string {
    const formatted = formatUtcTime(isoText);
    if (formatted === '-' || formatted.length < 5) {
      return '09:00';
    }
    return formatted.slice(0, 5);
  }

  private combineBangkokDateTimeToIso(workDate: string, timeText: string): string {
    const normalizedTime = timeText.length === 5 ? `${timeText}:00` : timeText;
    return new Date(`${workDate}T${normalizedTime}+07:00`).toISOString();
  }

  private extractErrorMessage(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse) {
      const body = err.error as Partial<ProblemDetails> | null;
      if (body?.detail && body.detail.trim().length > 0) {
        return body.detail;
      }
      if (body?.title && body.title.trim().length > 0) {
        return body.title;
      }
    }
    return fallback;
  }
}
