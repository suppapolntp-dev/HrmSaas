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
import { forkJoin } from 'rxjs';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableAction, DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import { JobGradeDto } from '../../../organization/models/organization.models';
import { WorkflowsService } from '../../data-access/workflows.service';
import { LeavePolicyItemDto } from '../../models/workflow.models';

@Component({
  selector: 'app-leave-policies-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './leave-policies-page.component.html',
  styleUrl: './leave-policies-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LeavePoliciesPageComponent implements OnInit {
  private readonly workflowsService = inject(WorkflowsService);
  private readonly organizationService = inject(OrganizationService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly policies = signal<LeavePolicyItemDto[]>([]);
  readonly jobGrades = signal<JobGradeDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly searchQuery = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isDrawerOpen = signal(false);
  readonly editingPolicy = signal<LeavePolicyItemDto | null>(null);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly policyForm = this.formBuilder.nonNullable.group({
    leaveTypeCode: ['', [Validators.required, Validators.maxLength(50)]],
    leaveTypeName: ['', [Validators.required, Validators.maxLength(150)]],
    isPaid: [true],
    allowHalfDay: [true],
    allowHourly: [false],
    requiresAttachment: [false],
    jobGradeId: [''],
    minTenureMonths: [0, [Validators.required, Validators.min(0)]],
    maxTenureMonths: [''],
    annualQuotaDays: [6, [Validators.required, Validators.min(0)]],
    prorateRule: ['NONE', [Validators.required]],
    maxCarryOverDays: [0, [Validators.required, Validators.min(0)]],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'leaveTypeCode', label: 'รหัส', isMono: true },
    { key: 'leaveTypeName', label: 'ประเภทวันลา' },
    { key: 'minTenureMonths', label: 'อายุงาน (เดือน)', align: 'right', isNumeric: true },
    { key: 'annualQuotaDays', label: 'โควตา/ปี (วัน)', align: 'right', isNumeric: true },
    { key: 'maxCarryOverDays', label: 'ยกยอดสูงสุด', align: 'right', isNumeric: true },
    { key: 'prorateRule', label: 'กติกา Prorate' },
    { key: 'isPaid', label: 'ได้รับค่าจ้าง' },
  ];

  readonly tableActions: readonly DataTableAction<LeavePolicyItemDto>[] = [
    { key: 'edit', label: 'แก้ไขนโยบาย', onClick: (row) => this.openDrawer(row) },
  ];

  readonly filteredPolicies = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.policies();
    }
    return this.policies().filter(
      (p) => p.leaveTypeCode.toLowerCase().includes(q) || p.leaveTypeName.toLowerCase().includes(q),
    );
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      policies: this.workflowsService.getLeavePolicies(),
      grades: this.organizationService.getJobGrades(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.policies.set(res.policies);
          this.jobGrades.set(res.grades);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.policies.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดนโยบายวันลาได้'));
          this.isLoading.set(false);
        },
      });
  }

  openDrawer(policy: LeavePolicyItemDto | null): void {
    this.editingPolicy.set(policy);
    this.formError.set(null);

    if (policy) {
      this.policyForm.controls.leaveTypeCode.disable();
      this.policyForm.controls.leaveTypeName.disable();
      this.policyForm.controls.jobGradeId.disable();
      this.policyForm.reset({
        leaveTypeCode: policy.leaveTypeCode,
        leaveTypeName: policy.leaveTypeName,
        isPaid: policy.isPaid,
        allowHalfDay: policy.allowHalfDay,
        allowHourly: policy.allowHourly,
        requiresAttachment: policy.requiresAttachment,
        jobGradeId: policy.jobGradeId ?? '',
        minTenureMonths: policy.minTenureMonths,
        maxTenureMonths: policy.maxTenureMonths !== null ? String(policy.maxTenureMonths) : '',
        annualQuotaDays: policy.annualQuotaDays,
        prorateRule: policy.prorateRule,
        maxCarryOverDays: policy.maxCarryOverDays,
      });
    } else {
      this.policyForm.controls.leaveTypeCode.enable();
      this.policyForm.controls.leaveTypeName.enable();
      this.policyForm.controls.jobGradeId.enable();
      this.policyForm.reset({
        leaveTypeCode: '',
        leaveTypeName: '',
        isPaid: true,
        allowHalfDay: true,
        allowHourly: false,
        requiresAttachment: false,
        jobGradeId: '',
        minTenureMonths: 0,
        maxTenureMonths: '',
        annualQuotaDays: 6,
        prorateRule: 'MONTHLY_PRORATE',
        maxCarryOverDays: 0,
      });
    }
    this.isDrawerOpen.set(true);
  }

  submitPolicy(): void {
    if (this.policyForm.invalid) {
      this.policyForm.markAllAsTouched();
      return;
    }

    const raw = this.policyForm.getRawValue();
    const editing = this.editingPolicy();
    const maxTenure =
      raw.maxTenureMonths.trim().length > 0 ? Number(raw.maxTenureMonths) : null;

    this.isSubmitting.set(true);
    this.formError.set(null);

    const req$ = editing
      ? this.workflowsService.updateLeavePolicy(editing.id, {
          minTenureMonths: Number(raw.minTenureMonths),
          maxTenureMonths: maxTenure,
          annualQuotaDays: Number(raw.annualQuotaDays),
          prorateRule: raw.prorateRule,
          maxCarryOverDays: Number(raw.maxCarryOverDays),
        })
      : this.workflowsService.createLeavePolicy({
          leaveTypeCode: raw.leaveTypeCode.trim().toUpperCase(),
          leaveTypeName: raw.leaveTypeName.trim(),
          isPaid: raw.isPaid,
          allowHalfDay: raw.allowHalfDay,
          allowHourly: raw.allowHourly,
          requiresAttachment: raw.requiresAttachment,
          jobGradeId: raw.jobGradeId.trim().length > 0 ? raw.jobGradeId : null,
          minTenureMonths: Number(raw.minTenureMonths),
          maxTenureMonths: maxTenure,
          annualQuotaDays: Number(raw.annualQuotaDays),
          prorateRule: raw.prorateRule,
          maxCarryOverDays: Number(raw.maxCarryOverDays),
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตนโยบายวันลาเรียบร้อยแล้ว' : 'สร้างนโยบายวันลาเรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
          const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.policyForm);
          if (unmapped.length > 0) {
            this.formError.set(unmapped[0]);
            return;
          }
        }
        this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถบันทึกนโยบายวันลาได้'));
      },
    });
  }

  formatTenureRange(row: LeavePolicyItemDto): string {
    return row.maxTenureMonths !== null
      ? `${row.minTenureMonths} - ${row.maxTenureMonths}`
      : `${row.minTenureMonths}+`;
  }
}
