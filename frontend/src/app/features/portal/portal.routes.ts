import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { AttendancePageComponent } from './pages/attendance/attendance-page.component';
import { LeavesPageComponent } from './pages/leaves/leaves-page.component';
import { OvertimePageComponent } from './pages/overtime/overtime-page.component';
import { PayslipsPageComponent } from './pages/payslips/payslips-page.component';
import { PortalDashboardPageComponent } from './pages/portal-dashboard/portal-dashboard-page.component';

export const PORTAL_ROUTES: Routes = [
  {
    path: 'dashboard',
    component: PortalDashboardPageComponent,
    data: { title: 'แดชบอร์ดพนักงาน' },
  },
  {
    path: 'attendance',
    canMatch: [permissionGuard('portal.attendance.read')],
    component: AttendancePageComponent,
    data: { title: 'เวลาทำงานของฉัน' },
  },
  {
    path: 'leaves',
    canMatch: [permissionGuard('portal.leave.read')],
    component: LeavesPageComponent,
    data: { title: 'การลาของฉัน' },
  },
  {
    path: 'overtime',
    canMatch: [permissionGuard('portal.overtime.read')],
    component: OvertimePageComponent,
    data: { title: 'คำขอ OT ของฉัน' },
  },
  {
    path: 'payslips',
    canMatch: [permissionGuard('portal.payslip.read')],
    component: PayslipsPageComponent,
    data: { title: 'สลิปเงินเดือน' },
  },
];
