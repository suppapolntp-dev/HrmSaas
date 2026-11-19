import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { PayrollPeriodsPageComponent } from './pages/payroll-periods/payroll-periods-page.component';
import { PayrollRunPageComponent } from './pages/payroll-run/payroll-run-page.component';

export const PAYROLL_ROUTES: Routes = [
  {
    path: 'periods',
    canMatch: [permissionGuard('payroll.period.manage')],
    component: PayrollPeriodsPageComponent,
    data: { title: 'งวดเงินเดือน' },
  },
  {
    path: 'run/:periodId',
    canMatch: [permissionGuard('payroll.calculate')],
    component: PayrollRunPageComponent,
    data: { title: 'คำนวณเงินเดือนประจำงวด' },
  },
];
