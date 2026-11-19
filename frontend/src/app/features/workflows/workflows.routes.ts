import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { ApprovalInboxPageComponent } from './pages/approval-inbox/approval-inbox-page.component';
import { ApprovalRoutesPageComponent } from './pages/approval-routes/approval-routes-page.component';
import { LeavePoliciesPageComponent } from './pages/leave-policies/leave-policies-page.component';

export const WORKFLOWS_ROUTES: Routes = [
  {
    path: 'inbox',
    canMatch: [permissionGuard('workflow.approve')],
    component: ApprovalInboxPageComponent,
    data: { title: 'กล่องอนุมัติคำขอ' },
  },
  {
    path: 'leave-policies',
    canMatch: [permissionGuard('workflow.policy.manage')],
    component: LeavePoliciesPageComponent,
    data: { title: 'นโยบายวันลา' },
  },
  {
    path: 'approval-routes',
    canMatch: [permissionGuard('workflow.route.manage')],
    component: ApprovalRoutesPageComponent,
    data: { title: 'ตั้งค่าสายอนุมัติ' },
  },
];
