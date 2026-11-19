import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { apiToggleInterceptor } from '../../../../core/http/api-toggle.interceptor';
import { ApiToggleService } from '../../../../core/http/api-toggle.service';
import { AttendancePageComponent } from './attendance-page.component';

describe('AttendancePageComponent', () => {
  let httpMock: HttpTestingController;
  let apiToggleService: ApiToggleService;

  beforeEach(async () => {
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [AttendancePageComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([apiToggleInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    apiToggleService = TestBed.inject(ApiToggleService);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('loads attendance history from /api/v1/attendance/my-history and renders rows', () => {
    const fixture = TestBed.createComponent(AttendancePageComponent);
    fixture.detectChanges();

    const req = httpMock.expectOne((r) => r.url === '/api/v1/attendance/my-history');
    expect(req.request.method).toBe('GET');

    req.flush([
      {
        id: 'sum-1',
        employeeId: 'e-1',
        employeeCode: 'EMP-0001',
        employeeName: 'สมชาย ใจดี',
        branchId: 'b-1',
        departmentId: 'd-1',
        workDate: '2026-11-19',
        shiftDefinitionId: 's-1',
        shiftName: 'กะเช้า 09:00-18:00',
        actualInUtc: '2026-11-19T02:05:00Z',
        actualOutUtc: '2026-11-19T11:00:00Z',
        workedMinutes: 480,
        lateMinutes: 5,
        earlyOutMinutes: 0,
        otMinutes: 0,
        attendanceStatus: 'LATE',
        isLockedByPayroll: false,
      },
    ]);

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('กะเช้า 09:00-18:00');
    expect(compiled.textContent).toContain('มาสาย');
  });

  it('enters Error State with zero fallback data when API toggle is OFF', () => {
    apiToggleService.toggleApi(); // API: OFF

    const fixture = TestBed.createComponent(AttendancePageComponent);
    fixture.detectChanges();

    httpMock.expectNone('/api/v1/attendance/my-history');
    fixture.detectChanges();

    const component = fixture.componentInstance;
    expect(component.records().length).toBe(0);
    expect(component.errorMessage()).toContain('ระบบจำลองสถานะขาดการเชื่อมต่อ API');

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('ไม่สามารถโหลดประวัติการลงเวลาได้');
  });
});
