export interface LeavePolicyItemDto {
  id: string;
  leaveTypeId: string;
  leaveTypeCode: string;
  leaveTypeName: string;
  isPaid: boolean;
  allowHalfDay: boolean;
  allowHourly: boolean;
  requiresAttachment: boolean;
  jobGradeId: string | null;
  minTenureMonths: number;
  maxTenureMonths: number | null;
  annualQuotaDays: number;
  prorateRule: string;
  maxCarryOverDays: number;
}

export interface CreateLeavePolicyRequest {
  leaveTypeCode: string;
  leaveTypeName: string;
  isPaid: boolean;
  allowHalfDay: boolean;
  allowHourly: boolean;
  requiresAttachment: boolean;
  jobGradeId: string | null;
  minTenureMonths: number;
  maxTenureMonths: number | null;
  annualQuotaDays: number;
  prorateRule: string;
  maxCarryOverDays: number;
}

export interface UpdateLeavePolicyRequest {
  minTenureMonths: number;
  maxTenureMonths: number | null;
  annualQuotaDays: number;
  prorateRule: string;
  maxCarryOverDays: number;
}

export interface WorkflowStepDto {
  id: string;
  stepOrder: number;
  approverStrategy: string;
  specificRoleId: string | null;
  specificRoleName: string | null;
  specificEmployeeId: string | null;
  specificEmployeeName: string | null;
}

export interface WorkflowDefinitionDto {
  id: string;
  requestType: string;
  departmentId: string | null;
  departmentName: string | null;
  name: string;
  isActive: boolean;
  steps: WorkflowStepDto[];
}

export interface WorkflowStepInput {
  stepOrder: number;
  approverStrategy: string;
  specificRoleId: string | null;
  specificEmployeeId: string | null;
}

export interface CreateWorkflowDefinitionRequest {
  requestType: string;
  departmentId: string | null;
  name: string;
  isActive: boolean;
  steps: WorkflowStepInput[];
}

export interface UpdateWorkflowDefinitionRequest {
  departmentId: string | null;
  name: string;
  isActive: boolean;
  steps: WorkflowStepInput[];
}

export function mapApproverStrategyLabel(strategy: string): string {
  switch (strategy) {
    case 'DIRECT_MANAGER':
      return 'หัวหน้างานสายตรง';
    case 'SECONDARY_MANAGER':
      return 'ผู้บังคับบัญชาระดับรอง';
    case 'HR_ROLE':
      return 'ฝ่ายบุคคล (HR)';
    case 'SPECIFIC_ROLE':
      return 'ตามบทบาท (Role)';
    case 'SPECIFIC_EMPLOYEE':
      return 'ระบุบุคคล';
    default:
      return strategy;
  }
}
