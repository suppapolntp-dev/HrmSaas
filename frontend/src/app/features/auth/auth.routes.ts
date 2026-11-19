import { Routes } from '@angular/router';
import { guestOnlyGuard, sessionGuard } from '../../core/auth/auth.guard';
import { LoginPageComponent } from './pages/login/login-page.component';
import { SelectCompanyPageComponent } from './pages/select-company/select-company-page.component';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    canMatch: [guestOnlyGuard],
    component: LoginPageComponent,
  },
  {
    path: 'select-company',
    canMatch: [sessionGuard],
    component: SelectCompanyPageComponent,
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
];
