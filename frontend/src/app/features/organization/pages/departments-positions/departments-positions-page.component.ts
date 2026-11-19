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
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent } from '../../../../shared/ui/data-table/data-table.component';
import { DataTableAction, DataTableColumn } from '../../../../shared/ui/data-table/data-table.models';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { OrganizationService } from '../../data-access/organization.service';
import {
  DepartmentDto,
  DepartmentTreeNodeDto,
  JobGradeDto,
  PositionDto,
} from '../../models/organization.models';

interface FlatDepartmentRow {
  id: string;
  code: string;
  name: string;
  depth: number;
  parentDepartmentId: string | null;
  headEmployeeId: string | null;
}

@Component({
  selector: 'app-departments-positions-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    SlideDrawerComponent,
    ConfirmDialogComponent,
    FormFieldComponent,
  ],
  templateUrl: './departments-positions-page.component.html',
  styleUrl: './departments-positions-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentsPositionsPageComponent implements OnInit {
  private readonly organizationService = inject(OrganizationService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly activeTab = signal<'DEPARTMENTS' | 'POSITIONS' | 'GRADES'>('DEPARTMENTS');
  readonly departments = signal<DepartmentDto[]>([]);
  readonly departmentTree = signal<DepartmentTreeNodeDto[]>([]);
  readonly positions = signal<PositionDto[]>([]);
  readonly jobGrades = signal<JobGradeDto[]>([]);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);
  readonly searchQuery = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isDeptDrawerOpen = signal(false);
  readonly editingDept = signal<FlatDepartmentRow | null>(null);
  readonly deleteDeptTarget = signal<FlatDepartmentRow | null>(null);
  readonly isDeletingDept = signal(false);

  readonly isPosDrawerOpen = signal(false);
  readonly editingPos = signal<PositionDto | null>(null);

  readonly isGradeDrawerOpen = signal(false);
  readonly editingGrade = signal<JobGradeDto | null>(null);

  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly deptForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    parentDepartmentId: [''],
  });

  readonly posForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    title: ['', [Validators.required, Validators.maxLength(150)]],
  });

  readonly gradeForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(30)]],
    levelOrder: [1, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
  });

  readonly deptColumns: readonly DataTableColumn[] = [
    { key: 'code', label: 'รหัสแผนก', isMono: true },
    { key: 'name', label: 'ชื่อแผนก (โครงสร้างลำดับชั้น)' },
  ];

  readonly posColumns: readonly DataTableColumn[] = [
    { key: 'code', label: 'รหัสตำแหน่ง', isMono: true },
    { key: 'title', label: 'ชื่อตำแหน่งงาน' },
  ];

  readonly gradeColumns: readonly DataTableColumn[] = [
    { key: 'levelOrder', label: 'ลำดับระดับ', align: 'right', isNumeric: true },
    { key: 'code', label: 'รหัสระดับ', isMono: true },
    { key: 'name', label: 'ชื่อระดับพนักงาน' },
  ];

  readonly deptActions: readonly DataTableAction<FlatDepartmentRow>[] = [
    { key: 'edit', label: 'แก้ไขแผนก', onClick: (row) => this.openDeptDrawer(row) },
    { key: 'delete', label: 'ลบแผนก', variant: 'danger', onClick: (row) => this.deleteDeptTarget.set(row) },
  ];

  readonly posActions: readonly DataTableAction<PositionDto>[] = [
    { key: 'edit', label: 'แก้ไขตำแหน่ง', onClick: (row) => this.openPosDrawer(row) },
  ];

  readonly gradeActions: readonly DataTableAction<JobGradeDto>[] = [
    { key: 'edit', label: 'แก้ไขระดับพนักงาน', onClick: (row) => this.openGradeDrawer(row) },
  ];

  readonly flattenedDepartments = computed(() => {
    const flatById = new Map<string, DepartmentDto>();
    for (const d of this.departments()) {
      flatById.set(d.id, d);
    }

    const rows: FlatDepartmentRow[] = [];
    const visit = (nodes: DepartmentTreeNodeDto[], depth: number): void => {
      for (const node of nodes) {
        const raw = flatById.get(node.id);
        rows.push({
          id: node.id,
          code: node.code,
          name: node.name,
          depth,
          parentDepartmentId: raw ? raw.parentDepartmentId : null,
          headEmployeeId: node.headEmployeeId,
        });
        if (node.children.length > 0) {
          visit(node.children, depth + 1);
        }
      }
    };

    visit(this.departmentTree(), 0);
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return rows;
    }
    return rows.filter((r) => r.code.toLowerCase().includes(q) || r.name.toLowerCase().includes(q));
  });

  readonly filteredPositions = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.positions();
    }
    return this.positions().filter(
      (p) => p.code.toLowerCase().includes(q) || p.title.toLowerCase().includes(q),
    );
  });

  readonly filteredGrades = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.jobGrades();
    }
    return this.jobGrades().filter(
      (g) => g.code.toLowerCase().includes(q) || g.name.toLowerCase().includes(q),
    );
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      departments: this.organizationService.getDepartments(),
      tree: this.organizationService.getDepartmentTree(),
      positions: this.organizationService.getPositions(),
      grades: this.organizationService.getJobGrades(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.departments.set(res.departments);
          this.departmentTree.set(res.tree);
          this.positions.set(res.positions);
          this.jobGrades.set(res.grades);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.departments.set([]);
          this.departmentTree.set([]);
          this.positions.set([]);
          this.jobGrades.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดโครงสร้างองค์กรได้'));
          this.isLoading.set(false);
        },
      });
  }

  openCreateForActiveTab(): void {
    const tab = this.activeTab();
    if (tab === 'DEPARTMENTS') {
      this.openDeptDrawer(null);
    } else if (tab === 'POSITIONS') {
      this.openPosDrawer(null);
    } else {
      this.openGradeDrawer(null);
    }
  }

  openDeptDrawer(row: FlatDepartmentRow | null): void {
    this.editingDept.set(row);
    this.formError.set(null);
    if (row) {
      this.deptForm.controls.code.disable();
      this.deptForm.reset({
        code: row.code,
        name: row.name,
        parentDepartmentId: row.parentDepartmentId ?? '',
      });
    } else {
      this.deptForm.controls.code.enable();
      this.deptForm.reset({ code: '', name: '', parentDepartmentId: '' });
    }
    this.isDeptDrawerOpen.set(true);
  }

  submitDept(): void {
    if (this.deptForm.invalid) {
      this.deptForm.markAllAsTouched();
      return;
    }

    const raw = this.deptForm.getRawValue();
    const editing = this.editingDept();
    const parentId = raw.parentDepartmentId.trim().length > 0 ? raw.parentDepartmentId.trim() : null;
    this.isSubmitting.set(true);
    this.formError.set(null);

    const req$ = editing
      ? this.organizationService.updateDepartment(editing.id, {
          name: raw.name.trim(),
          parentDepartmentId: parentId,
          headEmployeeId: editing.headEmployeeId,
        })
      : this.organizationService.createDepartment({
          code: raw.code.trim().toUpperCase(),
          name: raw.name.trim(),
          parentDepartmentId: parentId,
          headEmployeeId: null,
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isDeptDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตแผนกเรียบร้อยแล้ว' : 'เพิ่มแผนกเรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        this.handleError(err, this.deptForm, 'ไม่สามารถบันทึกข้อมูลแผนกได้');
      },
    });
  }

  confirmDeleteDept(): void {
    const target = this.deleteDeptTarget();
    if (!target) {
      return;
    }
    this.isDeletingDept.set(true);
    this.organizationService
      .deleteDepartment(target.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isDeletingDept.set(false);
          this.deleteDeptTarget.set(null);
          this.feedbackSuccess.set(`ลบแผนก ${target.name} เรียบร้อยแล้ว`);
          this.loadData();
        },
        error: (err: unknown) => {
          this.isDeletingDept.set(false);
          this.deleteDeptTarget.set(null);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถลบแผนกได้'));
        },
      });
  }

  openPosDrawer(pos: PositionDto | null): void {
    this.editingPos.set(pos);
    this.formError.set(null);
    if (pos) {
      this.posForm.controls.code.disable();
      this.posForm.reset({ code: pos.code, title: pos.title });
    } else {
      this.posForm.controls.code.enable();
      this.posForm.reset({ code: '', title: '' });
    }
    this.isPosDrawerOpen.set(true);
  }

  submitPos(): void {
    if (this.posForm.invalid) {
      this.posForm.markAllAsTouched();
      return;
    }
    const raw = this.posForm.getRawValue();
    const editing = this.editingPos();
    this.isSubmitting.set(true);
    this.formError.set(null);

    const req$ = editing
      ? this.organizationService.updatePosition(editing.id, { title: raw.title.trim() })
      : this.organizationService.createPosition({
          code: raw.code.trim().toUpperCase(),
          title: raw.title.trim(),
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isPosDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตตำแหน่งเรียบร้อยแล้ว' : 'เพิ่มตำแหน่งเรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        this.handleError(err, this.posForm, 'ไม่สามารถบันทึกตำแหน่งได้');
      },
    });
  }

  openGradeDrawer(grade: JobGradeDto | null): void {
    this.editingGrade.set(grade);
    this.formError.set(null);
    if (grade) {
      this.gradeForm.controls.code.disable();
      this.gradeForm.reset({ code: grade.code, levelOrder: grade.levelOrder, name: grade.name });
    } else {
      this.gradeForm.controls.code.enable();
      this.gradeForm.reset({ code: '', levelOrder: 1, name: '' });
    }
    this.isGradeDrawerOpen.set(true);
  }

  submitGrade(): void {
    if (this.gradeForm.invalid) {
      this.gradeForm.markAllAsTouched();
      return;
    }
    const raw = this.gradeForm.getRawValue();
    const editing = this.editingGrade();
    this.isSubmitting.set(true);
    this.formError.set(null);

    const req$ = editing
      ? this.organizationService.updateJobGrade(editing.id, {
          levelOrder: Number(raw.levelOrder),
          name: raw.name.trim(),
        })
      : this.organizationService.createJobGrade({
          code: raw.code.trim().toUpperCase(),
          levelOrder: Number(raw.levelOrder),
          name: raw.name.trim(),
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isGradeDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตระดับพนักงานเรียบร้อยแล้ว' : 'เพิ่มระดับพนักงานเรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        this.handleError(err, this.gradeForm, 'ไม่สามารถบันทึกระดับพนักงานได้');
      },
    });
  }

  formatDeptIndent(depth: number): string {
    return depth > 0 ? `${'— '.repeat(depth)}` : '';
  }

  private handleError(
    err: unknown,
    form: typeof this.deptForm | typeof this.posForm | typeof this.gradeForm,
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
