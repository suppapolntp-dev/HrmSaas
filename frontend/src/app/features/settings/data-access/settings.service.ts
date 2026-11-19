import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { PagedResult } from '../../../shared/models/api-response.model';
import {
  AuditLogDto,
  AuditLogQueryParams,
  CreateRoleRequest,
  PermissionDto,
  RoleDto,
  UpdateRoleRequest,
} from '../models/settings.models';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getPermissions(): Observable<PermissionDto[]> {
    return this.http.get<PermissionDto[]>(`${this.appConfig.apiBaseUrl}/permissions`);
  }

  getRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${this.appConfig.apiBaseUrl}/roles`);
  }

  createRole(request: CreateRoleRequest): Observable<RoleDto> {
    return this.http.post<RoleDto>(`${this.appConfig.apiBaseUrl}/roles`, request);
  }

  updateRole(id: string, request: UpdateRoleRequest): Observable<RoleDto> {
    return this.http.put<RoleDto>(`${this.appConfig.apiBaseUrl}/roles/${id}`, request);
  }

  getAuditLogs(query: AuditLogQueryParams): Observable<PagedResult<AuditLogDto>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.entityName && query.entityName.trim().length > 0) {
      params = params.set('entityName', query.entityName.trim());
    }
    if (query.action && query.action.trim().length > 0) {
      params = params.set('action', query.action.trim());
    }
    if (query.fromDate && query.fromDate.trim().length > 0) {
      params = params.set('fromDate', query.fromDate.trim());
    }
    if (query.toDate && query.toDate.trim().length > 0) {
      params = params.set('toDate', query.toDate.trim());
    }

    return this.http.get<PagedResult<AuditLogDto>>(`${this.appConfig.apiBaseUrl}/audit-logs`, { params });
  }
}
