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
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
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
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import {
  ApprovalRequestDto,
  formatUtcDateTime,
  mapApprovalStatusLabel,
  mapApprovalStatusTone,
  mapRequestTypeLabel,
  parseApprovalPayloadSummary,
} from '../../../portal/models/portal-dashboard.models';
import { WorkflowsService } from '../../data-access/workflows.service';

@Component({
  selector: 'app-approval-inbox-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './approval-inbox-page.component.html',
  styleUrl: './approval-inbox-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApprovalInboxPageComponent implements OnInit {
  private readonly workflowsService = inject(WorkflowsService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly activeTab = signal<'PENDING' | 'HISTORY'>('PENDING');
  readonly pendingItems = signal<ApprovalRequestDto[]>([]);
  readonly historyItems = signal<ApprovalRequestDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly requestTypeFilter = signal('');
  readonly searchQuery = signal('');
  readonly sortBy = signal<string | null>('submittedAt');
  readonly sortDirection = signal<SortDirection | null>('desc');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly decidingItem = signal<ApprovalRequestDto | null>(null);
  readonly decidingAction = signal<'APPROVE' | 'REJECT'>('APPROVE');
  readonly isSubmitting = signal(false);
  readonly decideError = signal<string | null>(null);

  readonly decideForm = this.formBuilder.nonNullable.group({
    comment: [''],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'submittedAt', label: 'วันที่ยื่น', sortable: true, isMono: true },
    { key: 'requesterEmployeeCode', label: 'รหัส', isMono: true },
    { key: 'requesterFullName', label: 'ผู้ยื่นคำขอ', sortable: true },
    { key: 'requestType', label: 'ประเภทคำขอ', sortable: true },
    { key: 'payloadSummary', label: 'รายละเอียด' },
    { key: 'status', label: 'สถานะ' },
  ];

  readonly pendingActions: readonly DataTableAction<ApprovalRequestDto>[] = [
    {
      key: 'approve',
      label: 'อนุมัติ',
      onClick: (row) => this.openDecideDrawer(row, 'APPROVE'),
    },
    {
      key: 'reject',
      label: 'ปฏิเสธ',
      variant: 'danger',
      onClick: (row) => this.openDecideDrawer(row, 'REJECT'),
    },
  ];

  readonly activeRows = computed(() => {
    const source = this.activeTab() === 'PENDING' ? this.pendingItems() : this.historyItems();
    const typeFilter = this.requestTypeFilter();
    const q = this.searchQuery().trim().toLowerCase();

    const matched: ApprovalRequestDto[] = [];
    for (const item of source) {
      if (typeFilter && item.requestType !== typeFilter) {
        continue;
      }
      if (q.length > 0) {
        const summary = parseApprovalPayloadSummary(item.payloadSummary).toLowerCase();
        if (
          !item.requesterFullName.toLowerCase().includes(q) &&
          !item.requesterEmployeeCode.toLowerCase().includes(q) &&
          !summary.includes(q)
        ) {
          continue;
        }
      }
      matched.push(item);
    }

    const activeSort = this.sortBy();
    const dir = this.sortDirection();
    if (activeSort && dir) {
      const factor = dir === 'asc' ? 1 : -1;
      matched.sort((a, b) => {
        switch (activeSort) {
          case 'submittedAt':
            return a.submittedAt.localeCompare(b.submittedAt) * factor;
          case 'requesterFullName':
            return a.requesterFullName.localeCompare(b.requesterFullName) * factor;
          case 'requestType':
            return a.requestType.localeCompare(b.requestType) * factor;
          default:
            return 0;
        }
      });
    }
    return matched;
  });

  readonly pagedRows = computed(() => {
    const start = (this.page() - 1) * Math.max(1, this.pageSize());
    return this.activeRows().slice(start, start + Math.max(1, this.pageSize()));
  });

  ngOnInit(): void {
    this.loadInbox();
  }

  loadInbox(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      pending: this.workflowsService.getPendingApprovals(),
      history: this.workflowsService.getApprovalHistory(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.pendingItems.set(res.pending);
          this.historyItems.set(res.history);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.pendingItems.set([]);
          this.historyItems.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดกล่องอนุมัติได้'));
          this.isLoading.set(false);
        },
      });
  }

  openDecideDrawer(item: ApprovalRequestDto, action: 'APPROVE' | 'REJECT'): void {
    this.decidingItem.set(item);
    this.decidingAction.set(action);
    this.decideError.set(null);
    this.decideForm.reset({ comment: '' });
  }

  submitDecision(): void {
    const target = this.decidingItem();
    if (!target) {
      return;
    }

    const action = this.decidingAction();
    const commentText = this.decideForm.controls.comment.value.trim();
    if (action === 'REJECT' && commentText.length === 0) {
      this.decideError.set('กรุณาระบุเหตุผลประกอบการปฏิเสธคำขอ');
      return;
    }

    this.isSubmitting.set(true);
    this.decideError.set(null);

    this.workflowsService
      .decideApproval(target.id, {
        action,
        comment: commentText.length > 0 ? commentText : null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.decidingItem.set(null);
          this.feedbackSuccess.set(
            action === 'APPROVE'
              ? `อนุมัติคำขอของ ${target.requesterFullName} เรียบร้อยแล้ว`
              : `ปฏิเสธคำขอของ ${target.requesterFullName} เรียบร้อยแล้ว`,
          );
          this.loadInbox();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          this.decideError.set(extractProblemDetailsMessage(err, 'ไม่สามารถบันทึกการพิจารณาได้'));
        },
      });
  }

  onSortChange(event: DataTableSortEvent): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.page.set(1);
  }

  formatDateTime(iso: string): string {
    return formatUtcDateTime(iso);
  }

  formatType(type: string): string {
    return mapRequestTypeLabel(type);
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
}
