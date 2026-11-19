import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthSessionStore } from './auth-session.store';
import { AuthService } from './auth.service';

export function permissionGuard(permissionKey: string): CanMatchFn {
  return async () => {
    const authService = inject(AuthService);
    const authSessionStore = inject(AuthSessionStore);
    const router = inject(Router);

    await authService.initializeSession();

    if (!authSessionStore.isAuthenticated()) {
      return router.createUrlTree(['/auth/login']);
    }

    if (authSessionStore.hasPermission(permissionKey)) {
      return true;
    }

    return router.createUrlTree(['/portal/dashboard']);
  };
}
