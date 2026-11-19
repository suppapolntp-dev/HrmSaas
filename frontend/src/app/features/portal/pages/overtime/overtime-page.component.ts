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
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableColumn, DataTableSortEvent } from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import { PortalOvertimeService } from '../../data-access/portal-overtime.service';
import {
  ApprovalRequestDto,
  formatDateOnlyThai,
  formatUtcDateTime,
  mapApprovalStatusLabel,
  mapApprovalStatusTone,
  parseApprovalPayloadSummary,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-overtime-page',
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
  templateUrl: './overtime-page.component.html',
  styleUrl: './overtime-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OvertimePageComponent implements OnInit {
  private readonly portalOvertimeService = inject(PortalOvertimeService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

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

  readonly otForm = this.formBuilder.nonNullable.group({
    workDate: [this.todayIso(), [Validators.required]],
    startTime: ['18:00', [Validators.required]],
    endTime: ['20:00', [Validators.required]],
    hours: [2, [Validators.required, Validators.min(0.5), Validators.max(16)]],
    reason: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(500)]],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'submittedAt', label: 'วันที่ยื่น', sortable: true, isMono: true },
    { key: 'payloadSummary', label: 'รายละเอียด OT' },
    { key: 'currentStepOrder', label: 'ลำดับอนุมัติ', align: 'right', isNumeric: true },
    { key: 'status', label: 'สถานะ', sortable: true },
  ];

  readonly filteredRequests = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    const status = this.statusFilter();
    const matched: ApprovalRequestDto[] = [];

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
      matched.push(req);
    }
    return matched;
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
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.portalOvertimeService
      .getMyRequests()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.requests.set(data);
          this.page.set(1);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.requests.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดคำขอ OT ได้'));
          this.isLoading.set(false);
        },
      });
  }

  openCreateDrawer(): void {
    this.formError.set(null);
    this.otForm.reset({
      workDate: this.todayIso(),
      startTime: '18:00',
      endTime: '20:00',
      hours: 2,
      reason: '',
    });
    this.isDrawerOpen.set(true);
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
    this.formError.set(null);
  }

  submitOvertime(): void {
    if (this.otForm.invalid) {
      this.otForm.markAllAsTouched();
      return;
    }

    const raw = this.otForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);
    this.feedbackSuccess.set(null);

    this.portalOvertimeService
      .submitOvertimeRequest({
        workDate: raw.workDate,
        startTime: this.toTimeOnlyString(raw.startTime),
        endTime: this.toTimeOnlyString(raw.endTime),
        hours: Number(raw.hours),
        reason: raw.reason.trim(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isDrawerOpen.set(false);
          this.feedbackSuccess.set(
            `ยื่นคำขอทำ OT วันที่ ${formatDateOnlyThai(raw.workDate)} เรียบร้อยแล้ว`,
          );
          this.loadRequests();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
            const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.otForm);
            if (unmapped.length > 0) {
              this.formError.set(unmapped[0]);
              return;
            }
          }
          this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถยื่นคำขอ OT ได้'));
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

  private toTimeOnlyString(value: string): string {
    return value.length === 5 ? `${value}:00` : value;
  }

  private todayIso(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
