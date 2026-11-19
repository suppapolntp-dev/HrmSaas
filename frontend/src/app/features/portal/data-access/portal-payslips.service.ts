import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { PayslipDto } from '../models/portal-ess.models';

@Injectable({ providedIn: 'root' })
export class PortalPayslipsService {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(APP_CONFIG);

  getMyPayslips(payrollPeriodId?: string): Observable<PayslipDto[]> {
    const url = `${this.appConfig.apiBaseUrl}/payroll/payslips`;
    let params = new HttpParams().set('mineOnly', 'true');
    if (payrollPeriodId && payrollPeriodId.trim().length > 0) {
      params = params.set('payrollPeriodId', payrollPeriodId.trim());
    }
    return this.http.get<PayslipDto[]>(url, { params });
  }
}
