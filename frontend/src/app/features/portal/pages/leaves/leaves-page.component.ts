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
import { forkJoin } from 'rxjs';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import {
  DataTableAction,
  DataTableColumn,
  DataTableSortEvent,
} from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import { PortalLeavesService } from '../../data-access/portal-leaves.service';
import {
  ApprovalRequestDto,
  MyLeaveBalanceDto,
  formatDateOnlyThai,
  formatUtcDateTime,
  mapApprovalStatusLabel,
  mapApprovalStatusTone,
  parseApprovalPayloadSummary,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-leaves-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    ConfirmDialogComponent,
    FormFieldComponent,
  ],
  templateUrl: './leaves-page.component.html',
  styleUrl: './leaves-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LeavesPageComponent implements OnInit {
  private readonly portalLeavesService = inject(PortalLeavesService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly balances = signal<MyLeaveBalanceDto[]>([]);
  readonly requests = signal<ApprovalRequestDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly searchQuery = signal('');
  readonly statusFilter = signal('');
  readonly sortBy = signal<string | null>('submittedAt');
  readonly sortDirection = signal<SortDirection | null>('desc');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isDrawerOpen = signal(false);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly cancelTarget = signal<ApprovalRequestDto | null>(null);
  readonly isCancelling = signal(false);

  readonly leaveForm = this.formBuilder.nonNullable.group({
    leaveTypeId: ['', [Validators.required]],
    startDate: [this.todayIso(), [Validators.required]],
    endDate: [this.todayIso(), [Validators.required]],
    daysCount: [1, [Validators.required, Validators.min(0.5)]],
    reason: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(500)]],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'submittedAt', label: 'วันที่ยื่น', sortable: true, isMono: true },
    { key: 'payloadSummary', label: 'รายละเอียดการลา' },
    { key: 'currentStepOrder', label: 'ลำดับอนุมัติ', align: 'right', isNumeric: true },
    { key: 'status', label: 'สถานะ', sortable: true },
  ];

  readonly tableActions: readonly DataTableAction<ApprovalRequestDto>[] = [
    {
      key: 'cancel',
      label: 'ยกเลิกคำขอ',
      variant: 'danger',
      isVisible: (row) => row.status === 'PENDING',
      onClick: (row) => this.cancelTarget.set(row),
    },
  ];

  readonly filteredRequests = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    const status = this.statusFilter();
    const result: ApprovalRequestDto[] = [];

    for (const req of this.requests()) {
      if (status && req.status !== status) {
        continue;
      }
      if (query.length > 0) {
        const summary = parseApprovalPayloadSummary(req.payloadSummary).toLowerCase();
        const statusLabel = mapApprovalStatusLabel(req.status).toLowerCase();
        if (!summary.includes(query) && !statusLabel.includes(query)) {
          continue;
        }
      }
      result.push(req);
    }
    return result;
  });

  readonly sortedRequests = computed(() => {
    const list = [...this.filteredRequests()];
    const activeSort = this.sortBy();
    const dir = this.sortDirection();
    if (!activeSort || !dir) {
      return list;
    }
    const factor = dir === 'asc' ? 1 : -1;
    list.sort((a, b) => {
      if (activeSort === 'submittedAt') {
        return a.submittedAt.localeCompare(b.submittedAt) * factor;
      }
      if (activeSort === 'status') {
        return a.status.localeCompare(b.status) * factor;
      }
      return 0;
    });
    return list;
  });

  readonly pagedRequests = computed(() => {
    const start = (this.page() - 1) * Math.max(1, this.pageSize());
    return this.sortedRequests().slice(start, start + Math.max(1, this.pageSize()));
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      balances: this.portalLeavesService.getMyBalances(),
      requests: this.portalLeavesService.getMyRequests(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.balances.set(res.balances);
          this.requests.set(res.requests);
          this.page.set(1);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.balances.set([]);
          this.requests.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดข้อมูลการลาได้'));
          this.isLoading.set(false);
        },
      });
  }

  openCreateDrawer(): void {
    this.formError.set(null);
    const firstBalance = this.balances()[0];
    this.leaveForm.reset({
      leaveTypeId: firstBalance ? firstBalance.leaveTypeId : '',
      startDate: this.todayIso(),
      endDate: this.todayIso(),
      daysCount: 1,
      reason: '',
    });
    this.isDrawerOpen.set(true);
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
    this.formError.set(null);
  }

  submitLeave(): void {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }

    const raw = this.leaveForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);
    this.feedbackSuccess.set(null);

    this.portalLeavesService
      .submitLeaveRequest({
        leaveTypeId: raw.leaveTypeId,
        startDate: raw.startDate,
        endDate: raw.endDate,
        daysCount: Number(raw.daysCount),
        reason: raw.reason.trim(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isDrawerOpen.set(false);
          this.feedbackSuccess.set(
            `ยื่นใบลาวันที่ ${formatDateOnlyThai(raw.startDate)} เรียบร้อยแล้ว`,
          );
          this.loadData();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
            const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.leaveForm);
            if (unmapped.length > 0) {
              this.formError.set(unmapped[0]);
              return;
            }
          }
          this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถยื่นใบลาได้'));
        },
      });
  }

  confirmCancelRequest(): void {
    const target = this.cancelTarget();
    if (!target) {
      return;
    }

    this.isCancelling.set(true);
    this.feedbackSuccess.set(null);

    this.portalLeavesService
      .cancelLeaveRequest(target.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isCancelling.set(false);
          this.cancelTarget.set(null);
          this.feedbackSuccess.set('ยกเลิกคำขอลาและคืนโควตาเรียบร้อยแล้ว');
          this.loadData();
        },
        error: (err: unknown) => {
          this.isCancelling.set(false);
          this.cancelTarget.set(null);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถยกเลิกคำขอลาได้'));
        },
      });
  }

  onStatusChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.statusFilter.set(select.value);
    this.page.set(1);
  }

  onSortChange(event: DataTableSortEvent): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.page.set(1);
  }

  formatSubmitted(iso: string): string {
    return formatUtcDateTime(iso);
  }

  formatPayload(json: string): string {
    return parseApprovalPayloadSummary(json);
  }

  getStatusLabel(status: string): string {
    return mapApprovalStatusLabel(status);
  }

  getStatusTone(status: string) {
    return mapApprovalStatusTone(status);
  }

  private todayIso(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
