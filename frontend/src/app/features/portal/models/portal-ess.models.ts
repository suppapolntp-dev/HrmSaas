export interface SubmitLeaveRequest {
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  daysCount: number;
  reason: string;
}

export interface SubmitOvertimeRequest {
  workDate: string;
  startTime: string;
  endTime: string;
  hours: number;
  reason: string;
}

export interface PayslipItemDto {
  id: string;
  salaryComponentId: string;
  componentName: string;
  amount: number;
  units: number | null;
}

export interface PayslipDto {
  id: string;
  payrollPeriodId: string;
  periodCode: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  grossIncome: number;
  totalDeductions: number;
  ssoEmployeeAmount: number;
  withholdingTaxAmount: number;
  netPay: number;
  isLocked: boolean;
  items: PayslipItemDto[];
}
