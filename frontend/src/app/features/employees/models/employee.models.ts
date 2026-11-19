import { StatusTone } from '../../../shared/ui/status-indicator/status-indicator.component';

export interface EmployeeListQueryParams {
  page: number;
  pageSize: number;
  branchId?: string | null;
  departmentId?: string | null;
  status?: string | null;
  search?: string | null;
  sortBy?: string | null;
  sortDesc?: boolean;
}

export interface EmployeeListItemDto {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  branchId: string;
  branchName: string;
  departmentId: string;
  departmentName: string;
  positionId: string;
  positionTitle: string;
  jobGradeId: string;
  jobGradeName: string;
  hireDate: string;
  employmentStatus: string;
}

export interface EmployeeContractDto {
  id: string;
  contractType: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  baseSalary: number;
  paymentCycle: string;
  isActive: boolean;
}

export interface EmployeeReportingLineDto {
  id: string;
  managerEmployeeId: string;
  managerEmployeeCode: string;
  managerFullName: string;
  lineType: string;
  priorityOrder: number;
  effectiveFrom: string;
  effectiveTo: string | null;
}

export interface EmployeeLeaveBalanceSummaryDto {
  leaveTypeId: string;
  leaveTypeCode: string;
  leaveTypeName: string;
  quotaYear: number;
  carriedOverDays: number;
  entitledDays: number;
  usedDays: number;
  pendingDays: number;
  remainingDays: number;
}

export interface Employee360Dto {
  id: string;
  companyId: string;
  branchId: string;
  branchName: string;
  departmentId: string;
  departmentName: string;
  positionId: string;
  positionTitle: string;
  jobGradeId: string;
  jobGradeName: string;
  userId: string | null;
  employeeCode: string;
  firstName: string;
  lastName: string;
  hireDate: string;
  probationEndDate: string | null;
  employmentStatus: string;
  contracts: EmployeeContractDto[];
  reportingLines: EmployeeReportingLineDto[];
  leaveBalances: EmployeeLeaveBalanceSummaryDto[];
}

export interface OnboardEmployeeRequest {
  employeeCode: string;
  firstName: string;
  lastName: string;
  branchId: string;
  departmentId: string;
  positionId: string;
  jobGradeId: string;
  hireDate: string;
  probationEndDate: string | null;
  employmentStatus: string;
  contractType: string;
  baseSalary: number;
  paymentCycle: string;
  email: string | null;
  initialPassword: string | null;
  roleId: string | null;
}

export interface UpdateEmployeeRequest {
  branchId: string;
  departmentId: string;
  positionId: string;
  jobGradeId: string;
  firstName: string;
  lastName: string;
  employmentStatus: string;
}

export interface ReportingLineItemRequest {
  managerEmployeeId: string;
  lineType: string;
  priorityOrder: number;
  effectiveFrom: string;
  effectiveTo: string | null;
}

export interface UpdateReportingLinesRequest {
  lines: ReportingLineItemRequest[];
}

export interface CreateContractRequest {
  contractType: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  baseSalary: number;
  paymentCycle: string;
}

export function mapEmploymentStatusLabel(status: string): string {
  switch (status) {
    case 'ACTIVE':
      return 'ปฏิบัติงาน';
    case 'PROBATION':
      return 'ทดลองงาน';
    case 'RESIGNED':
      return 'ลาออก';
    case 'TERMINATED':
      return 'พ้นสภาพ';
    case 'SUSPENDED':
      return 'พักงาน';
    default:
      return status;
  }
}

export function mapEmploymentStatusTone(status: string): StatusTone {
  switch (status) {
    case 'ACTIVE':
      return 'success';
    case 'PROBATION':
      return 'info';
    case 'SUSPENDED':
      return 'warning';
    case 'RESIGNED':
    case 'TERMINATED':
      return 'danger';
    default:
      return 'neutral';
  }
}
