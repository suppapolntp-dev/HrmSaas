import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthSessionStore } from '../auth/auth-session.store';
import { extractProblemDetails } from './http-error.helper';

export const problemDetailsInterceptor: HttpInterceptorFn = (req, next) => {
  const authSessionStore = inject(AuthSessionStore);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      const problemDetails = extractProblemDetails(error);

      if (error instanceof HttpErrorResponse) {
        const isAuthProbeOrLogin =
          req.url.includes('/auth/me') || req.url.includes('/auth/login');

        if (error.status === 401 && !isAuthProbeOrLogin) {
          authSessionStore.clearSession();
          void router.navigate(['/auth/login']);
        }

        if (!(error.status === 401 && req.url.includes('/auth/me'))) {
          console.error('[HrmSaas API Error]', {
            method: req.method,
            url: req.urlWithParams,
            status: problemDetails.status,
            title: problemDetails.title,
            detail: problemDetails.detail,
            traceId: problemDetails.traceId,
            hint: resolveDiagnosticHint(req.url, problemDetails.status),
          });
        }

        const normalizedError = new HttpErrorResponse({
          status: problemDetails.status,
          statusText: error.statusText || problemDetails.title,
          url: error.url ?? req.url,
          headers: error.headers,
          error: problemDetails,
        });

        return throwError(() => normalizedError);
      }

      return throwError(() => error);
    }),
  );
};

function resolveDiagnosticHint(url: string, status: number): string {
  if (status === 404 && url.startsWith('/api/')) {
    return 'Endpoint ไม่พบใน Backend ที่รันอยู่ — ตรวจว่า rebuild Docker hrmsaas-backend แล้วหรือยัง';
  }
  if (status === 503) {
    return 'API Toggle ถูกปิดอยู่ (API: OFF)';
  }
  if (status === 403) {
    return 'บัญชีปัจจุบันไม่มี Permission key สำหรับเส้นทางนี้';
  }
  if (status >= 500) {
    return 'Backend เกิด Internal Server Error — ตรวจ docker logs hrmsaas-backend';
  }
  return 'ดูรายละเอียดเพิ่มเติมใน Network tab หรือ Backend logs';
}
