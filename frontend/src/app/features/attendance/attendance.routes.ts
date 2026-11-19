import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { DailyAttendanceLogsPageComponent } from './pages/daily-logs/daily-logs-page.component';
import { RosterMatrixPageComponent } from './pages/roster-matrix/roster-matrix-page.component';
import { ShiftPatternsPageComponent } from './pages/shift-patterns/shift-patterns-page.component';

export const ATTENDANCE_ROUTES: Routes = [
  {
    path: 'shift-patterns',
    canMatch: [permissionGuard('attendance.shift.manage')],
    component: ShiftPatternsPageComponent,
    data: { title: 'รูปแบบกะงาน' },
  },
  {
    path: 'roster-matrix',
    canMatch: [permissionGuard('attendance.roster.manage')],
    component: RosterMatrixPageComponent,
    data: { title: 'ตารางกะ (Roster Matrix)' },
  },
  {
    path: 'daily-logs',
    canMatch: [permissionGuard('attendance.log.read')],
    component: DailyAttendanceLogsPageComponent,
    data: { title: 'บันทึกเวลาประจำวัน' },
  },
];
