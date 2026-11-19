import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AccessibleBranchDto } from '../auth/auth.models';
import { AuthSessionStore } from '../auth/auth-session.store';
import { AuthService } from '../auth/auth.service';
import { APP_CONFIG } from '../config/app-config.token';
import { ApiToggleService } from '../http/api-toggle.service';
import { extractProblemDetails } from '../http/http-error.helper';
import { ThemeService } from '../theme/theme.service';
import { NavGroup, NavItem, SIDEBAR_NAV_GROUPS } from './navigation.config';
import { TopNavbarComponent } from './top-navbar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TopNavbarComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly appConfig = inject(APP_CONFIG);
  protected readonly authSessionStore = inject(AuthSessionStore);
  protected readonly apiToggleService = inject(ApiToggleService);
  protected readonly themeService = inject(ThemeService);

  protected readonly isMobileNavOpen = signal(false);
  protected readonly isSidebarCollapsed = signal(false);
  protected readonly navSearchQuery = signal('');
  protected readonly isSwitchingContext = signal(false);
  protected readonly isLoggingOut = signal(false);
  protected readonly contextError = signal<string | null>(null);

  protected readonly visibleNavGroups = computed<NavGroup[]>(() => {
    // Touch permissions() so computed re-evaluates whenever user permissions update.
    this.authSessionStore.permissions();
    const query = this.navSearchQuery().trim().toLowerCase();

    const groups: NavGroup[] = [];
    for (const group of SIDEBAR_NAV_GROUPS) {
      const visibleItems: NavItem[] = [];
      for (const item of group.items) {
        const hasPerm = !item.permission || this.authSessionStore.hasPermission(item.permission);
        if (!hasPerm) {
          continue;
        }
        if (query.length > 0 && !item.label.toLowerCase().includes(query)) {
          continue;
        }
        visibleItems.push(item);
      }

      if (visibleItems.length > 0) {
        groups.push({
          groupLabel: group.groupLabel,
          items: visibleItems,
        });
      }
    }

    return groups;
  });

  protected readonly availableBranches = computed<AccessibleBranchDto[]>(() => {
    const activeCompanyId = this.authSessionStore.activeCompanyId();
    const companies = this.authSessionStore.companies();
    if (!activeCompanyId) {
      return [];
    }

    for (const company of companies) {
      if (company.companyId === activeCompanyId) {
        return company.branches;
      }
    }

    return [];
  });

  protected readonly displayUserName = computed(() => {
    const user = this.authSessionStore.user();
    if (!user) {
      return '';
    }
    if (user.fullName && user.fullName.trim().length > 0) {
      return user.fullName;
    }
    return user.email;
  });

  protected readonly userInitials = computed(() => {
    const name = this.displayUserName().trim();
    if (name.length === 0) {
      return 'U';
    }
    const parts = name.split(/\s+/);
    if (parts.length >= 2) {
      return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  });

  protected toggleMobileNav(): void {
    this.isMobileNavOpen.set(!this.isMobileNavOpen());
  }

  protected toggleSidebarCollapse(): void {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed());
  }

  protected onNavSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.navSearchQuery.set(target.value);
  }

  protected closeMobileNav(): void {
    this.isMobileNavOpen.set(false);
  }

  protected onToggleTheme(): void {
    this.themeService.toggleTheme();
  }

  protected onToggleApi(): void {
    this.apiToggleService.toggleApi();
  }

  protected onCompanyChange(nextCompanyId: string): void {
    if (!nextCompanyId) {
      return;
    }

    let firstBranchId: string | null = null;
    for (const company of this.authSessionStore.companies()) {
      if (company.companyId === nextCompanyId && company.branches.length > 0) {
        firstBranchId = company.branches[0].branchId;
        break;
      }
    }

    this.applyContextSwitch(nextCompanyId, firstBranchId);
  }

  protected onBranchChange(nextBranchId: string | null): void {
    const currentCompanyId = this.authSessionStore.activeCompanyId();
    if (!currentCompanyId) {
      return;
    }

    this.applyContextSwitch(currentCompanyId, nextBranchId);
  }

  protected onLogout(): void {
    if (this.isLoggingOut()) {
      return;
    }

    this.isLoggingOut.set(true);
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isLoggingOut.set(false);
          void this.router.navigate(['/auth/login']);
        },
        error: () => {
          this.isLoggingOut.set(false);
          void this.router.navigate(['/auth/login']);
        },
      });
  }

  private applyContextSwitch(companyId: string, branchId: string | null): void {
    this.isSwitchingContext.set(true);
    this.contextError.set(null);

    this.authService
      .switchContext({ companyId, branchId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSwitchingContext.set(false);
        },
        error: (err: unknown) => {
          this.isSwitchingContext.set(false);
          const problem = extractProblemDetails(err);
          this.contextError.set(problem.detail ?? problem.title);
        },
      });
  }
}
