import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AccessibleBranchDto } from '../../../../core/auth/auth.models';
import { AuthSessionStore } from '../../../../core/auth/auth-session.store';
import { AuthService } from '../../../../core/auth/auth.service';
import { extractProblemDetails } from '../../../../core/http/http-error.helper';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { StateContainerComponent } from '../../../../shared/ui/state-container/state-container.component';

@Component({
  selector: 'app-select-company-page',
  standalone: true,
  imports: [FormFieldComponent, StateContainerComponent],
  templateUrl: './select-company-page.component.html',
  styleUrl: './select-company-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SelectCompanyPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  protected readonly authSessionStore = inject(AuthSessionStore);

  protected readonly selectedCompanyId = signal<string>(this.resolveInitialCompanyId());
  protected readonly selectedBranchId = signal<string | null>(this.resolveInitialBranchId());
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly companies = this.authSessionStore.companies;

  protected readonly branchesForSelectedCompany = computed<AccessibleBranchDto[]>(() => {
    const companyId = this.selectedCompanyId();
    for (const company of this.companies()) {
      if (company.companyId === companyId) {
        return company.branches;
      }
    }
    return [];
  });

  protected onSelectCompany(companyId: string): void {
    this.selectedCompanyId.set(companyId);
    this.errorMessage.set(null);

    let defaultBranchId: string | null = null;
    for (const company of this.companies()) {
      if (company.companyId === companyId && company.branches.length > 0) {
        defaultBranchId = company.branches[0].branchId;
        break;
      }
    }
    this.selectedBranchId.set(defaultBranchId);
  }

  protected onBranchChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedBranchId.set(select.value.length > 0 ? select.value : null);
  }

  protected onReloadContext(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .refreshMe()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (me) => {
          this.isLoading.set(false);
          if (me.activeCompanyId) {
            this.selectedCompanyId.set(me.activeCompanyId);
          } else if (me.companies.length > 0) {
            this.onSelectCompany(me.companies[0].companyId);
          }
        },
        error: (err: unknown) => {
          this.isLoading.set(false);
          const problem = extractProblemDetails(err);
          this.errorMessage.set(problem.detail ?? problem.title);
        },
      });
  }

  protected onContinue(): void {
    const companyId = this.selectedCompanyId();
    if (!companyId || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authService
      .switchContext({
        companyId,
        branchId: this.selectedBranchId(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          void this.router.navigate(['/portal/dashboard']);
        },
        error: (err: unknown) => {
          this.isSubmitting.set(false);
          const problem = extractProblemDetails(err);
          this.errorMessage.set(problem.detail ?? problem.title);
        },
      });
  }

  protected onLogout(): void {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigate(['/auth/login']);
        },
        error: () => {
          void this.router.navigate(['/auth/login']);
        },
      });
  }

  private resolveInitialCompanyId(): string {
    const active = this.authSessionStore.activeCompanyId();
    if (active) {
      return active;
    }

    const list = this.authSessionStore.companies();
    if (list.length > 0) {
      return list[0].companyId;
    }

    return '';
  }

  private resolveInitialBranchId(): string | null {
    const activeBranch = this.authSessionStore.activeBranchId();
    if (activeBranch) {
      return activeBranch;
    }

    const companyId = this.resolveInitialCompanyId();
    for (const company of this.authSessionStore.companies()) {
      if (company.companyId === companyId && company.branches.length > 0) {
        return company.branches[0].branchId;
      }
    }

    return null;
  }
}
