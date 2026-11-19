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
import { forkJoin } from 'rxjs';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableSortEvent,
  RowAccentTone,
} from '../../../../shared/ui/data-table/data-table.models';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import { BranchDto, DepartmentDto } from '../../../organization/models/organization.models';
import {
  AttendanceDailySummaryDto,
  formatDateOnlyThai,
  formatMinutesToDuration,
  formatUtcTime,
  mapAttendanceStatusLabel,
  mapAttendanceStatusTone,
} from '../../../portal/models/portal-dashboard.models';
import { AttendanceAdminService } from '../../data-access/attendance-admin.service';

@Component({
  selector: 'app-daily-logs-page',
  standalone: true,
  imports: [DataTableComponent, StateContainerComponent, StatusIndicatorComponent],
  templateUrl: './daily-logs-page.component.html',
  styleUrl: './daily-logs-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DailyAttendanceLogsPageComponent implements OnInit {
  private readonly attendanceAdminService = inject(AttendanceAdminService);
  private readonly organizationService = inject(OrganizationService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly destroyRef = inject(DestroyRef);

  readonly logs = signal<AttendanceDailySummaryDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);

  readonly startDate = signal(this.daysAgoIso(7));
  readonly endDate = signal(this.daysAgoIso(0));
  readonly branchFilter = signal('');
  readonly departmentFilter = signal('');
  readonly statusFilter = signal('');
  readonly searchQuery = signal('');

  readonly sortBy = signal<string | null>('workDate');
  readonly sortDirection = signal<SortDirection | null>('desc');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly columns: readonly DataTableColumn[] = [
    { key: 'workDate', label: 'วันที่', sortable: true, isMono: true },
    { key: 'employeeCode', label: 'รหัส', sortable: true, isMono: true },
    { key: 'employeeName', label: 'พนักงาน', sortable: true },
    { key: 'shiftName', label: 'กะงาน' },
    { key: 'actualInUtc', label: 'เข้างาน', isMono: true },
    { key: 'actualOutUtc', label: 'ออกงาน', isMono: true },
    { key: 'workedMinutes', label: 'ชั่วโมงทำงาน', align: 'right', isNumeric: true },
    { key: 'attendanceStatus', label: 'สถานะ', sortable: true },
  ];

  readonly rowAccentFn = (row: AttendanceDailySummaryDto): RowAccentTone => {
    if (row.attendanceStatus === 'MISSING_PUNCH' || row.attendanceStatus === 'ABSENT') {
      return 'danger';
    }
    if (row.attendanceStatus === 'LATE' || row.attendanceStatus === 'EARLY_OUT') {
      return 'warning';
    }
    return null;
  };

  readonly stats = computed(() => {
    let total = 0;
    let late = 0;
    let earlyOut = 0;
    let missingOrAbsent = 0;
    for (const item of this.logs()) {
      total++;
      if (item.attendanceStatus === 'LATE') {
        late++;
      } else if (item.attendanceStatus === 'EARLY_OUT') {
        earlyOut++;
      } else if (item.attendanceStatus === 'MISSING_PUNCH' || item.attendanceStatus === 'ABSENT') {
        missingOrAbsent++;
      }
    }
    return { total, late, earlyOut, missingOrAbsent };
  });

  readonly filteredLogs = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.logs();
    }
    return this.logs().filter(
      (l) =>
        l.employeeCode.toLowerCase().includes(q) ||
        l.employeeName.toLowerCase().includes(q) ||
        l.workDate.toLowerCase().includes(q),
    );
  });

  readonly sortedLogs = computed(() => {
    const list = [...this.filteredLogs()];
    const activeSort = this.sortBy();
    const dir = this.sortDirection();
    if (!activeSort || !dir) {
      return list;
    }
    const factor = dir === 'asc' ? 1 : -1;
    list.sort((a, b) => {
      switch (activeSort) {
        case 'workDate':
          return a.workDate.localeCompare(b.workDate) * factor;
        case 'employeeCode':
          return a.employeeCode.localeCompare(b.employeeCode) * factor;
        case 'employeeName':
          return a.employeeName.localeCompare(b.employeeName) * factor;
        case 'attendanceStatus':
          return a.attendanceStatus.localeCompare(b.attendanceStatus) * factor;
        default:
          return 0;
      }
    });
    return list;
  });

  readonly pagedLogs = computed(() => {
    const start = (this.page() - 1) * Math.max(1, this.pageSize());
    return this.sortedLogs().slice(start, start + Math.max(1, this.pageSize()));
  });

  ngOnInit(): void {
    this.loadInitial();
  }

  loadInitial(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      branches: this.organizationService.getBranches(),
      departments: this.organizationService.getDepartments(),
      logs: this.attendanceAdminService.getDailySummaries(
        this.startDate(),
        this.endDate(),
        this.branchFilter() || null,
        this.departmentFilter() || null,
        this.statusFilter() || null,
      ),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.branches.set(res.branches);
          this.departments.set(res.departments);
          this.logs.set(res.logs);
          this.page.set(1);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.logs.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดบันทึกเวลาประจำวันได้'));
          this.isLoading.set(false);
        },
      });
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.attendanceAdminService
      .getDailySummaries(
        this.startDate(),
        this.endDate(),
        this.branchFilter() || null,
        this.departmentFilter() || null,
        this.statusFilter() || null,
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.logs.set(data);
          this.page.set(1);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.logs.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดบันทึกเวลาประจำวันได้'));
          this.isLoading.set(false);
        },
      });
  }

  onSortChange(event: DataTableSortEvent): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.page.set(1);
  }

  formatDate(d: string): string {
    return formatDateOnlyThai(d);
  }

  formatTime(iso: string | null): string {
    return formatUtcTime(iso);
  }

  formatDuration(min: number): string {
    return formatMinutesToDuration(min);
  }

  getStatusLabel(status: string): string {
    return mapAttendanceStatusLabel(status);
  }

  getStatusTone(status: string) {
    return mapAttendanceStatusTone(status);
  }

  private daysAgoIso(days: number): string {
    const dt = new Date();
    dt.setDate(dt.getDate() - days);
    const y = dt.getFullYear();
    const m = String(dt.getMonth() + 1).padStart(2, '0');
    const d = String(dt.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
