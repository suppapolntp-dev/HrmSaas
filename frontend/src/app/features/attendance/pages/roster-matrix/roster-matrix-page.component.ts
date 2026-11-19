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
import { forkJoin } from 'rxjs';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { EmployeesService } from '../../../employees/data-access/employees.service';
import { EmployeeListItemDto } from '../../../employees/models/employee.models';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import { BranchDto, DepartmentDto } from '../../../organization/models/organization.models';
import {
  formatDateOnlyThai,
  mapDayTypeLabel,
  mapDayTypeTone,
} from '../../../portal/models/portal-dashboard.models';
import { AttendanceAdminService } from '../../data-access/attendance-admin.service';
import { RosterItemDto, ShiftDefinitionDto } from '../../models/attendance.models';

interface MatrixEmployeeRow {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  cells: Record<string, RosterItemDto | undefined>;
}

@Component({
  selector: 'app-roster-matrix-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './roster-matrix-page.component.html',
  styleUrl: './roster-matrix-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RosterMatrixPageComponent implements OnInit {
  private readonly attendanceAdminService = inject(AttendanceAdminService);
  private readonly organizationService = inject(OrganizationService);
  private readonly employeesService = inject(EmployeesService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly startDate = signal(this.todayIso());
  readonly endDate = signal(this.addDaysIso(this.todayIso(), 6));
  readonly branchFilter = signal('');
  readonly departmentFilter = signal('');

  readonly rosters = signal<RosterItemDto[]>([]);
  readonly shifts = signal<ShiftDefinitionDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly employees = signal<EmployeeListItemDto[]>([]);

  readonly selectedEmployeeIds = signal<string[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly isDrawerOpen = signal(false);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly bulkForm = this.formBuilder.nonNullable.group({
    startDate: [this.todayIso(), [Validators.required]],
    endDate: [this.addDaysIso(this.todayIso(), 6), [Validators.required]],
    dayType: ['WORKDAY', [Validators.required]],
    shiftDefinitionId: [''],
  });

  readonly dateColumns = computed(() => {
    const start = new Date(`${this.startDate()}T00:00:00`);
    const end = new Date(`${this.endDate()}T00:00:00`);
    const cols: string[] = [];
    const cursor = new Date(start);
    let safety = 0;

    while (cursor <= end && safety < 14) {
      const y = cursor.getFullYear();
      const m = String(cursor.getMonth() + 1).padStart(2, '0');
      const d = String(cursor.getDate()).padStart(2, '0');
      cols.push(`${y}-${m}-${d}`);
      cursor.setDate(cursor.getDate() + 1);
      safety++;
    }
    return cols;
  });

  readonly matrixRows = computed<MatrixEmployeeRow[]>(() => {
    const rowMap = new Map<string, MatrixEmployeeRow>();
    for (const emp of this.employees()) {
      rowMap.set(emp.id, {
        employeeId: emp.id,
        employeeCode: emp.employeeCode,
        employeeName: `${emp.firstName} ${emp.lastName}`.trim(),
        cells: {},
      });
    }

    for (const item of this.rosters()) {
      let row = rowMap.get(item.employeeId);
      if (!row) {
        row = {
          employeeId: item.employeeId,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          cells: {},
        };
        rowMap.set(item.employeeId, row);
      }
      row.cells[item.workDate] = item;
    }

    return Array.from(rowMap.values());
  });

  ngOnInit(): void {
    this.loadInitial();
  }

  loadInitial(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      shifts: this.attendanceAdminService.getShifts(),
      branches: this.organizationService.getBranches(),
      departments: this.organizationService.getDepartments(),
      employees: this.employeesService.getEmployees({ page: 1, pageSize: 100 }),
      rosters: this.attendanceAdminService.getRosters(
        this.startDate(),
        this.endDate(),
        this.branchFilter() || null,
        this.departmentFilter() || null,
      ),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.shifts.set(res.shifts);
          this.branches.set(res.branches);
          this.departments.set(res.departments);
          this.employees.set(res.employees.items);
          this.rosters.set(res.rosters);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.rosters.set([]);
          this.employees.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดตารางกะงานได้'));
          this.isLoading.set(false);
        },
      });
  }

  loadRosters(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      employees: this.employeesService.getEmployees({
        page: 1,
        pageSize: 100,
        branchId: this.branchFilter() || null,
        departmentId: this.departmentFilter() || null,
      }),
      rosters: this.attendanceAdminService.getRosters(
        this.startDate(),
        this.endDate(),
        this.branchFilter() || null,
        this.departmentFilter() || null,
      ),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.employees.set(res.employees.items);
          this.rosters.set(res.rosters);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.rosters.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดตารางกะงานได้'));
          this.isLoading.set(false);
        },
      });
  }

  toggleEmployeeSelection(empId: string): void {
    const current = this.selectedEmployeeIds();
    if (current.includes(empId)) {
      this.selectedEmployeeIds.set(current.filter((id) => id !== empId));
    } else {
      this.selectedEmployeeIds.set([...current, empId]);
    }
  }

  isEmployeeSelected(empId: string): boolean {
    return this.selectedEmployeeIds().includes(empId);
  }

  openBulkDrawer(): void {
    this.formError.set(null);
    this.bulkForm.reset({
      startDate: this.startDate(),
      endDate: this.endDate(),
      dayType: 'WORKDAY',
      shiftDefinitionId: this.shifts()[0]?.id ?? '',
    });
    this.isDrawerOpen.set(true);
  }

  submitBulkAssign(): void {
    const selected = this.selectedEmployeeIds();
    if (selected.length === 0) {
      this.formError.set('กรุณาเลือกพนักงานในตารางอย่างน้อย 1 คนก่อนจัดกะ');
      return;
    }
    if (this.bulkForm.invalid) {
      this.bulkForm.markAllAsTouched();
      return;
    }

    const raw = this.bulkForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);

    this.attendanceAdminService
      .bulkAssignRoster({
        employeeIds: selected,
        startDate: raw.startDate,
        endDate: raw.endDate,
        dayType: raw.dayType,
        shiftDefinitionId:
          raw.dayType === 'WORKDAY' && raw.shiftDefinitionId.length > 0 ? raw.shiftDefinitionId : null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.isSubmitting.set(false);
          this.isDrawerOpen.set(false);
          this.feedbackSuccess.set(`จัดตารางกะสำเร็จ ${res.assignedCount} รายการ`);
          this.loadRosters();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถจัดตารางกะได้'));
        },
      });
  }

  formatHeaderDate(isoDate: string): string {
    return formatDateOnlyThai(isoDate);
  }

  getDayTypeLabel(dayType: string): string {
    return mapDayTypeLabel(dayType);
  }

  getDayTypeTone(dayType: string) {
    return mapDayTypeTone(dayType);
  }

  private todayIso(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private addDaysIso(baseIso: string, days: number): string {
    const dt = new Date(`${baseIso}T00:00:00`);
    dt.setDate(dt.getDate() + days);
    const y = dt.getFullYear();
    const m = String(dt.getMonth() + 1).padStart(2, '0');
    const d = String(dt.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
