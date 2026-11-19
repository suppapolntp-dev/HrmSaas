import { Injectable, computed, signal } from '@angular/core';
import { AccessibleCompanyDto, AuthMeResponse } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthSessionStore {
  readonly user = signal<AuthMeResponse | null>(null);
  readonly isInitialized = signal<boolean>(false);
  readonly activeCompanyId = signal<string | null>(null);
  readonly activeBranchId = signal<string | null>(null);
  readonly permissions = signal<string[]>([]);
  readonly companies = signal<AccessibleCompanyDto[]>([]);

  readonly isAuthenticated = computed(() => this.user() !== null);

  setSession(response: AuthMeResponse): void {
    this.user.set(response);
    this.activeCompanyId.set(response.activeCompanyId);
    this.activeBranchId.set(response.activeBranchId);
    this.permissions.set(response.permissions);
    this.companies.set(response.companies);
    this.isInitialized.set(true);
  }

  clearSession(): void {
    this.user.set(null);
    this.activeCompanyId.set(null);
    this.activeBranchId.set(null);
    this.permissions.set([]);
    this.companies.set([]);
    this.isInitialized.set(true);
  }

  markInitialized(): void {
    this.isInitialized.set(true);
  }

  hasPermission(permissionKey: string): boolean {
    if (permissionKey.trim().length === 0) {
      return true;
    }

    const currentPermissions = this.permissions();
    for (const item of currentPermissions) {
      if (item === permissionKey) {
        return true;
      }
    }

    return false;
  }
}
