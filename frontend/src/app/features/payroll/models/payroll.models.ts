export interface PayrollPeriodDto {
  id: string;
  periodCode: string;
  periodStart: string;
  periodEnd: string;
  attendanceCutoffDate: string;
  payDate: string;
  status: string;
}

export interface CreatePayrollPeriodRequest {
  periodCode: string;
  periodStart: string;
  periodEnd: string;
  attendanceCutoffDate: string;
  payDate: string;
}

export interface PayrollCalculationSummaryDto {
  payrollPeriodId: string;
  periodCode: string;
  status: string;
  processedEmployees: number;
  totalGrossIncome: number;
  totalNetPay: number;
}
