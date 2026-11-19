import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import {
  BranchDto,
  CompanyDto,
  CreateBranchRequest,
  CreateCompanyRequest,
  CreateDepartmentRequest,
  CreateJobGradeRequest,
  CreatePositionRequest,
  DepartmentDto,
  DepartmentTreeNodeDto,
  JobGradeDto,
  PositionDto,
  UpdateBranchGeofenceRequest,
  UpdateCompanyRequest,
  UpdateDepartmentRequest,
  UpdateJobGradeRequest,
  UpdatePositionRequest,
} from '../models/organization.models';

@Injectable({ providedIn: 'root' })
export class OrganizationService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getCompanies(): Observable<CompanyDto[]> {
    return this.http.get<CompanyDto[]>(`${this.appConfig.apiBaseUrl}/organizations/companies`);
  }

  createCompany(request: CreateCompanyRequest): Observable<CompanyDto> {
    return this.http.post<CompanyDto>(`${this.appConfig.apiBaseUrl}/organizations/companies`, request);
  }

  updateCompany(id: string, request: UpdateCompanyRequest): Observable<CompanyDto> {
    return this.http.put<CompanyDto>(`${this.appConfig.apiBaseUrl}/organizations/companies/${id}`, request);
  }

  getBranches(): Observable<BranchDto[]> {
    return this.http.get<BranchDto[]>(`${this.appConfig.apiBaseUrl}/organizations/branches`);
  }

  createBranch(request: CreateBranchRequest): Observable<BranchDto> {
    return this.http.post<BranchDto>(`${this.appConfig.apiBaseUrl}/organizations/branches`, request);
  }

  updateBranchGeofence(id: string, request: UpdateBranchGeofenceRequest): Observable<BranchDto> {
    return this.http.put<BranchDto>(`${this.appConfig.apiBaseUrl}/organizations/branches/${id}`, request);
  }

  getDepartments(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(`${this.appConfig.apiBaseUrl}/organizations/departments`);
  }

  getDepartmentTree(): Observable<DepartmentTreeNodeDto[]> {
    return this.http.get<DepartmentTreeNodeDto[]>(`${this.appConfig.apiBaseUrl}/organizations/departments/tree`);
  }

  createDepartment(request: CreateDepartmentRequest): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(`${this.appConfig.apiBaseUrl}/organizations/departments`, request);
  }

  updateDepartment(id: string, request: UpdateDepartmentRequest): Observable<DepartmentDto> {
    return this.http.put<DepartmentDto>(`${this.appConfig.apiBaseUrl}/organizations/departments/${id}`, request);
  }

  deleteDepartment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.appConfig.apiBaseUrl}/organizations/departments/${id}`);
  }

  getPositions(): Observable<PositionDto[]> {
    return this.http.get<PositionDto[]>(`${this.appConfig.apiBaseUrl}/organizations/positions`);
  }

  createPosition(request: CreatePositionRequest): Observable<PositionDto> {
    return this.http.post<PositionDto>(`${this.appConfig.apiBaseUrl}/organizations/positions`, request);
  }

  updatePosition(id: string, request: UpdatePositionRequest): Observable<PositionDto> {
    return this.http.put<PositionDto>(`${this.appConfig.apiBaseUrl}/organizations/positions/${id}`, request);
  }

  getJobGrades(): Observable<JobGradeDto[]> {
    return this.http.get<JobGradeDto[]>(`${this.appConfig.apiBaseUrl}/organizations/job-grades`);
  }

  createJobGrade(request: CreateJobGradeRequest): Observable<JobGradeDto> {
    return this.http.post<JobGradeDto>(`${this.appConfig.apiBaseUrl}/organizations/job-grades`, request);
  }

  updateJobGrade(id: string, request: UpdateJobGradeRequest): Observable<JobGradeDto> {
    return this.http.put<JobGradeDto>(`${this.appConfig.apiBaseUrl}/organizations/job-grades/${id}`, request);
  }
}
