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
import { ProblemDetails } from '../../../../shared/models/api-response.model';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { SlideDrawerComponent } from '../../../../shared/ui/slide-drawer/slide-drawer.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { SettingsService } from '../../data-access/settings.service';
import { PermissionDto, RoleDto } from '../../models/settings.models';

interface PermissionGroup {
  module: string;
  permissions: PermissionDto[];
}

@Component({
  selector: 'app-roles-permissions-page',
  standalone: true,
  imports: [ReactiveFormsModule, StateContainerComponent, SlideDrawerComponent, FormFieldComponent],
  templateUrl: './roles-permissions-page.component.html',
  styleUrl: './roles-permissions-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RolesPermissionsPageComponent implements OnInit {
  private readonly settingsService = inject(SettingsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly roles = signal<RoleDto[]>([]);
  readonly permissions = signal<PermissionDto[]>([]);
  readonly selectedRoleId = signal<string | null>(null);
  readonly draftRoleName = signal('');
  readonly draftPermissionCodes = signal<string[]>([]);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);
  readonly isSavingRole = signal(false);

  readonly isCreateDrawerOpen = signal(false);
  readonly isCreating = signal(false);
  readonly createError = signal<string | null>(null);

  readonly createRoleForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(120)]],
  });

  readonly selectedRole = computed(() => {
    const id = this.selectedRoleId();
    return this.roles().find((r) => r.id === id) ?? null;
  });

  readonly permissionGroups = computed<PermissionGroup[]>(() => {
    const byModule = new Map<string, PermissionDto[]>();
    for (const perm of this.permissions()) {
      const list = byModule.get(perm.module) ?? [];
      list.push(perm);
      byModule.set(perm.module, list);
    }

    const groups: PermissionGroup[] = [];
    for (const [moduleName, list] of byModule.entries()) {
      groups.push({ module: moduleName, permissions: list });
    }
    return groups;
  });

  ngOnInit(): void {
    this.loadRbac();
  }

  loadRbac(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      roles: this.settingsService.getRoles(),
      permissions: this.settingsService.getPermissions(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.roles.set(res.roles);
          this.permissions.set(res.permissions);
          if (res.roles.length > 0) {
            const existing = res.roles.find((r) => r.id === this.selectedRoleId()) ?? res.roles[0];
            this.selectRole(existing);
          } else {
            this.selectedRoleId.set(null);
            this.draftPermissionCodes.set([]);
          }
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.roles.set([]);
          this.permissions.set([]);
          this.errorMessage.set(
            extractProblemDetailsMessage(err, 'ไม่สามารถโหลดข้อมูลบทบาทและสิทธิ์ได้'),
          );
          this.isLoading.set(false);
        },
      });
  }

  selectRole(role: RoleDto): void {
    this.selectedRoleId.set(role.id);
    this.draftRoleName.set(role.name);
    this.draftPermissionCodes.set([...role.permissionCodes]);
  }

  hasDraftPermission(code: string): boolean {
    return this.draftPermissionCodes().includes(code);
  }

  toggleDraftPermission(code: string): void {
    const current = this.draftPermissionCodes();
    if (current.includes(code)) {
      this.draftPermissionCodes.set(current.filter((c) => c !== code));
    } else {
      this.draftPermissionCodes.set([...current, code]);
    }
  }

  saveSelectedRole(): void {
    const role = this.selectedRole();
    if (!role) {
      return;
    }

    this.isSavingRole.set(true);
    this.errorMessage.set(null);
    this.feedbackSuccess.set(null);

    this.settingsService
      .updateRole(role.id, {
        name: this.draftRoleName().trim() || role.name,
        permissionCodes: this.draftPermissionCodes(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.isSavingRole.set(false);
          this.roles.update((list) => list.map((r) => (r.id === updated.id ? updated : r)));
          this.selectRole(updated);
          this.feedbackSuccess.set(`บันทึกสิทธิ์ของบทบาท ${updated.name} เรียบร้อยแล้ว`);
        },
        error: (err: unknown) => {
          this.isSavingRole.set(false);
          this.errorMessage.set(extractProblemDetailsMessage(err, 'ไม่สามารถบันทึกสิทธิ์ของบทบาทได้'));
        },
      });
  }

  openCreateDrawer(): void {
    this.createError.set(null);
    this.createRoleForm.reset({ code: '', name: '' });
    this.isCreateDrawerOpen.set(true);
  }

  submitCreateRole(): void {
    if (this.createRoleForm.invalid) {
      this.createRoleForm.markAllAsTouched();
      return;
    }

    const raw = this.createRoleForm.getRawValue();
    this.isCreating.set(true);
    this.createError.set(null);

    this.settingsService
      .createRole({
        code: raw.code.trim().toUpperCase(),
        name: raw.name.trim(),
        permissionCodes: [],
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (created) => {
          this.isCreating.set(false);
          this.isCreateDrawerOpen.set(false);
          this.roles.update((list) => [...list, created]);
          this.selectRole(created);
          this.feedbackSuccess.set(`สร้างบทบาท ${created.name} เรียบร้อยแล้ว`);
        },
        error: (err: unknown) => {
          this.isCreating.set(false);
          if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
            const unmapped = applyProblemDetailsErrors(
              err.error as ProblemDetails,
              this.createRoleForm,
            );
            if (unmapped.length > 0) {
              this.createError.set(unmapped[0]);
              return;
            }
          }
          this.createError.set(extractProblemDetailsMessage(err, 'ไม่สามารถสร้างบทบาทใหม่ได้'));
        },
      });
  }
}
