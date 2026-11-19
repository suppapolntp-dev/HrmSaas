export type NavIconKey =
  | 'dashboard'
  | 'clock'
  | 'calendar-off'
  | 'bolt'
  | 'receipt'
  | 'building'
  | 'sitemap'
  | 'users'
  | 'user-plus'
  | 'inbox'
  | 'clipboard-check'
  | 'grid'
  | 'layers'
  | 'file-text'
  | 'git-branch'
  | 'wallet'
  | 'shield'
  | 'history';

export interface NavItem {
  label: string;
  route: string;
  icon: NavIconKey;
  permission?: string;
}

export interface NavGroup {
  groupLabel: string;
  items: readonly NavItem[];
}

export const SIDEBAR_NAV_GROUPS: readonly NavGroup[] = [
  {
    groupLabel: 'พื้นที่พนักงาน (ESS)',
    items: [
      { label: 'แดชบอร์ด', route: '/portal/dashboard', icon: 'dashboard' },
      { label: 'เวลาทำงานของฉัน', route: '/portal/attendance', icon: 'clock', permission: 'portal.attendance.read' },
      { label: 'การลาของฉัน', route: '/portal/leaves', icon: 'calendar-off', permission: 'portal.leave.read' },
      { label: 'คำขอ OT ของฉัน', route: '/portal/overtime', icon: 'bolt', permission: 'portal.overtime.read' },
      { label: 'สลิปเงินเดือน', route: '/portal/payslips', icon: 'receipt', permission: 'portal.payslip.read' },
    ],
  },
  {
    groupLabel: 'องค์กรและพนักงาน',
    items: [
      { label: 'บริษัทและสาขา', route: '/org/companies-branches', icon: 'building', permission: 'org.branch.manage' },
      { label: 'แผนกและตำแหน่ง', route: '/org/departments-positions', icon: 'sitemap', permission: 'org.structure.manage' },
      { label: 'รายชื่อพนักงาน', route: '/employees/list', icon: 'users', permission: 'employee.read' },
      { label: 'รับพนักงานใหม่', route: '/employees/onboarding', icon: 'user-plus', permission: 'employee.create' },
    ],
  },
  {
    groupLabel: 'เวลาและสายอนุมัติ',
    items: [
      { label: 'กล่องอนุมัติคำขอ', route: '/workflows/inbox', icon: 'inbox', permission: 'workflow.approve' },
      { label: 'บันทึกเวลาประจำวัน', route: '/attendance/daily-logs', icon: 'clipboard-check', permission: 'attendance.log.read' },
      { label: 'ตารางกะ (Roster)', route: '/attendance/roster-matrix', icon: 'grid', permission: 'attendance.roster.manage' },
      { label: 'รูปแบบกะงาน', route: '/attendance/shift-patterns', icon: 'layers', permission: 'attendance.shift.manage' },
      { label: 'นโยบายวันลา', route: '/workflows/leave-policies', icon: 'file-text', permission: 'workflow.policy.manage' },
      { label: 'ตั้งค่าสายอนุมัติ', route: '/workflows/approval-routes', icon: 'git-branch', permission: 'workflow.route.manage' },
    ],
  },
  {
    groupLabel: 'เงินเดือนและระบบ',
    items: [
      { label: 'งวดเงินเดือน', route: '/payroll/periods', icon: 'wallet', permission: 'payroll.period.manage' },
      { label: 'บทบาทและสิทธิ์ (RBAC)', route: '/settings/roles-permissions', icon: 'shield', permission: 'settings.rbac.manage' },
      { label: 'ประวัติการใช้งาน (Audit)', route: '/settings/audit-logs', icon: 'history', permission: 'settings.audit.read' },
    ],
  },
];
