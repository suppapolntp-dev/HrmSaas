import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, finalize, firstValueFrom, tap } from 'rxjs';
import { APP_CONFIG } from '../config/app-config.token';
import { AuthSessionStore } from './auth-session.store';
import { AuthMeResponse, LoginRequest, RegisterRequest, SwitchContextRequest } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);
  private readonly authSessionStore = inject(AuthSessionStore);

  private pendingInitPromise: Promise<void> | null = null;
  readonly isInitializing = signal<boolean>(false);

  initializeSession(): Promise<void> {
    if (this.authSessionStore.isInitialized()) {
      return Promise.resolve();
    }

    if (this.pendingInitPromise !== null) {
      return this.pendingInitPromise;
    }

    this.isInitializing.set(true);
    const url = `${this.appConfig.apiBaseUrl}/auth/me`;

    this.pendingInitPromise = firstValueFrom(this.http.get<AuthMeResponse>(url))
      .then((me) => {
        this.authSessionStore.setSession(me);
      })
      .catch(() => {
        this.authSessionStore.clearSession();
      })
      .finally(() => {
        this.isInitializing.set(false);
        this.pendingInitPromise = null;
      });

    return this.pendingInitPromise;
  }

  login(request: LoginRequest): Observable<AuthMeResponse> {
    const url = `${this.appConfig.apiBaseUrl}/auth/login`;
    return this.http.post<AuthMeResponse>(url, request).pipe(
      tap((me) => {
        this.authSessionStore.setSession(me);
      }),
    );
  }

  register(request: RegisterRequest): Observable<AuthMeResponse> {
    const url = `${this.appConfig.apiBaseUrl}/auth/register`;
    return this.http.post<AuthMeResponse>(url, request).pipe(
      tap((me) => {
        this.authSessionStore.setSession(me);
      }),
    );
  }

  logout(): Observable<void> {
    const url = `${this.appConfig.apiBaseUrl}/auth/logout`;
    return this.http.post<void>(url, {}).pipe(
      finalize(() => {
        this.authSessionStore.clearSession();
      }),
    );
  }

  switchContext(request: SwitchContextRequest): Observable<AuthMeResponse> {
    const url = `${this.appConfig.apiBaseUrl}/auth/switch-context`;
    return this.http.post<AuthMeResponse>(url, request).pipe(
      tap((updated) => {
        this.authSessionStore.setSession(updated);
      }),
    );
  }

  refreshMe(): Observable<AuthMeResponse> {
    const url = `${this.appConfig.apiBaseUrl}/auth/me`;
    return this.http.get<AuthMeResponse>(url).pipe(
      tap((me) => {
        this.authSessionStore.setSession(me);
      }),
    );
  }
}
