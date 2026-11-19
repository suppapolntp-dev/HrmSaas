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
import { forkJoin, of } from 'rxjs';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { APP_CONFIG } from '../../../../core/config/app-config.token';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableAction, DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { EmployeesService } from '../../../employees/data-access/employees.service';
import { EmployeeListItemDto } from '../../../employees/models/employee.models';
import { OrganizationService } from '../../../organization/data-access/organization.service';
import { DepartmentDto } from '../../../organization/models/organization.models';
import { mapRequestTypeLabel } from '../../../portal/models/portal-dashboard.models';
import { SettingsService } from '../../../settings/data-access/settings.service';
import { RoleDto } from '../../../settings/models/settings.models';
import { WorkflowsService } from '../../data-access/workflows.service';
import {
  WorkflowDefinitionDto,
  WorkflowStepInput,
  mapApproverStrategyLabel,
} from '../../models/workflow.models';

@Component({
  selector: 'app-approval-routes-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './approval-routes-page.component.html',
  styleUrl: './approval-routes-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApprovalRoutesPageComponent implements OnInit {
  private readonly workflowsService = inject(WorkflowsService);
  private readonly organizationService = inject(OrganizationService);
  private readonly settingsService = inject(SettingsService);
  private readonly employeesService = inject(EmployeesService);
  private readonly authSessionStore = inject(AuthSessionStore);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly routes = signal<WorkflowDefinitionDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly roles = signal<RoleDto[]>([]);
  readonly employees = signal<EmployeeListItemDto[]>([]);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly searchQuery = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isDrawerOpen = signal(false);
  readonly editingRoute = signal<WorkflowDefinitionDto | null>(null);
  readonly draftSteps = signal<WorkflowStepInput[]>([]);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly routeForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    requestType: ['LEAVE', [Validators.required]],
    departmentId: [''],
    isActive: [true],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'name', label: 'ชื่อสายอนุมัติ' },
    { key: 'requestType', label: 'ประเภทคำขอ' },
    { key: 'departmentName', label: 'แผนกที่บังคับใช้' },
    { key: 'steps', label: 'ลำดับผู้อนุมัติ' },
    { key: 'isActive', label: 'สถานะ' },
  ];

  readonly tableActions: readonly DataTableAction<WorkflowDefinitionDto>[] = [
    { key: 'edit', label: 'แก้ไขสายอนุมัติ', onClick: (row) => this.openDrawer(row) },
  ];

  readonly filteredRoutes = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.routes();
    }
    return this.routes().filter(
      (r) => r.name.toLowerCase().includes(q) || (r.departmentName ?? '').toLowerCase().includes(q),
    );
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const roles$ = this.authSessionStore.hasPermission('settings.rbac.manage')
      ? this.settingsService.getRoles()
      : of<RoleDto[]>([]);
    const employees$ = this.authSessionStore.hasPermission('employee.read')
      ? this.employeesService.getEmployees({ page: 1, pageSize: 100 })
      : of({ items: [], totalCount: 0, page: 1, pageSize: 100, totalPages: 0 });

    forkJoin({
      routes: this.workflowsService.getApprovalRoutes(),
      departments: this.organizationService.getDepartments(),
      roles: roles$,
      employees: employees$,
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.routes.set(res.routes);
          this.departments.set(res.departments);
          this.roles.set(res.roles);
          this.employees.set(res.employees.items);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.routes.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดสายอนุมัติได้'));
          this.isLoading.set(false);
        },
      });
  }

  openDrawer(route: WorkflowDefinitionDto | null): void {
    this.editingRoute.set(route);
    this.formError.set(null);

    if (route) {
      this.routeForm.controls.requestType.disable();
      this.routeForm.reset({
        name: route.name,
        requestType: route.requestType,
        departmentId: route.departmentId ?? '',
        isActive: route.isActive,
      });
      this.draftSteps.set(
        route.steps.map((s) => ({
          stepOrder: s.stepOrder,
          approverStrategy: s.approverStrategy,
          specificRoleId: s.specificRoleId,
          specificEmployeeId: s.specificEmployeeId,
        })),
      );
    } else {
      this.routeForm.controls.requestType.enable();
      this.routeForm.reset({
        name: '',
        requestType: 'LEAVE',
        departmentId: '',
        isActive: true,
      });
      this.draftSteps.set([
        {
          stepOrder: 1,
          approverStrategy: 'DIRECT_MANAGER',
          specificRoleId: null,
          specificEmployeeId: null,
        },
      ]);
    }
    this.isDrawerOpen.set(true);
  }

  addStep(): void {
    const current = this.draftSteps();
    this.draftSteps.set([
      ...current,
      {
        stepOrder: current.length + 1,
        approverStrategy: 'HR_ROLE',
        specificRoleId: null,
        specificEmployeeId: null,
      },
    ]);
  }

  removeStep(index: number): void {
    const remaining = this.draftSteps().filter((_, idx) => idx !== index);
    this.draftSteps.set(
      remaining.map((step, idx) => ({
        ...step,
        stepOrder: idx + 1,
      })),
    );
  }

  updateStepStrategy(index: number, event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.draftSteps.update((steps) =>
      steps.map((s, idx) =>
        idx === index
          ? { ...s, approverStrategy: value, specificRoleId: null, specificEmployeeId: null }
          : s,
      ),
    );
  }

  updateStepRole(index: number, event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.draftSteps.update((steps) =>
      steps.map((s, idx) => (idx === index ? { ...s, specificRoleId: value || null } : s)),
    );
  }

  updateStepEmployee(index: number, event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.draftSteps.update((steps) =>
      steps.map((s, idx) => (idx === index ? { ...s, specificEmployeeId: value || null } : s)),
    );
  }

  submitRoute(): void {
    if (this.routeForm.invalid) {
      this.routeForm.markAllAsTouched();
      return;
    }
    if (this.draftSteps().length === 0) {
      this.formError.set('ต้องกำหนดขั้นตอนการอนุมัติอย่างน้อย 1 ขั้นตอน');
      return;
    }

    const raw = this.routeForm.getRawValue();
    const editing = this.editingRoute();
    const deptId = raw.departmentId.trim().length > 0 ? raw.departmentId : null;
    this.isSubmitting.set(true);
    this.formError.set(null);

    const req$ = editing
      ? this.workflowsService.updateApprovalRoute(editing.id, {
          departmentId: deptId,
          name: raw.name.trim(),
          isActive: raw.isActive,
          steps: this.draftSteps(),
        })
      : this.workflowsService.createApprovalRoute({
          requestType: raw.requestType,
          departmentId: deptId,
          name: raw.name.trim(),
          isActive: raw.isActive,
          steps: this.draftSteps(),
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตสายอนุมัติเรียบร้อยแล้ว' : 'สร้างสายอนุมัติเรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถบันทึกสายอนุมัติได้'));
      },
    });
  }

  formatRequestType(type: string): string {
    return mapRequestTypeLabel(type);
  }

  formatStepsSummary(route: WorkflowDefinitionDto): string {
    return route.steps
      .map((s) => `${s.stepOrder}. ${mapApproverStrategyLabel(s.approverStrategy)}`)
      .join(' → ');
  }
}
