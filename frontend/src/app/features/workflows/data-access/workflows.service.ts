import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { ApprovalRequestDto, DecideApprovalRequest } from '../../portal/models/portal-dashboard.models';
import {
  CreateLeavePolicyRequest,
  CreateWorkflowDefinitionRequest,
  LeavePolicyItemDto,
  UpdateLeavePolicyRequest,
  UpdateWorkflowDefinitionRequest,
  WorkflowDefinitionDto,
} from '../models/workflow.models';

@Injectable({ providedIn: 'root' })
export class WorkflowsService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getPendingApprovals(): Observable<ApprovalRequestDto[]> {
    return this.http.get<ApprovalRequestDto[]>(`${this.appConfig.apiBaseUrl}/approvals/pending`);
  }

  getApprovalHistory(): Observable<ApprovalRequestDto[]> {
    return this.http.get<ApprovalRequestDto[]>(`${this.appConfig.apiBaseUrl}/approvals/history`);
  }

  decideApproval(requestId: string, request: DecideApprovalRequest): Observable<ApprovalRequestDto> {
    return this.http.post<ApprovalRequestDto>(
      `${this.appConfig.apiBaseUrl}/approvals/${requestId}/decide`,
      request,
    );
  }

  getLeavePolicies(): Observable<LeavePolicyItemDto[]> {
    return this.http.get<LeavePolicyItemDto[]>(`${this.appConfig.apiBaseUrl}/leaves/policies`);
  }

  createLeavePolicy(request: CreateLeavePolicyRequest): Observable<LeavePolicyItemDto> {
    return this.http.post<LeavePolicyItemDto>(`${this.appConfig.apiBaseUrl}/leaves/policies`, request);
  }

  updateLeavePolicy(id: string, request: UpdateLeavePolicyRequest): Observable<LeavePolicyItemDto> {
    return this.http.put<LeavePolicyItemDto>(`${this.appConfig.apiBaseUrl}/leaves/policies/${id}`, request);
  }

  getApprovalRoutes(): Observable<WorkflowDefinitionDto[]> {
    return this.http.get<WorkflowDefinitionDto[]>(`${this.appConfig.apiBaseUrl}/approvals/routes`);
  }

  createApprovalRoute(request: CreateWorkflowDefinitionRequest): Observable<WorkflowDefinitionDto> {
    return this.http.post<WorkflowDefinitionDto>(`${this.appConfig.apiBaseUrl}/approvals/routes`, request);
  }

  updateApprovalRoute(id: string, request: UpdateWorkflowDefinitionRequest): Observable<WorkflowDefinitionDto> {
    return this.http.put<WorkflowDefinitionDto>(
      `${this.appConfig.apiBaseUrl}/approvals/routes/${id}`,
      request,
    );
  }
}
