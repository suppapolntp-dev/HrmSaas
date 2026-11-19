import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { ApprovalRequestDto, MyLeaveBalanceDto } from '../models/portal-dashboard.models';
import { SubmitLeaveRequest } from '../models/portal-ess.models';

@Injectable({ providedIn: 'root' })
export class PortalLeavesService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getMyBalances(year?: number): Observable<MyLeaveBalanceDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/leaves/my-balances`;
    let params = new HttpParams();
    if (year !== undefined) {
      params = params.set('year', String(year));
    }
    return this.http.get<MyLeaveBalanceDto[]>(url, { params });
  }

  getMyRequests(): Observable<ApprovalRequestDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/leaves/my-requests`;
    return this.http.get<ApprovalRequestDto[]>(url);
  }

  submitLeaveRequest(request: SubmitLeaveRequest): Observable<ApprovalRequestDto> {
    const url = `${this.appConfig.apiBaseUrl}/leaves/requests`;
    return this.http.post<ApprovalRequestDto>(url, request);
  }

  cancelLeaveRequest(id: string): Observable<ApprovalRequestDto> {
    const url = `${this.appConfig.apiBaseUrl}/leaves/requests/${id}/cancel`;
    return this.http.post<ApprovalRequestDto>(url, {});
  }
}
