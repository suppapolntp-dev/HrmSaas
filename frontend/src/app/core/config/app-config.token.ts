import { InjectionToken, isDevMode } from '@angular/core';

export interface AppConfig {
  apiBaseUrl: string;
  defaultPageSize: number;
  pageSizeOptions: readonly number[];
  enableApiToggle: boolean;
}

export const DEFAULT_APP_CONFIG: AppConfig = {
  apiBaseUrl: '/api/v1',
  defaultPageSize: 20,
  pageSizeOptions: [10, 20, 50, 100],
  enableApiToggle: isDevMode(),
};

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG', {
  providedIn: 'root',
  factory: () => DEFAULT_APP_CONFIG,
});
