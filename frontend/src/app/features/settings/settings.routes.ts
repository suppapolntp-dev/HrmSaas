import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { AuditLogsPageComponent } from './pages/audit-logs/audit-logs-page.component';
import { RolesPermissionsPageComponent } from './pages/roles-permissions/roles-permissions-page.component';

export const SETTINGS_ROUTES: Routes = [
  {
    path: 'roles-permissions',
    canMatch: [permissionGuard('settings.rbac.manage')],
    component: RolesPermissionsPageComponent,
    data: { title: 'บทบาทและสิทธิ์ (RBAC)' },
  },
  {
    path: 'audit-logs',
    canMatch: [permissionGuard('settings.audit.read')],
    component: AuditLogsPageComponent,
    data: { title: 'ประวัติการใช้งาน (Audit Logs)' },
  },
];
