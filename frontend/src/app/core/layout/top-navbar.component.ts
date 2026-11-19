import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { AccessibleBranchDto, AccessibleCompanyDto } from '../auth/auth.models';
import { ThemeMode } from '../theme/theme.service';

@Component({
  selector: 'app-top-navbar',
  standalone: true,
  templateUrl: './top-navbar.component.html',
  styleUrl: './top-navbar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopNavbarComponent {
  readonly companies = input<AccessibleCompanyDto[]>([]);
  readonly activeCompanyId = input<string | null>(null);
  readonly availableBranches = input<AccessibleBranchDto[]>([]);
  readonly activeBranchId = input<string | null>(null);
  readonly isSwitchingContext = input<boolean>(false);
  readonly enableApiToggle = input<boolean>(false);
  readonly isApiEnabled = input<boolean>(true);
  readonly theme = input<ThemeMode>('light');
  readonly isLoggingOut = input<boolean>(false);

  readonly mobileNavToggle = output<void>();
  readonly companyChange = output<string>();
  readonly branchChange = output<string | null>();
  readonly apiToggle = output<void>();
  readonly themeToggle = output<void>();
  readonly logout = output<void>();

  protected onSelectCompany(event: Event): void {
    const target = event.target as HTMLSelectElement;
    if (target.value.length > 0) {
      this.companyChange.emit(target.value);
    }
  }

  protected onSelectBranch(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const nextBranchId = target.value.length > 0 ? target.value : null;
    this.branchChange.emit(nextBranchId);
  }
}
