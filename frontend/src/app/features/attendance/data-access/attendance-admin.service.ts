import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { AttendanceDailySummaryDto } from '../../portal/models/portal-dashboard.models';
import {
  BulkAssignRosterRequest,
  CreateShiftRequest,
  RosterItemDto,
  ShiftDefinitionDto,
  UpdateShiftRequest,
} from '../models/attendance.models';

@Injectable({ providedIn: 'root' })
export class AttendanceAdminService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getShifts(): Observable<ShiftDefinitionDto[]> {
    return this.http.get<ShiftDefinitionDto[]>(`${this.appConfig.apiBaseUrl}/shifts`);
  }

  createShift(request: CreateShiftRequest): Observable<ShiftDefinitionDto> {
    return this.http.post<ShiftDefinitionDto>(`${this.appConfig.apiBaseUrl}/shifts`, request);
  }

  updateShift(id: string, request: UpdateShiftRequest): Observable<ShiftDefinitionDto> {
    return this.http.put<ShiftDefinitionDto>(`${this.appConfig.apiBaseUrl}/shifts/${id}`, request);
  }

  getRosters(
    startDate: string,
    endDate: string,
    branchId?: string | null,
    departmentId?: string | null,
  ): Observable<RosterItemDto[]> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (branchId && branchId.trim().length > 0) {
      params = params.set('branchId', branchId.trim());
    }
    if (departmentId && departmentId.trim().length > 0) {
      params = params.set('departmentId', departmentId.trim());
    }
    return this.http.get<RosterItemDto[]>(`${this.appConfig.apiBaseUrl}/rosters`, { params });
  }

  bulkAssignRoster(request: BulkAssignRosterRequest): Observable<{ assignedCount: number }> {
    return this.http.post<{ assignedCount: number }>(
      `${this.appConfig.apiBaseUrl}/rosters/bulk-assign`,
      request,
    );
  }

  getDailySummaries(
    startDate: string,
    endDate: string,
    branchId?: string | null,
    departmentId?: string | null,
    status?: string | null,
  ): Observable<AttendanceDailySummaryDto[]> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (branchId && branchId.trim().length > 0) {
      params = params.set('branchId', branchId.trim());
    }
    if (departmentId && departmentId.trim().length > 0) {
      params = params.set('departmentId', departmentId.trim());
    }
    if (status && status.trim().length > 0) {
      params = params.set('status', status.trim());
    }
    return this.http.get<AttendanceDailySummaryDto[]>(
      `${this.appConfig.apiBaseUrl}/attendance/daily-summary`,
      { params },
    );
  }
}
