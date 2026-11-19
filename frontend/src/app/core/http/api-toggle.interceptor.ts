import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { ProblemDetails } from '../../shared/models/api-response.model';
import { ApiToggleService } from './api-toggle.service';

export const apiToggleInterceptor: HttpInterceptorFn = (req, next) => {
  const apiToggleService = inject(ApiToggleService);

  if (!req.url.startsWith('/api')) {
    return next(req);
  }

  if (apiToggleService.isApiEnabled()) {
    return next(req);
  }

  const problemDetails: ProblemDetails = {
    type: 'https://httpstatuses.com/503',
    title: 'API ถูกปิดสำหรับการทดสอบ (API: OFF)',
    status: 503,
    detail: 'ระบบจำลองสถานะขาดการเชื่อมต่อ API เพื่อตรวจสอบว่าหน้าจอนี้ไม่ได้ Hardcode ข้อมูลไว้',
  };

  const blockedResponse = new HttpErrorResponse({
    status: 503,
    statusText: 'Service Unavailable',
    url: req.url,
    error: problemDetails,
  });

  return throwError(() => blockedResponse);
};
