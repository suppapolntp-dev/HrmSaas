import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { EmployeeDetailPageComponent } from './pages/employee-detail/employee-detail-page.component';
import { EmployeeListPageComponent } from './pages/employee-list/employee-list-page.component';
import { EmployeeOnboardingPageComponent } from './pages/employee-onboarding/employee-onboarding-page.component';

export const EMPLOYEES_ROUTES: Routes = [
  {
    path: 'list',
    canMatch: [permissionGuard('employee.read')],
    component: EmployeeListPageComponent,
    data: { title: 'รายชื่อพนักงาน' },
  },
  {
    path: 'onboarding',
    canMatch: [permissionGuard('employee.create')],
    component: EmployeeOnboardingPageComponent,
    data: { title: 'รับพนักงานใหม่' },
  },
  {
    path: ':id',
    canMatch: [permissionGuard('employee.read')],
    component: EmployeeDetailPageComponent,
    data: { title: 'โปรไฟล์พนักงาน 360' },
  },
];
