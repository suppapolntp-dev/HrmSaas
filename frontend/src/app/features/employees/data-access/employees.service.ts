import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { PagedResult } from '../../../shared/models/api-response.model';
import {
  CreateContractRequest,
  Employee360Dto,
  EmployeeContractDto,
  EmployeeListItemDto,
  EmployeeListQueryParams,
  OnboardEmployeeRequest,
  UpdateEmployeeRequest,
  UpdateReportingLinesRequest,
} from '../models/employee.models';

@Injectable({ providedIn: 'root' })
export class EmployeesService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getEmployees(query: EmployeeListQueryParams): Observable<PagedResult<EmployeeListItemDto>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.branchId && query.branchId.trim().length > 0) {
      params = params.set('branchId', query.branchId.trim());
    }
    if (query.departmentId && query.departmentId.trim().length > 0) {
      params = params.set('departmentId', query.departmentId.trim());
    }
    if (query.status && query.status.trim().length > 0) {
      params = params.set('status', query.status.trim());
    }
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.sortBy && query.sortBy.trim().length > 0) {
      params = params.set('sortBy', query.sortBy.trim());
      params = params.set('sortDesc', String(Boolean(query.sortDesc)));
    }

    return this.http.get<PagedResult<EmployeeListItemDto>>(`${this.appConfig.apiBaseUrl}/employees`, {
      params,
    });
  }

  getEmployeeById(id: string): Observable<Employee360Dto> {
    return this.http.get<Employee360Dto>(`${this.appConfig.apiBaseUrl}/employees/${id}`);
  }

  onboardEmployee(request: OnboardEmployeeRequest): Observable<Employee360Dto> {
    return this.http.post<Employee360Dto>(`${this.appConfig.apiBaseUrl}/employees/onboarding`, request);
  }

  updateEmployee(id: string, request: UpdateEmployeeRequest): Observable<Employee360Dto> {
    return this.http.put<Employee360Dto>(`${this.appConfig.apiBaseUrl}/employees/${id}`, request);
  }

  updateReportingLines(id: string, request: UpdateReportingLinesRequest): Observable<void> {
    return this.http.put<void>(`${this.appConfig.apiBaseUrl}/employees/${id}/reporting-lines`, request);
  }

  createContract(id: string, request: CreateContractRequest): Observable<EmployeeContractDto> {
    return this.http.post<EmployeeContractDto>(`${this.appConfig.apiBaseUrl}/employees/${id}/contracts`, request);
  }
}
