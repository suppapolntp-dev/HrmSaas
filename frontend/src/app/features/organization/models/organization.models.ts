export interface CompanyDto {
  id: string;
  code: string;
  legalName: string;
  taxId: string;
  ssoEmployerCode: string;
  baseCurrency: string;
}

export interface CreateCompanyRequest {
  code: string;
  legalName: string;
  taxId: string;
  ssoEmployerCode: string;
  baseCurrency: string;
}

export interface UpdateCompanyRequest {
  legalName: string;
  taxId: string;
  ssoEmployerCode: string;
  baseCurrency: string;
}

export interface BranchDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  timezone: string;
  latitude: number;
  longitude: number;
  geofenceRadiusMeters: number;
  allowedIps: string[];
}

export interface CreateBranchRequest {
  code: string;
  name: string;
  timezone: string;
  latitude: number;
  longitude: number;
  geofenceRadiusMeters: number;
  allowedIps: string[];
}

export interface UpdateBranchGeofenceRequest {
  latitude: number;
  longitude: number;
  geofenceRadiusMeters: number;
  allowedIps: string[];
}

export interface DepartmentDto {
  id: string;
  companyId: string;
  parentDepartmentId: string | null;
  code: string;
  name: string;
  headEmployeeId: string | null;
}

export interface DepartmentTreeNodeDto {
  id: string;
  code: string;
  name: string;
  headEmployeeId: string | null;
  children: DepartmentTreeNodeDto[];
}

export interface CreateDepartmentRequest {
  code: string;
  name: string;
  parentDepartmentId: string | null;
  headEmployeeId: string | null;
}

export interface UpdateDepartmentRequest {
  name: string;
  parentDepartmentId: string | null;
  headEmployeeId: string | null;
}

export interface PositionDto {
  id: string;
  companyId: string;
  code: string;
  title: string;
}

export interface CreatePositionRequest {
  code: string;
  title: string;
}

export interface UpdatePositionRequest {
  title: string;
}

export interface JobGradeDto {
  id: string;
  companyId: string;
  code: string;
  levelOrder: number;
  name: string;
}

export interface CreateJobGradeRequest {
  code: string;
  levelOrder: number;
  name: string;
}

export interface UpdateJobGradeRequest {
  levelOrder: number;
  name: string;
}
