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
import { formatTimeOnly } from '../../../portal/models/portal-dashboard.models';
import { AttendanceAdminService } from '../../data-access/attendance-admin.service';
import { ShiftDefinitionDto } from '../../models/attendance.models';

@Component({
  selector: 'app-shift-patterns-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    StatusIndicatorComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './shift-patterns-page.component.html',
  styleUrl: './shift-patterns-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShiftPatternsPageComponent implements OnInit {
  private readonly attendanceAdminService = inject(AttendanceAdminService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly shifts = signal<ShiftDefinitionDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly searchQuery = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isDrawerOpen = signal(false);
  readonly editingShift = signal<ShiftDefinitionDto | null>(null);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly shiftForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(150)]],
    shiftType: ['NORMAL', [Validators.required]],
    startTime: ['09:00', [Validators.required]],
    endTime: ['18:00', [Validators.required]],
    isOvernight: [false],
    gracePeriodMinutes: [10, [Validators.required, Validators.min(0)]],
    lateAfterMinutes: [15, [Validators.required, Validators.min(0)]],
    standardWorkMinutes: [480, [Validators.required, Validators.min(60)]],
    breakStart: ['12:00'],
    breakEnd: ['13:00'],
    breakDurationMinutes: [60],
  });

  readonly columns: readonly DataTableColumn[] = [
    { key: 'code', label: 'รหัสกะ', isMono: true },
    { key: 'name', label: 'ชื่อกะงาน' },
    { key: 'shiftType', label: 'ประเภทกะ' },
    { key: 'startTime', label: 'เวลาเข้า-ออก', isMono: true },
    { key: 'gracePeriodMinutes', label: 'ผ่อนผันสาย (น.)', align: 'right', isNumeric: true },
    { key: 'standardWorkMinutes', label: 'เวลามาตรฐาน (น.)', align: 'right', isNumeric: true },
    { key: 'isOvernight', label: 'กะข้ามคืน' },
  ];

  readonly tableActions: readonly DataTableAction<ShiftDefinitionDto>[] = [
    { key: 'edit', label: 'แก้ไขกะงาน', onClick: (row) => this.openDrawer(row) },
  ];

  readonly filteredShifts = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (q.length === 0) {
      return this.shifts();
    }
    return this.shifts().filter(
      (s) => s.code.toLowerCase().includes(q) || s.name.toLowerCase().includes(q),
    );
  });

  ngOnInit(): void {
    this.loadShifts();
  }

  loadShifts(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.attendanceAdminService
      .getShifts()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.shifts.set(data);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.shifts.set([]);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถโหลดรูปแบบกะงานได้'));
          this.isLoading.set(false);
        },
      });
  }

  openDrawer(shift: ShiftDefinitionDto | null): void {
    this.editingShift.set(shift);
    this.formError.set(null);
    if (shift) {
      this.shiftForm.controls.code.disable();
      const firstBreak = shift.breakRules[0];
      this.shiftForm.reset({
        code: shift.code,
        name: shift.name,
        shiftType: shift.shiftType,
        startTime: formatTimeOnly(shift.startTime),
        endTime: formatTimeOnly(shift.endTime),
        isOvernight: shift.isOvernight,
        gracePeriodMinutes: shift.gracePeriodMinutes,
        lateAfterMinutes: shift.lateAfterMinutes,
        standardWorkMinutes: shift.standardWorkMinutes,
        breakStart: firstBreak ? formatTimeOnly(firstBreak.breakStart) : '',
        breakEnd: firstBreak ? formatTimeOnly(firstBreak.breakEnd) : '',
        breakDurationMinutes: firstBreak ? firstBreak.durationMinutes : 0,
      });
    } else {
      this.shiftForm.controls.code.enable();
      this.shiftForm.reset({
        code: '',
        name: '',
        shiftType: 'NORMAL',
        startTime: '09:00',
        endTime: '18:00',
        isOvernight: false,
        gracePeriodMinutes: 10,
        lateAfterMinutes: 15,
        standardWorkMinutes: 480,
        breakStart: '12:00',
        breakEnd: '13:00',
        breakDurationMinutes: 60,
      });
    }
    this.isDrawerOpen.set(true);
  }

  submitShift(): void {
    if (this.shiftForm.invalid) {
      this.shiftForm.markAllAsTouched();
      return;
    }

    const raw = this.shiftForm.getRawValue();
    const editing = this.editingShift();
    const breakRules =
      raw.breakStart && raw.breakEnd && Number(raw.breakDurationMinutes) > 0
        ? [
            {
              breakStart: this.toTimeOnly(raw.breakStart),
              breakEnd: this.toTimeOnly(raw.breakEnd),
              durationMinutes: Number(raw.breakDurationMinutes),
              isPaid: false,
            },
          ]
        : [];

    this.isSubmitting.set(true);
    this.formError.set(null);

    const req$ = editing
      ? this.attendanceAdminService.updateShift(editing.id, {
          name: raw.name.trim(),
          shiftType: raw.shiftType,
          startTime: this.toTimeOnly(raw.startTime),
          endTime: this.toTimeOnly(raw.endTime),
          isOvernight: raw.isOvernight,
          gracePeriodMinutes: Number(raw.gracePeriodMinutes),
          lateAfterMinutes: Number(raw.lateAfterMinutes),
          standardWorkMinutes: Number(raw.standardWorkMinutes),
          breakRules,
        })
      : this.attendanceAdminService.createShift({
          code: raw.code.trim().toUpperCase(),
          name: raw.name.trim(),
          shiftType: raw.shiftType,
          startTime: this.toTimeOnly(raw.startTime),
          endTime: this.toTimeOnly(raw.endTime),
          isOvernight: raw.isOvernight,
          gracePeriodMinutes: Number(raw.gracePeriodMinutes),
          lateAfterMinutes: Number(raw.lateAfterMinutes),
          standardWorkMinutes: Number(raw.standardWorkMinutes),
          breakRules,
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตกะงานเรียบร้อยแล้ว' : 'สร้างกะงานใหม่เรียบร้อยแล้ว');
        this.loadShifts();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
          const unmapped = applyProblemDetailsErrors(err.error as ProblemDetails, this.shiftForm);
          if (unmapped.length > 0) {
            this.formError.set(unmapped[0]);
            return;
          }
        }
        this.formError.set(extractProblemDetailsMessage(err, 'ไม่สามารถบันทึกกะงานได้'));
      },
    });
  }

  formatWindow(start: string, end: string): string {
    return `${formatTimeOnly(start)} - ${formatTimeOnly(end)}`;
  }

  private toTimeOnly(val: string): string {
    return val.length === 5 ? `${val}:00` : val;
  }
}
