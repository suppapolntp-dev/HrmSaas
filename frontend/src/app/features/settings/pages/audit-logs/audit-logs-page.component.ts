import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableAction, DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { formatUtcDateTime } from '../../../portal/models/portal-dashboard.models';
import { SettingsService } from '../../data-access/settings.service';
import { AuditLogDto } from '../../models/settings.models';

@Component({
  selector: 'app-audit-logs-page',
  standalone: true,
  imports: [DataTableComponent, StateContainerComponent, SlideDrawerComponent],
  templateUrl: './audit-logs-page.component.html',
  styleUrl: './audit-logs-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditLogsPageComponent implements OnInit {
  private readonly settingsService = inject(SettingsService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly destroyRef = inject(DestroyRef);

  readonly logs = signal<AuditLogDto[]>([]);
  readonly totalCount = signal(0);
  readonly selectedLog = signal<AuditLogDto | null>(null);

  readonly entityFilter = signal('');
  readonly actionFilter = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly columns: readonly DataTableColumn[] = [
    { key: 'occurredAt', label: 'เวลาที่เกิดรายการ', isMono: true },
    { key: 'entityName', label: 'ข้อมูล / ตาราง', isMono: true },
    { key: 'action', label: 'การกระทำ', isMono: true },
    { key: 'ipAddress', label: 'IP Address', isMono: true },
    { key: 'correlationId', label: 'Correlation ID', isMono: true },
  ];

  readonly tableActions: readonly DataTableAction<AuditLogDto>[] = [
    { key: 'diff', label: 'ดู Diff ค่า Old / New', onClick: (row) => this.selectedLog.set(row) },
  ];

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.settingsService
      .getAuditLogs({
        page: this.page(),
        pageSize: this.pageSize(),
        entityName: this.entityFilter() || null,
        action: this.actionFilter() || null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.logs.set(res.items);
          this.totalCount.set(res.totalCount);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.logs.set([]);
          this.totalCount.set(0);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลด Audit Logs ได้'));
          this.isLoading.set(false);
        },
      });
  }

  onFilterChange(): void {
    this.page.set(1);
    this.loadLogs();
  }

  onPageChange(nextPage: number): void {
    this.page.set(nextPage);
    this.loadLogs();
  }

  onPageSizeChange(nextSize: number): void {
    this.pageSize.set(nextSize);
    this.page.set(1);
    this.loadLogs();
  }

  formatDateTime(iso: string): string {
    return formatUtcDateTime(iso);
  }

  formatJsonSnapshot(raw: string | null): string {
    if (!raw || raw.trim().length === 0) {
      return '-';
    }
    try {
      return JSON.stringify(JSON.parse(raw), null, 2);
    } catch {
      return raw;
    }
  }
}
