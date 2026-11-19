import { Routes } from '@angular/router';
import { sessionGuard } from './core/auth/auth.guard';
import { AppShellComponent } from './core/layout/app-shell.component';
import { AuthShellComponent } from './core/layout/auth-shell.component';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthShellComponent,
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    component: AppShellComponent,
    canMatch: [sessionGuard],
    children: [
      {
        path: 'portal',
        loadChildren: () => import('./features/portal/portal.routes').then((m) => m.PORTAL_ROUTES),
      },
      {
        path: 'org',
        loadChildren: () =>
          import('./features/organization/organization.routes').then((m) => m.ORGANIZATION_ROUTES),
      },
      {
        path: 'employees',
        loadChildren: () =>
          import('./features/employees/employees.routes').then((m) => m.EMPLOYEES_ROUTES),
      },
      {
        path: 'attendance',
        loadChildren: () =>
          import('./features/attendance/attendance.routes').then((m) => m.ATTENDANCE_ROUTES),
      },
      {
        path: 'workflows',
        loadChildren: () =>
          import('./features/workflows/workflows.routes').then((m) => m.WORKFLOWS_ROUTES),
      },
      {
        path: 'payroll',
        loadChildren: () =>
          import('./features/payroll/payroll.routes').then((m) => m.PAYROLL_ROUTES),
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./features/settings/settings.routes').then((m) => m.SETTINGS_ROUTES),
      },
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'portal/dashboard',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'portal/dashboard',
  },
];
