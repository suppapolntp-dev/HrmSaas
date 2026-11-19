import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthSessionStore } from './auth-session.store';
import { AuthService } from './auth.service';

export const sessionGuard: CanMatchFn = async () => {
  const authService = inject(AuthService);
  const authSessionStore = inject(AuthSessionStore);
  const router = inject(Router);

  await authService.initializeSession();

  if (authSessionStore.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};

export const guestOnlyGuard: CanMatchFn = async () => {
  const authService = inject(AuthService);
  const authSessionStore = inject(AuthSessionStore);
  const router = inject(Router);

  await authService.initializeSession();

  if (!authSessionStore.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/portal/dashboard']);
};
