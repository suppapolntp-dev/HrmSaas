import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import {
  ApprovalRequestDto,
  AttendanceDailySummaryDto,
  CheckInOutRequest,
  DecideApprovalRequest,
  HrOverviewDto,
  MyDashboardSummaryDto,
  MyLeaveBalanceDto,
  MyTodayAttendanceDto,
  MyUpcomingRosterDto,
  PayrollReadinessDto,
  TeamPulseDto,
} from '../models/portal-dashboard.models';

@Injectable({ providedIn: 'root' })
export class PortalDashboardService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getMyTodayAttendance(): Observable<MyTodayAttendanceDto> {
    const url = `${this.appConfig.apiBaseUrl}/attendance/my-today`;
    return this.http.get<MyTodayAttendanceDto>(url);
  }

  checkIn(request: CheckInOutRequest): Observable<AttendanceDailySummaryDto> {
    const url = `${this.appConfig.apiBaseUrl}/attendance/check-in`;
    return this.http.post<AttendanceDailySummaryDto>(url, request);
  }

  checkOut(request: CheckInOutRequest): Observable<AttendanceDailySummaryDto> {
    const url = `${this.appConfig.apiBaseUrl}/attendance/check-out`;
    return this.http.post<AttendanceDailySummaryDto>(url, request);
  }

  getMyLeaveBalances(year?: number): Observable<MyLeaveBalanceDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/leaves/my-balances`;
    let params = new HttpParams();
    if (year !== undefined) {
      params = params.set('year', String(year));
    }
    return this.http.get<MyLeaveBalanceDto[]>(url, { params });
  }

  getMyUpcomingRosters(days = 7): Observable<MyUpcomingRosterDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/rosters/my-upcoming`;
    const params = new HttpParams().set('days', String(days));
    return this.http.get<MyUpcomingRosterDto[]>(url, { params });
  }

  getPendingApprovals(): Observable<ApprovalRequestDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/approvals/pending`;
    return this.http.get<ApprovalRequestDto[]>(url);
  }

  decideApproval(requestId: string, request: DecideApprovalRequest): Observable<ApprovalRequestDto> {
    const url = `${this.appConfig.apiBaseUrl}/approvals/${requestId}/decide`;
    return this.http.post<ApprovalRequestDto>(url, request);
  }

  getMySummary(): Observable<MyDashboardSummaryDto> {
    const url = `${this.appConfig.apiBaseUrl}/dashboard/my-summary`;
    return this.http.get<MyDashboardSummaryDto>(url);
  }

  getTeamPulse(): Observable<TeamPulseDto> {
    const url = `${this.appConfig.apiBaseUrl}/dashboard/team-pulse`;
    return this.http.get<TeamPulseDto>(url);
  }

  getHrOverview(): Observable<HrOverviewDto> {
    const url = `${this.appConfig.apiBaseUrl}/dashboard/hr-overview`;
    return this.http.get<HrOverviewDto>(url);
  }

  getPayrollReadiness(): Observable<PayrollReadinessDto> {
    const url = `${this.appConfig.apiBaseUrl}/dashboard/payroll-readiness`;
    return this.http.get<PayrollReadinessDto>(url);
  }
}
