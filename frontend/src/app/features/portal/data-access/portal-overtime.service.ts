import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { ApprovalRequestDto } from '../models/portal-dashboard.models';
import { SubmitOvertimeRequest } from '../models/portal-ess.models';

@Injectable({ providedIn: 'root' })
export class PortalOvertimeService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getMyRequests(): Observable<ApprovalRequestDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/overtime/my-requests`;
    return this.http.get<ApprovalRequestDto[]>(url);
  }

  submitOvertimeRequest(request: SubmitOvertimeRequest): Observable<ApprovalRequestDto> {
    const url = `${this.appConfig.apiBaseUrl}/overtime/requests`;
    return this.http.post<ApprovalRequestDto>(url, request);
  }
}
