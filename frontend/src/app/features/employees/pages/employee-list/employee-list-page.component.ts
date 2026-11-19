import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { HasPermissionDirective } from '../../../../core/directives/has-permission.directive';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import {
  DataTableAction,
  DataTableColumn,
  DataTableSortEvent,
} from '../../../../shared/ui/data-table/data-table.models';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SortDirection } from '../../../../shared/utils/pagination.helper';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import { BranchDto, DepartmentDto } from '../../../organization/models/organization.models';
import { formatDateOnlyThai } from '../../../portal/models/portal-dashboard.models';
import { EmployeesService } from '../../data-access/employees.service';
import {
  EmployeeListItemDto,
  mapEmploymentStatusLabel,
  mapEmploymentStatusTone,
} from '../../models/employee.models';

@Component({
  selector: 'app-employee-list-page',
  standalone: true,
  imports: [
    RouterLink,
    HasPermissionDirective,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
  ],
  templateUrl: './employee-list-page.component.html',
  styleUrl: './employee-list-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeeListPageComponent implements OnInit {
  private readonly employeesService = inject(EmployeesService);
  private readonly organizationService = inject(OrganizationService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly employees = signal<EmployeeListItemDto[]>([]);
  readonly totalCount = signal(0);
  readonly branches = signal<BranchDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly branchFilter = signal('');
  readonly departmentFilter = signal('');
  readonly statusFilter = signal('');
  readonly searchQuery = signal('');
  readonly sortBy = signal<string | null>('employeeCode');
  readonly sortDirection = signal<SortDirection | null>('asc');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly columns: readonly DataTableColumn[] = [
    { key: 'employeeCode', label: 'รหัสพนักงาน', sortable: true, isMono: true },
    { key: 'firstName', label: 'ชื่อ-สกุล', sortable: true },
    { key: 'branchName', label: 'สาขา' },
    { key: 'departmentName', label: 'แผนก' },
    { key: 'positionTitle', label: 'ตำแหน่ง' },
    { key: 'hireDate', label: 'วันเริ่มงาน', sortable: true, isMono: true },
    { key: 'employmentStatus', label: 'สถานะ' },
  ];

  readonly tableActions: readonly DataTableAction<EmployeeListItemDto>[] = [
    {
      key: 'detail',
      label: 'ดูโปรไฟล์ 360',
      onClick: (row) => this.openDetail(row),
    },
  ];

  ngOnInit(): void {
    this.loadInitial();
  }

  loadInitial(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      branches: this.organizationService.getBranches(),
      departments: this.organizationService.getDepartments(),
      employees: this.employeesService.getEmployees(this.buildQuery()),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.branches.set(res.branches);
          this.departments.set(res.departments);
          this.employees.set(res.employees.items);
          this.totalCount.set(res.employees.totalCount);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.employees.set([]);
          this.totalCount.set(0);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดรายชื่อพนักงานได้'));
          this.isLoading.set(false);
        },
      });
  }

  loadEmployees(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.employeesService
      .getEmployees(this.buildQuery())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.employees.set(res.items);
          this.totalCount.set(res.totalCount);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.employees.set([]);
          this.totalCount.set(0);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดรายชื่อพนักงานได้'));
          this.isLoading.set(false);
        },
      });
  }

  onBranchChange(event: Event): void {
    this.branchFilter.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
    this.loadEmployees();
  }

  onDepartmentChange(event: Event): void {
    this.departmentFilter.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
    this.loadEmployees();
  }

  onStatusChange(event: Event): void {
    this.statusFilter.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
    this.loadEmployees();
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query);
    this.page.set(1);
    this.loadEmployees();
  }

  onSortChange(event: DataTableSortEvent): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.page.set(1);
    this.loadEmployees();
  }

  onPageChange(nextPage: number): void {
    this.page.set(nextPage);
    this.loadEmployees();
  }

  onPageSizeChange(nextSize: number): void {
    this.pageSize.set(nextSize);
    this.page.set(1);
    this.loadEmployees();
  }

  openDetail(row: EmployeeListItemDto): void {
    void this.router.navigate(['/employees', row.id]);
  }

  formatDate(dateText: string): string {
    return formatDateOnlyThai(dateText);
  }

  getStatusLabel(status: string): string {
    return mapEmploymentStatusLabel(status);
  }

  getStatusTone(status: string) {
    return mapEmploymentStatusTone(status);
  }

  private buildQuery() {
    return {
      page: this.page(),
      pageSize: this.pageSize(),
      branchId: this.branchFilter() || null,
      departmentId: this.departmentFilter() || null,
      status: this.statusFilter() || null,
      search: this.searchQuery() || null,
      sortBy: this.sortBy(),
      sortDesc: this.sortDirection() === 'desc',
    };
  }
}
