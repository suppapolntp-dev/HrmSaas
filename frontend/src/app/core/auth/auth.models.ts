export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface SwitchContextRequest {
  companyId: string;
  branchId: string | null;
}

export interface AccessibleBranchDto {
  branchId: string;
  code: string;
  name: string;
}

export interface AccessibleCompanyDto {
  companyId: string;
  code: string;
  legalName: string;
  branches: AccessibleBranchDto[];
}

export interface AuthMeResponse {
  userId: string;
  tenantId: string;
  email: string;
  employeeId: string | null;
  employeeCode: string | null;
  fullName: string | null;
  activeCompanyId: string | null;
  activeBranchId: string | null;
  permissions: string[];
  companies: AccessibleCompanyDto[];
}
