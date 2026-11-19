import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { APP_CONFIG } from '../config/app-config.token';
import { ApiToggleService } from '../http/api-toggle.service';
import { ThemeService } from '../theme/theme.service';

@Component({
  selector: 'app-auth-shell',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './auth-shell.component.html',
  styleUrl: './auth-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthShellComponent {
  protected readonly appConfig = inject(APP_CONFIG);
  protected readonly apiToggleService = inject(ApiToggleService);
  protected readonly themeService = inject(ThemeService);

  protected onToggleTheme(): void {
    this.themeService.toggleTheme();
  }

  protected onToggleApi(): void {
    this.apiToggleService.toggleApi();
  }
}
