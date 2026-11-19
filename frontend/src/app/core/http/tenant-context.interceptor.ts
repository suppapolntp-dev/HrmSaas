import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthSessionStore } from '../auth/auth-session.store';

export const tenantContextInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api')) {
    return next(req);
  }

  const authSessionStore = inject(AuthSessionStore);
  const companyId = authSessionStore.activeCompanyId();
  const branchId = authSessionStore.activeBranchId();

  const headersToAdd: Record<string, string> = {};
  if (companyId !== null && companyId.length > 0) {
    headersToAdd['X-Company-Id'] = companyId;
  }

  if (branchId !== null && branchId.length > 0) {
    headersToAdd['X-Branch-Id'] = branchId;
  }

  if (Object.keys(headersToAdd).length === 0) {
    return next(req);
  }

  return next(req.clone({ setHeaders: headersToAdd }));
};
