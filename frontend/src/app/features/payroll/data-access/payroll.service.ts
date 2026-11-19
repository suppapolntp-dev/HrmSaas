import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { PayslipDto } from '../../portal/models/portal-ess.models';
import {
  CreatePayrollPeriodRequest,
  PayrollCalculationSummaryDto,
  PayrollPeriodDto,
} from '../models/payroll.models';

@Injectable({ providedIn: 'root' })
export class PayrollService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getPeriods(): Observable<PayrollPeriodDto[]> {
    return this.http.get<PayrollPeriodDto[]>(`${this.appConfig.apiBaseUrl}/payroll/periods`);
  }

  createPeriod(request: CreatePayrollPeriodRequest): Observable<PayrollPeriodDto> {
    return this.http.post<PayrollPeriodDto>(`${this.appConfig.apiBaseUrl}/payroll/periods`, request);
  }

  calculatePeriod(id: string): Observable<PayrollCalculationSummaryDto> {
    return this.http.post<PayrollCalculationSummaryDto>(
      `${this.appConfig.apiBaseUrl}/payroll/periods/${id}/calculate`,
      {},
    );
  }

  lockPeriod(id: string): Observable<PayrollPeriodDto> {
    return this.http.post<PayrollPeriodDto>(`${this.appConfig.apiBaseUrl}/payroll/periods/${id}/lock`, {});
  }

  getPayslips(payrollPeriodId?: string | null): Observable<PayslipDto[]> {
    let params = new HttpParams();
    if (payrollPeriodId && payrollPeriodId.trim().length > 0) {
      params = params.set('payrollPeriodId', payrollPeriodId.trim());
    }
    return this.http.get<PayslipDto[]>(`${this.appConfig.apiBaseUrl}/payroll/payslips`, { params });
  }
}
