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
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import {
  BranchDto,
  DepartmentDto,
  JobGradeDto,
  PositionDto,
} from '../../../organization/models/organization.models';
import { EmployeesService } from '../../data-access/employees.service';

@Component({
  selector: 'app-employee-onboarding-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, StateContainerComponent, FormFieldComponent],
  templateUrl: './employee-onboarding-page.component.html',
  styleUrl: './employee-onboarding-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeeOnboardingPageComponent implements OnInit {
  private readonly employeesService = inject(EmployeesService);
  private readonly organizationService = inject(OrganizationService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly branches = signal<BranchDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly positions = signal<PositionDto[]>([]);
  readonly jobGrades = signal<JobGradeDto[]>([]);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly onboardingForm = this.formBuilder.nonNullable.group({
    employeeCode: ['', [Validators.required, Validators.maxLength(50)]],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    branchId: ['', [Validators.required]],
    departmentId: ['', [Validators.required]],
    positionId: ['', [Validators.required]],
    jobGradeId: ['', [Validators.required]],
    hireDate: [this.todayIso(), [Validators.required]],
    probationEndDate: [''],
    employmentStatus: ['PROBATION', [Validators.required]],
    contractType: ['PROBATION', [Validators.required]],
    baseSalary: [30000, [Validators.required, Validators.min(1)]],
    paymentCycle: ['MONTHLY', [Validators.required]],
    email: [''],
    initialPassword: [''],
  });

  ngOnInit(): void {
    this.loadLookups();
  }

  loadLookups(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      branches: this.organizationService.getBranches(),
      departments: this.organizationService.getDepartments(),
      positions: this.organizationService.getPositions(),
      grades: this.organizationService.getJobGrades(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.branches.set(res.branches);
          this.departments.set(res.departments);
          this.positions.set(res.positions);
          this.jobGrades.set(res.grades);
          this.onboardingForm.patchValue({
            branchId: res.branches[0]?.id ?? '',
            departmentId: res.departments[0]?.id ?? '',
            positionId: res.positions[0]?.id ?? '',
            jobGradeId: res.grades[0]?.id ?? '',
          });
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.branches.set([]);
          this.departments.set([]);
          this.positions.set([]);
          this.jobGrades.set([]);
          this.errorMessage.set(
            extractProblemDetailsMessage(err, 'ไม่สามารถโหลดข้อมูลตั้งต้นสำหรับรับพนักงานใหม่ได้'),
          );
          this.isLoading.set(false);
        },
      });
  }

  submitOnboarding(): void {
    if (this.onboardingForm.invalid) {
      this.onboardingForm.markAllAsTouched();
      return;
    }

    const raw = this.onboardingForm.getRawValue();
    this.isSubmitting.set(true);
    this.submitError.set(null);

    this.employeesService
      .onboardEmployee({
        employeeCode: raw.employeeCode.trim().toUpperCase(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        branchId: raw.branchId,
        departmentId: raw.departmentId,
        positionId: raw.positionId,
        jobGradeId: raw.jobGradeId,
        hireDate: raw.hireDate,
        probationEndDate: raw.probationEndDate.trim().length > 0 ? raw.probationEndDate : null,
        employmentStatus: raw.employmentStatus,
        contractType: raw.contractType,
        baseSalary: Number(raw.baseSalary),
        paymentCycle: raw.paymentCycle,
        email: raw.email.trim().length > 0 ? raw.email.trim() : null,
        initialPassword: raw.initialPassword.trim().length > 0 ? raw.initialPassword.trim() : null,
        roleId: null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (created) => {
          this.isSubmitting.set(false);
          void this.router.navigate(['/employees', created.id]);
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
            const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.onboardingForm);
            if (unmapped.length > 0) {
              this.submitError.set(unmapped[0]);
              return;
            }
          }
          this.submitError.set(extractProblemDetailsMessage(err, 'ไม่สามารถรับพนักงานใหม่ได้'));
        },
      });
  }

  private todayIso(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
