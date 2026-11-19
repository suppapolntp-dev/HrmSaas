import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { AuthService } from './core/auth/auth.service';
import { apiToggleInterceptor } from './core/http/api-toggle.interceptor';
import { credentialsInterceptor } from './core/http/credentials.interceptor';
import { problemDetailsInterceptor } from './core/http/problem-details.interceptor';
import { tenantContextInterceptor } from './core/http/tenant-context.interceptor';
import { ThemeService } from './core/theme/theme.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        apiToggleInterceptor,
        credentialsInterceptor,
        tenantContextInterceptor,
        problemDetailsInterceptor,
      ]),
    ),
    provideAppInitializer(() => {
      inject(ThemeService);
      return inject(AuthService).initializeSession();
    }),
  ],
};
