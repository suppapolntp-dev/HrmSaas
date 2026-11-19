import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import {
  AttendanceDailySummaryDto,
  TimeAdjustmentCreatedDto,
  TimeAdjustmentRequest,
} from '../models/portal-dashboard.models';

@Injectable({ providedIn: 'root' })
export class PortalAttendanceService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getAttendanceHistory(
    startDate?: string,
    endDate?: string,
    status?: string,
  ): Observable<AttendanceDailySummaryDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/attendance/my-history`;
    let params = new HttpParams();

    if (startDate && startDate.trim().length > 0) {
      params = params.set('startDate', startDate.trim());
    }
    if (endDate && endDate.trim().length > 0) {
      params = params.set('endDate', endDate.trim());
    }
    if (status && status.trim().length > 0) {
      params = params.set('status', status.trim());
    }

    return this.http.get<AttendanceDailySummaryDto[]>(url, { params });
  }

  requestTimeAdjustment(request: TimeAdjustmentRequest): Observable<TimeAdjustmentCreatedDto> {
    const url = `${this.appConfig.apiBaseUrl}/attendance/time-adjustments`;
    return this.http.post<TimeAdjustmentCreatedDto>(url, request);
  }
}
