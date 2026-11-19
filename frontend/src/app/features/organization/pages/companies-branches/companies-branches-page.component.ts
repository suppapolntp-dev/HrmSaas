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
import { applyProblemDetailsErrors } from '../../../../shared/utils/form-error.helper';
import { extractProblemDetailsMessage } from '../../../../shared/utils/http-error.helper';
import { OrganizationService } from '../../data-access/organization.service';
import { BranchDto, CompanyDto } from '../../models/organization.models';

@Component({
  selector: 'app-companies-branches-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    StateContainerComponent,
    SlideDrawerComponent,
    FormFieldComponent,
  ],
  templateUrl: './companies-branches-page.component.html',
  styleUrl: './companies-branches-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompaniesBranchesPageComponent implements OnInit {
  private readonly organizationService = inject(OrganizationService);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly activeTab = signal<'COMPANIES' | 'BRANCHES'>('COMPANIES');
  readonly companies = signal<CompanyDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackSuccess = signal<string | null>(null);

  readonly companySearch = signal('');
  readonly branchSearch = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(this.appConfig.defaultPageSize);

  readonly isCompanyDrawerOpen = signal(false);
  readonly editingCompany = signal<CompanyDto | null>(null);
  readonly isBranchDrawerOpen = signal(false);
  readonly editingBranch = signal<BranchDto | null>(null);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly companyForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    legalName: ['', [Validators.required, Validators.maxLength(250)]],
    taxId: ['', [Validators.required, Validators.maxLength(20)]],
    ssoEmployerCode: ['', [Validators.required, Validators.maxLength(30)]],
    baseCurrency: ['THB', [Validators.required, Validators.maxLength(3)]],
  });

  readonly branchForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    timezone: ['Asia/Bangkok', [Validators.required]],
    latitude: [13.7563, [Validators.required]],
    longitude: [100.5018, [Validators.required]],
    geofenceRadiusMeters: [250, [Validators.required, Validators.min(10)]],
    allowedIpsText: [''],
  });

  readonly companyColumns: readonly DataTableColumn[] = [
    { key: 'code', label: 'รหัสบริษัท', isMono: true },
    { key: 'legalName', label: 'ชื่อนิติบุคคล' },
    { key: 'taxId', label: 'เลขผู้เสียภาษี', isMono: true },
    { key: 'ssoEmployerCode', label: 'รหัสประกันสังคม', isMono: true },
    { key: 'baseCurrency', label: 'สกุลเงิน', isMono: true },
  ];

  readonly branchColumns: readonly DataTableColumn[] = [
    { key: 'code', label: 'รหัสสาขา', isMono: true },
    { key: 'name', label: 'ชื่อสาขา' },
    { key: 'timezone', label: 'Timezone', isMono: true },
    { key: 'geofenceRadiusMeters', label: 'รัศมี GPS (ม.)', align: 'right', isNumeric: true },
    { key: 'allowedIps', label: 'IP Whitelist', isMono: true },
  ];

  readonly companyActions: readonly DataTableAction<CompanyDto>[] = [
    { key: 'edit', label: 'แก้ไขข้อมูลบริษัท', onClick: (row) => this.openCompanyDrawer(row) },
  ];

  readonly branchActions: readonly DataTableAction<BranchDto>[] = [
    { key: 'edit', label: 'ตั้งค่า GPS / IP', onClick: (row) => this.openBranchDrawer(row) },
  ];

  readonly filteredCompanies = computed(() => {
    const q = this.companySearch().trim().toLowerCase();
    if (q.length === 0) {
      return this.companies();
    }
    return this.companies().filter(
      (c) =>
        c.code.toLowerCase().includes(q) ||
        c.legalName.toLowerCase().includes(q) ||
        c.taxId.toLowerCase().includes(q),
    );
  });

  readonly filteredBranches = computed(() => {
    const q = this.branchSearch().trim().toLowerCase();
    if (q.length === 0) {
      return this.branches();
    }
    return this.branches().filter(
      (b) => b.code.toLowerCase().includes(q) || b.name.toLowerCase().includes(q),
    );
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      companies: this.organizationService.getCompanies(),
      branches: this.organizationService.getBranches(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.companies.set(res.companies);
          this.branches.set(res.branches);
          this.isLoading.set(false);
        },
        error: (err: unknown) => {
          this.companies.set([]);
          this.branches.set([]);
          this.errorMessage.set(
            extractProblemDetailsMessage(err, 'ไม่สามารถโหลดข้อมูลบริษัทและสาขาได้'),
          );
          this.isLoading.set(false);
        },
      });
  }

  openCompanyDrawer(company: CompanyDto | null): void {
    this.editingCompany.set(company);
    this.formError.set(null);
    if (company) {
      this.companyForm.controls.code.disable();
      this.companyForm.reset({
        code: company.code,
        legalName: company.legalName,
        taxId: company.taxId,
        ssoEmployerCode: company.ssoEmployerCode,
        baseCurrency: company.baseCurrency,
      });
    } else {
      this.companyForm.controls.code.enable();
      this.companyForm.reset({
        code: '',
        legalName: '',
        taxId: '',
        ssoEmployerCode: '',
        baseCurrency: 'THB',
      });
    }
    this.isCompanyDrawerOpen.set(true);
  }

  submitCompany(): void {
    if (this.companyForm.invalid) {
      this.companyForm.markAllAsTouched();
      return;
    }

    const raw = this.companyForm.getRawValue();
    const editing = this.editingCompany();
    this.isSubmitting.set(true);
    this.formError.set(null);
    this.feedbackSuccess.set(null);

    const request$ = editing
      ? this.organizationService.updateCompany(editing.id, {
          legalName: raw.legalName.trim(),
          taxId: raw.taxId.trim(),
          ssoEmployerCode: raw.ssoEmployerCode.trim(),
          baseCurrency: raw.baseCurrency.trim().toUpperCase(),
        })
      : this.organizationService.createCompany({
          code: raw.code.trim().toUpperCase(),
          legalName: raw.legalName.trim(),
          taxId: raw.taxId.trim(),
          ssoEmployerCode: raw.ssoEmployerCode.trim(),
          baseCurrency: raw.baseCurrency.trim().toUpperCase(),
        });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isCompanyDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตข้อมูลบริษัทเรียบร้อยแล้ว' : 'เพิ่มบริษัทในเครือเรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        this.handleFormError(err, this.companyForm, 'ไม่สามารถบันทึกข้อมูลบริษัทได้');
      },
    });
  }

  openBranchDrawer(branch: BranchDto | null): void {
    this.editingBranch.set(branch);
    this.formError.set(null);
    if (branch) {
      this.branchForm.controls.code.disable();
      this.branchForm.controls.name.disable();
      this.branchForm.controls.timezone.disable();
      this.branchForm.reset({
        code: branch.code,
        name: branch.name,
        timezone: branch.timezone,
        latitude: branch.latitude,
        longitude: branch.longitude,
        geofenceRadiusMeters: branch.geofenceRadiusMeters,
        allowedIpsText: branch.allowedIps.join(', '),
      });
    } else {
      this.branchForm.controls.code.enable();
      this.branchForm.controls.name.enable();
      this.branchForm.controls.timezone.enable();
      this.branchForm.reset({
        code: '',
        name: '',
        timezone: 'Asia/Bangkok',
        latitude: 13.7563,
        longitude: 100.5018,
        geofenceRadiusMeters: 250,
        allowedIpsText: '',
      });
    }
    this.isBranchDrawerOpen.set(true);
  }

  submitBranch(): void {
    if (this.branchForm.invalid) {
      this.branchForm.markAllAsTouched();
      return;
    }

    const raw = this.branchForm.getRawValue();
    const editing = this.editingBranch();
    const ips = raw.allowedIpsText
      .split(',')
      .map((s) => s.trim())
      .filter((s) => s.length > 0);

    this.isSubmitting.set(true);
    this.formError.set(null);
    this.feedbackSuccess.set(null);

    const request$ = editing
      ? this.organizationService.updateBranchGeofence(editing.id, {
          latitude: Number(raw.latitude),
          longitude: Number(raw.longitude),
          geofenceRadiusMeters: Number(raw.geofenceRadiusMeters),
          allowedIps: ips,
        })
      : this.organizationService.createBranch({
          code: raw.code.trim().toUpperCase(),
          name: raw.name.trim(),
          timezone: raw.timezone.trim(),
          latitude: Number(raw.latitude),
          longitude: Number(raw.longitude),
          geofenceRadiusMeters: Number(raw.geofenceRadiusMeters),
          allowedIps: ips,
        });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isBranchDrawerOpen.set(false);
        this.feedbackSuccess.set(editing ? 'อัปเดตพิกัดสาขาเรียบร้อยแล้ว' : 'เพิ่มสาขาใหม่เรียบร้อยแล้ว');
        this.loadData();
      },
      error: (err: unknown) => {
        this.isSubmitting.set(false);
        this.handleFormError(err, this.branchForm, 'ไม่สามารถบันทึกข้อมูลสาขาได้');
      },
    });
  }

  formatAllowedIps(ips: string[]): string {
    return ips.length > 0 ? ips.join(', ') : '-';
  }

  private handleFormError(err: unknown, form: typeof this.companyForm | typeof this.branchForm, fallback: string): void {
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
