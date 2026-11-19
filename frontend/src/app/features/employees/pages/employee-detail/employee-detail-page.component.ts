import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HasPermissionDirective } from '../../../../core/directives/has-permission.directive';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import {
  BranchDto,
  DepartmentDto,
  JobGradeDto,
  PositionDto,
} from '../../../organization/models/organization.models';
import {
  formatCurrencyThb,
  formatDateOnlyThai,
} from '../../../portal/models/portal-dashboard.models';
import { EmployeesService } from '../../data-access/employees.service';
import {
  Employee360Dto,
  EmployeeListItemDto,
  mapEmploymentStatusLabel,
  mapEmploymentStatusTone,
} from '../../models/employee.models';

@Component({
  selector: 'app-employee-detail-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    HasPermissionDirective,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './employee-detail-page.component.html',
  styleUrl: './employee-detail-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeeDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly employeesService = inject(EmployeesService);
  private readonly organizationService = inject(OrganizationService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly employee = signal<Employee360Dto | null>(null);
  readonly branches = signal<BranchDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly positions = signal<PositionDto[]>([]);
  readonly jobGrades = signal<JobGradeDto[]>([]);
  readonly allEmployees = signal<EmployeeListItemDto[]>([]);

  readonly activeTab = signal<'PROFILE' | 'CONTRACTS' | 'REPORTING' | 'LEAVES'>('PROFILE');
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly isEditDrawerOpen = signal(false);
  readonly isContractDrawerOpen = signal(false);
  readonly isReportingDrawerOpen = signal(false);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly editForm = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    branchId: ['', [Validators.required]],
    departmentId: ['', [Validators.required]],
    positionId: ['', [Validators.required]],
    jobGradeId: ['', [Validators.required]],
    employmentStatus: ['ACTIVE', [Validators.required]],
  });

  readonly contractForm = this.formBuilder.nonNullable.group({
    contractType: ['PERMANENT', [Validators.required]],
    effectiveFrom: [this.todayIso(), [Validators.required]],
    effectiveTo: [''],
    baseSalary: [30000, [Validators.required, Validators.min(1)]],
    paymentCycle: ['MONTHLY', [Validators.required]],
  });

  readonly reportingForm = this.formBuilder.nonNullable.group({
    managerEmployeeId: ['', [Validators.required]],
    lineType: ['DIRECT', [Validators.required]],
    priorityOrder: [1, [Validators.required, Validators.min(1)]],
    effectiveFrom: [this.todayIso(), [Validators.required]],
  });

  readonly contractColumns: readonly DataTableColumn[] = [
    { key: 'contractType', label: 'ประเภทสัญญา', isMono: true },
    { key: 'effectiveFrom', label: 'วันที่มีผล', isMono: true },
    { key: 'baseSalary', label: 'ฐานเงินเดือน', align: 'right', isNumeric: true },
    { key: 'paymentCycle', label: 'รอบจ่าย' },
    { key: 'isActive', label: 'สถานะ' },
  ];

  readonly reportingColumns: readonly DataTableColumn[] = [
    { key: 'managerEmployeeCode', label: 'รหัสหัวหน้า', isMono: true },
    { key: 'managerFullName', label: 'ชื่อหัวหน้างาน' },
    { key: 'lineType', label: 'ประเภทสายบังคับบัญชา' },
    { key: 'priorityOrder', label: 'ลำดับ', align: 'right', isNumeric: true },
    { key: 'effectiveFrom', label: 'มีผลตั้งแต่', isMono: true },
  ];

  readonly leaveColumns: readonly DataTableColumn[] = [
    { key: 'leaveTypeCode', label: 'รหัส', isMono: true },
    { key: 'leaveTypeName', label: 'ประเภทวันลา' },
    { key: 'entitledDays', label: 'สิทธิ์ทั้งปี', align: 'right', isNumeric: true },
    { key: 'usedDays', label: 'ใช้ไป', align: 'right', isNumeric: true },
    { key: 'pendingDays', label: 'รออนุมัติ', align: 'right', isNumeric: true },
    { key: 'remainingDays', label: 'คงเหลือ', align: 'right', isNumeric: true },
  ];

  ngOnInit(): void {
    this.loadDetail();
  }

  loadDetail(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('ไม่พบรหัสพนักงานใน URL');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      employee: this.employeesService.getEmployeeById(id),
      branches: this.organizationService.getBranches(),
      departments: this.organizationService.getDepartments(),
      positions: this.organizationService.getPositions(),
      grades: this.organizationService.getJobGrades(),
      allEmployees: this.employeesService.getEmployees({ page: 1, pageSize: 100 }),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.employee.set(res.employee);
          this.branches.set(res.branches);
          this.departments.set(res.departments);
          this.positions.set(res.positions);
          this.jobGrades.set(res.grades);
          this.allEmployees.set(res.allEmployees.items);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.employee.set(null);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดข้อมูลพนักงานได้'));
          this.isLoading.set(false);
        },
      });
  }

  openEditDrawer(): void {
    const emp = this.employee();
    if (!emp) {
      return;
    }
    this.formError.set(null);
    this.editForm.reset({
      firstName: emp.firstName,
      lastName: emp.lastName,
      branchId: emp.branchId,
      departmentId: emp.departmentId,
      positionId: emp.positionId,
      jobGradeId: emp.jobGradeId,
      employmentStatus: emp.employmentStatus,
    });
    this.isEditDrawerOpen.set(true);
  }

  submitEdit(): void {
    const emp = this.employee();
    if (!emp || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const raw = this.editForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);

    this.employeesService
      .updateEmployee(emp.id, {
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        branchId: raw.branchId,
        departmentId: raw.departmentId,
        positionId: raw.positionId,
        jobGradeId: raw.jobGradeId,
        employmentStatus: raw.employmentStatus,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.employee.set(updated);
          this.isSubmitting.set(false);
          this.isEditDrawerOpen.set(false);
          this.feedbackSuccess.set('อัปเดตข้อมูลพนักงานเรียบร้อยแล้ว');
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          this.handleError(err, this.editForm, 'ไม่สามารถอัปเดตข้อมูลพนักงานได้');
        },
      });
  }

  openContractDrawer(): void {
    this.formError.set(null);
    this.contractForm.reset({
      contractType: 'PERMANENT',
      effectiveFrom: this.todayIso(),
      effectiveTo: '',
      baseSalary: 30000,
      paymentCycle: 'MONTHLY',
    });
    this.isContractDrawerOpen.set(true);
  }

  submitContract(): void {
    const emp = this.employee();
    if (!emp || this.contractForm.invalid) {
      this.contractForm.markAllAsTouched();
      return;
    }
    const raw = this.contractForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);

    this.employeesService
      .createContract(emp.id, {
        contractType: raw.contractType,
        effectiveFrom: raw.effectiveFrom,
        effectiveTo: raw.effectiveTo.trim().length > 0 ? raw.effectiveTo : null,
        baseSalary: Number(raw.baseSalary),
        paymentCycle: raw.paymentCycle,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isContractDrawerOpen.set(false);
          this.feedbackSuccess.set('บันทึกสัญญาจ้างใหม่เรียบร้อยแล้ว');
          this.loadDetail();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          this.handleError(err, this.contractForm, 'ไม่สามารถบันทึกสัญญาจ้างได้');
        },
      });
  }

  openReportingDrawer(): void {
    const emp = this.employee();
    this.formError.set(null);
    const existing = emp?.reportingLines[0];
    this.reportingForm.reset({
      managerEmployeeId: existing ? existing.managerEmployeeId : '',
      lineType: existing ? existing.lineType : 'DIRECT',
      priorityOrder: existing ? existing.priorityOrder : 1,
      effectiveFrom: existing ? existing.effectiveFrom : this.todayIso(),
    });
    this.isReportingDrawerOpen.set(true);
  }

  submitReporting(): void {
    const emp = this.employee();
    if (!emp || this.reportingForm.invalid) {
      this.reportingForm.markAllAsTouched();
      return;
    }
    const raw = this.reportingForm.getRawValue();
    this.isSubmitting.set(true);
    this.formError.set(null);

    this.employeesService
      .updateReportingLines(emp.id, {
        lines: [
          {
            managerEmployeeId: raw.managerEmployeeId,
            lineType: raw.lineType,
            priorityOrder: Number(raw.priorityOrder),
            effectiveFrom: raw.effectiveFrom,
            effectiveTo: null,
          },
        ],
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isReportingDrawerOpen.set(false);
          this.feedbackSuccess.set('อัปเดตสายบังคับบัญชาเรียบร้อยแล้ว');
          this.loadDetail();
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          this.handleError(err, this.reportingForm, 'ไม่สามารถอัปเดตสายบังคับบัญชาได้');
        },
      });
  }

  formatDate(dateText: string | null): string {
    return formatDateOnlyThai(dateText);
  }

  formatMoney(amount: number): string {
    return formatCurrencyThb(amount);
  }

  getStatusLabel(status: string): string {
    return mapEmploymentStatusLabel(status);
  }

  getStatusTone(status: string) {
    return mapEmploymentStatusTone(status);
  }

  private todayIso(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private handleError(
    err: unknown,
    form: typeof this.editForm | typeof this.contractForm | typeof this.reportingForm,
    fallback: string,
  ): void {
    if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
      const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, form);
      if (unmapped.length > 0) {
        this.formError.set(unmapped[0]);
        return;
      }
    }
    this.formError.set(extractProblemDetailsMessage(err, fallback));
  }
}
