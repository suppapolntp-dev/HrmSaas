using System.Net;
using System.Security.Cryptography;
using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Payroll;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private static readonly Guid DemoTenantId = Guid.Parse("01930000-0000-7000-8000-000000000001");
    private static readonly Guid DemoCompanyId = Guid.Parse("01930000-0000-7000-8000-000000000002");
    private static readonly Guid DemoBranchId = Guid.Parse("01930000-0000-7000-8000-000000000003");

    public async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // Step 1: Seed Global Master Permissions
        List<Permission> allPermissions = await SeedPermissionsAsync(dbContext, cancellationToken);

        // Step 2: Seed Global Statutory Tables (Thai Tax Brackets & SSO Configuration)
        await SeedStatutoryTablesAsync(dbContext, cancellationToken);

        // Step 3: Seed Demo Tenant, Company, and Branch
        bool hasTenant = await dbContext.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == DemoTenantId, cancellationToken);

        if (hasTenant)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var tenant = Tenant.Create(
            code: "TENANT-DEMO",
            name: "HrmSaas Demo Group Co., Ltd.",
            subscriptionPlan: "ENTERPRISE",
            status: TenantStatus.Active,
            id: DemoTenantId);
        dbContext.Tenants.Add(tenant);

        var company = Company.Create(
            tenantId: DemoTenantId,
            code: "COMP-HQ",
            legalName: "บริษัท เอชอาร์เอ็ม ซาส เดโม จำกัด",
            taxId: "0105569000123",
            ssoEmployerCode: "1000123456",
            baseCurrency: "THB",
            id: DemoCompanyId);
        dbContext.Companies.Add(company);

        var branch = Branch.Create(
            tenantId: DemoTenantId,
            companyId: DemoCompanyId,
            code: "BR-BKK",
            name: "สำนักงานใหญ่ กรุงเทพฯ",
            timezone: "Asia/Bangkok",
            latitude: 13.7563309m,
            longitude: 100.5017651m,
            geofenceRadiusMeters: 250,
            allowedIps: ["127.0.0.1", "::1"],
            id: DemoBranchId);
        var branchCnx = Branch.Create(
            tenantId: DemoTenantId,
            companyId: DemoCompanyId,
            code: "BR-CNX",
            name: "สาขาเชียงใหม่",
            timezone: "Asia/Bangkok",
            latitude: 18.7883439m,
            longitude: 98.9853008m,
            geofenceRadiusMeters: 200,
            allowedIps: ["127.0.0.1"]);
        var branchHkt = Branch.Create(
            tenantId: DemoTenantId,
            companyId: DemoCompanyId,
            code: "BR-HKT",
            name: "สาขาภูเก็ต",
            timezone: "Asia/Bangkok",
            latitude: 7.8804479m,
            longitude: 98.3922504m,
            geofenceRadiusMeters: 200,
            allowedIps: ["127.0.0.1"]);
        var branchCbi = Branch.Create(
            tenantId: DemoTenantId,
            companyId: DemoCompanyId,
            code: "BR-CBI",
            name: "สาขาชลบุรี",
            timezone: "Asia/Bangkok",
            latitude: 13.3611431m,
            longitude: 100.9846717m,
            geofenceRadiusMeters: 250,
            allowedIps: ["127.0.0.1"]);
        var branchKkn = Branch.Create(
            tenantId: DemoTenantId,
            companyId: DemoCompanyId,
            code: "BR-KKN",
            name: "สาขาขอนแก่น",
            timezone: "Asia/Bangkok",
            latitude: 16.4321938m,
            longitude: 102.8236214m,
            geofenceRadiusMeters: 200,
            allowedIps: ["127.0.0.1"]);
        dbContext.Branches.AddRange(branch, branchCnx, branchHkt, branchCbi, branchKkn);

        // Step 4: Seed Departments, Positions, and Job Grades
        var deptExec = Department.Create(DemoTenantId, DemoCompanyId, "DEPT-EXEC", "ผู้บริหารและปฏิบัติการ");
        var deptHr = Department.Create(DemoTenantId, DemoCompanyId, "DEPT-HR", "ฝ่ายทรัพยากรบุคคล", deptExec.Id);
        var deptAcc = Department.Create(DemoTenantId, DemoCompanyId, "DEPT-ACC", "ฝ่ายบัญชีและการเงิน", deptExec.Id);
        var deptIt = Department.Create(DemoTenantId, DemoCompanyId, "DEPT-IT", "ฝ่ายเทคโนโลยีสารสนเทศ", deptExec.Id);
        var deptOps = Department.Create(DemoTenantId, DemoCompanyId, "DEPT-OPS", "ฝ่ายปฏิบัติการสาขา", deptExec.Id);
        dbContext.Departments.AddRange(deptExec, deptHr, deptAcc, deptIt, deptOps);

        var posStaff = Position.Create(DemoTenantId, DemoCompanyId, "POS-STAFF", "เจ้าหน้าที่ประจำทีม");
        var posLead = Position.Create(DemoTenantId, DemoCompanyId, "POS-LEAD", "หัวหน้าทีมปฏิบัติการ");
        var posHr = Position.Create(DemoTenantId, DemoCompanyId, "POS-HR", "ผู้จัดการฝ่ายบุคคล");
        var posAcc = Position.Create(DemoTenantId, DemoCompanyId, "POS-ACC", "สมุห์บัญชี");
        var posIt = Position.Create(DemoTenantId, DemoCompanyId, "POS-IT", "วิศวกรระบบ");
        dbContext.Positions.AddRange(posStaff, posLead, posHr, posAcc, posIt);

        var grade1 = JobGrade.Create(DemoTenantId, DemoCompanyId, "JG-1", 1, "Staff");
        var grade2 = JobGrade.Create(DemoTenantId, DemoCompanyId, "JG-2", 2, "Senior / Lead");
        var grade3 = JobGrade.Create(DemoTenantId, DemoCompanyId, "JG-3", 3, "Manager / Admin");
        var grade4 = JobGrade.Create(DemoTenantId, DemoCompanyId, "JG-4", 4, "Department Director");
        var grade5 = JobGrade.Create(DemoTenantId, DemoCompanyId, "JG-5", 5, "Executive");
        dbContext.JobGrades.AddRange(grade1, grade2, grade3, grade4, grade5);

        // Step 5: Seed Default 6 Roles & RolePermissions
        Dictionary<string, Role> roles = SeedRolesAndPermissions(dbContext, DemoTenantId, DemoCompanyId, allPermissions);

        // Step 6: Seed Demo Users & Employees for All 6 Profiles
        List<Employee> seededEmployees = await SeedDemoUsersAndEmployeesAsync(
            dbContext,
            DemoTenantId,
            DemoCompanyId,
            DemoBranchId,
            deptOps,
            deptHr,
            deptAcc,
            deptIt,
            deptExec,
            posStaff,
            posLead,
            posHr,
            posAcc,
            posIt,
            grade1,
            grade2,
            grade3,
            roles,
            cancellationToken);

        // Step 7: Seed Shift Definitions, Rosters, Attendance, Leave, Workflows, and Payroll Demo Data (5 each)
        List<ShiftDefinition> seededShifts = SeedShiftDefinitions(dbContext, DemoTenantId, DemoCompanyId);
        SeedRostersAndAttendance(dbContext, DemoTenantId, DemoCompanyId, DemoBranchId, seededEmployees, seededShifts);
        SeedLeaveAndWorkflows(dbContext, DemoTenantId, DemoCompanyId, roles, seededEmployees, deptOps, deptIt);
        List<SalaryComponent> seededComponents = SeedSalaryComponents(dbContext, DemoTenantId, DemoCompanyId);
        SeedPayrollDemoData(dbContext, DemoTenantId, DemoCompanyId, seededEmployees, seededComponents);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<List<Permission>> SeedPermissionsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<Permission> existing = await dbContext.Permissions
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        var permissionSpecs = new (string Code, string Module, string Description)[]
        {
            ("portal.attendance.read", "PORTAL", "ดูประวัติการลงเวลาของตนเองและขอแก้เวลา"),
            ("portal.leave.read", "PORTAL", "ดูโควตาและยื่นใบลาของตนเอง"),
            ("portal.overtime.read", "PORTAL", "ยื่นและดูประวัติคำขอ OT ของตนเอง"),
            ("portal.payslip.read", "PORTAL", "ดูและดาวน์โหลดสลิปเงินเดือนของตนเอง"),
            ("org.branch.manage", "ORGANIZATION", "จัดการข้อมูลบริษัท สาขา พิกัด GPS และ IP Whitelist"),
            ("org.structure.manage", "ORGANIZATION", "จัดการโครงสร้างแผนก ตำแหน่ง และ Job Grade"),
            ("employee.read", "EMPLOYEE", "ดูรายชื่อและโปรไฟล์พนักงาน"),
            ("employee.create", "EMPLOYEE", "รับพนักงานใหม่ (Onboarding)"),
            ("employee.update", "EMPLOYEE", "แก้ไขข้อมูลพนักงาน สัญญาจ้าง และสายบังคับบัญชา"),
            ("attendance.shift.manage", "ATTENDANCE", "ตั้งค่ารูปแบบกะงานและเวลาพัก"),
            ("attendance.roster.manage", "ATTENDANCE", "จัดตารางกะ Roster Matrix"),
            ("attendance.log.read", "ATTENDANCE", "ตรวจสอบบันทึกเวลาเข้า-ออกงานรายวัน"),
            ("workflow.approve", "WORKFLOW", "อนุมัติหรือปฏิเสธคำขอใน Approval Inbox"),
            ("workflow.policy.manage", "WORKFLOW", "ตั้งค่าประเภทวันลาและนโยบายโควตา"),
            ("workflow.route.manage", "WORKFLOW", "ตั้งค่าสายอนุมัติ Multi-Step Workflow"),
            ("payroll.period.manage", "PAYROLL", "เปิดและจัดการงวดเงินเดือน"),
            ("payroll.calculate", "PAYROLL", "คำนวณเงินเดือน ภาษี และ Lock งวดเงินเดือน"),
            ("settings.rbac.manage", "SETTINGS", "จัดการ Role และ Permission Matrix"),
            ("settings.audit.read", "SETTINGS", "ตรวจสอบ Audit Logs")
        };

        var existingByCode = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);
        foreach (Permission item in existing)
        {
            existingByCode[item.Code] = item;
        }

        foreach (var spec in permissionSpecs)
        {
            if (!existingByCode.ContainsKey(spec.Code))
            {
                var created = Permission.Create(spec.Code, spec.Module, spec.Description);
                dbContext.Permissions.Add(created);
                existingByCode[created.Code] = created;
            }
        }

        return existingByCode.Values.ToList();
    }

    private static async Task SeedStatutoryTablesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        bool hasTaxBrackets = await dbContext.TaxBrackets.AnyAsync(
            x => x.CountryCode == "TH" && x.TaxYear == 2026,
            cancellationToken);

        if (!hasTaxBrackets)
        {
            dbContext.TaxBrackets.AddRange(
                TaxBracket.Create(2026, "TH", 0m, 150000m, 0m),
                TaxBracket.Create(2026, "TH", 150000.0001m, 300000m, 5m),
                TaxBracket.Create(2026, "TH", 300000.0001m, 500000m, 10m),
                TaxBracket.Create(2026, "TH", 500000.0001m, 750000m, 15m),
                TaxBracket.Create(2026, "TH", 750000.0001m, 1000000m, 20m),
                TaxBracket.Create(2026, "TH", 1000000.0001m, 2000000m, 25m),
                TaxBracket.Create(2026, "TH", 2000000.0001m, 5000000m, 30m),
                TaxBracket.Create(2026, "TH", 5000000.0001m, null, 35m));
        }

        bool hasSsoConfig = await dbContext.SsoConfigurations.AnyAsync(
            x => x.CountryCode == "TH",
            cancellationToken);

        if (!hasSsoConfig)
        {
            dbContext.SsoConfigurations.Add(
                SsoConfiguration.Create(
                    countryCode: "TH",
                    effectiveFrom: new DateOnly(2026, 1, 1),
                    effectiveTo: null,
                    employeeRatePercent: 5.0m,
                    employerRatePercent: 5.0m,
                    minWageBase: 1650m,
                    maxWageBase: 15000m));
        }
    }

    private static Dictionary<string, Role> SeedRolesAndPermissions(
        AppDbContext dbContext,
        Guid tenantId,
        Guid companyId,
        List<Permission> allPermissions)
    {
        var permissionMap = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);
        foreach (Permission permission in allPermissions)
        {
            permissionMap[permission.Code] = permission;
        }

        var roleConfigs = new (string Code, string Name, string[] PermissionCodes)[]
        {
            ("USER", "Employee (User)", [
                "portal.attendance.read", "portal.leave.read", "portal.overtime.read", "portal.payslip.read"
            ]),
            ("OPERATION", "Operation Manager", [
                "portal.attendance.read", "portal.leave.read", "portal.overtime.read", "portal.payslip.read",
                "employee.read", "attendance.shift.manage", "attendance.roster.manage", "attendance.log.read",
                "workflow.approve"
            ]),
            ("HR", "Human Resources", [
                "portal.attendance.read", "portal.leave.read", "portal.overtime.read", "portal.payslip.read",
                "org.structure.manage", "employee.read", "employee.create", "employee.update",
                "attendance.shift.manage", "attendance.roster.manage", "attendance.log.read",
                "workflow.approve", "workflow.policy.manage", "workflow.route.manage",
                "payroll.period.manage", "payroll.calculate"
            ]),
            ("ACCOUNTING", "Accounting & Payroll", [
                "portal.attendance.read", "portal.leave.read", "portal.overtime.read", "portal.payslip.read",
                "employee.read", "payroll.period.manage", "payroll.calculate", "settings.audit.read"
            ]),
            ("ADMIN", "Company Admin", [
                "portal.attendance.read", "portal.leave.read", "portal.overtime.read", "portal.payslip.read",
                "org.branch.manage", "org.structure.manage", "employee.read", "employee.create", "employee.update",
                "attendance.shift.manage", "workflow.approve", "workflow.policy.manage", "workflow.route.manage",
                "payroll.period.manage", "settings.rbac.manage", "settings.audit.read"
            ]),
            ("IT", "IT & Security Admin", [
                "portal.attendance.read", "portal.leave.read", "portal.overtime.read", "portal.payslip.read",
                "org.branch.manage", "workflow.route.manage", "settings.rbac.manage", "settings.audit.read"
            ])
        };

        var result = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);

        foreach (var cfg in roleConfigs)
        {
            var role = Role.Create(tenantId, companyId, cfg.Code, cfg.Name, isSystemRole: true);
            dbContext.Roles.Add(role);
            result[cfg.Code] = role;

            foreach (string permCode in cfg.PermissionCodes)
            {
                if (permissionMap.TryGetValue(permCode, out Permission? permission))
                {
                    dbContext.RolePermissions.Add(RolePermission.Create(role.Id, permission.Id));
                }
            }
        }

        return result;
    }

    private static Task<List<Employee>> SeedDemoUsersAndEmployeesAsync(
        AppDbContext dbContext,
        Guid tenantId,
        Guid companyId,
        Guid branchId,
        Department deptOps,
        Department deptHr,
        Department deptAcc,
        Department deptIt,
        Department deptExec,
        Position posStaff,
        Position posLead,
        Position posHr,
        Position posAcc,
        Position posIt,
        JobGrade grade1,
        JobGrade grade2,
        JobGrade grade3,
        Dictionary<string, Role> roles,
        CancellationToken cancellationToken)
    {
        string demoPasswordHash = HashPassword("Demo@123456!");
        var hireDate = new DateOnly(2025, 1, 15);

        var accounts = new (string Email, string RoleCode, string EmpCode, string FirstName, string LastName, Department Dept, Position Pos, JobGrade Grade, decimal Salary)[]
        {
            ("admin@demo.hrmsaas.local", "ADMIN", "EMP-0001", "สมชาย", "บริหารกิจ", deptExec, posLead, grade3, 95000m),
            ("hr@demo.hrmsaas.local", "HR", "EMP-0002", "พิมพ์ชนก", "รักษ์คน", deptHr, posHr, grade3, 65000m),
            ("accounting@demo.hrmsaas.local", "ACCOUNTING", "EMP-0003", "สุภาวดี", "ตรวจบัญชี", deptAcc, posAcc, grade2, 58000m),
            ("it@demo.hrmsaas.local", "IT", "EMP-0004", "ธนกร", "ดูแลระบบ", deptIt, posIt, grade2, 62000m),
            ("operation@demo.hrmsaas.local", "OPERATION", "EMP-0005", "กิตติศักดิ์", "คุมสาขา", deptOps, posLead, grade2, 48000m),
            ("user@demo.hrmsaas.local", "USER", "EMP-0006", "ณัฐพงษ์", "ตั้งใจงาน", deptOps, posStaff, grade1, 28000m)
        };

        var seededEmployees = new List<Employee>();
        var employeeByCode = new Dictionary<string, Employee>();

        foreach (var account in accounts)
        {
            var user = User.Create(tenantId, account.Email, demoPasswordHash);
            dbContext.Users.Add(user);

            Role role = roles[account.RoleCode];
            dbContext.UserCompanyRoles.Add(UserCompanyRole.Create(tenantId, user.Id, companyId, role.Id));

            var employee = Employee.Create(
                tenantId,
                companyId,
                branchId,
                account.Dept.Id,
                account.Pos.Id,
                account.Grade.Id,
                user.Id,
                account.EmpCode,
                account.FirstName,
                account.LastName,
                hireDate,
                hireDate.AddDays(119),
                EmploymentStatus.Active);
            dbContext.Employees.Add(employee);
            seededEmployees.Add(employee);
            employeeByCode[account.EmpCode] = employee;

            var contract = EmploymentContract.Create(
                tenantId,
                companyId,
                employee.Id,
                ContractType.FullTime,
                hireDate,
                null,
                account.Salary,
                PaymentCycle.Monthly,
                isActive: true);
            dbContext.EmploymentContracts.Add(contract);
        }

        // Reporting lines (5 lines): EMP-0002..EMP-0005 report to EMP-0001, EMP-0006 reports to EMP-0005
        if (employeeByCode.TryGetValue("EMP-0001", out Employee? topAdmin) &&
            employeeByCode.TryGetValue("EMP-0005", out Employee? opsManager) &&
            employeeByCode.TryGetValue("EMP-0006", out Employee? staff))
        {
            string[] directToAdminCodes = ["EMP-0002", "EMP-0003", "EMP-0004", "EMP-0005"];
            foreach (string code in directToAdminCodes)
            {
                if (employeeByCode.TryGetValue(code, out Employee? subordinate))
                {
                    dbContext.EmployeeReportingLines.Add(
                        EmployeeReportingLine.Create(
                            tenantId,
                            companyId,
                            subordinate.Id,
                            topAdmin.Id,
                            ReportingLineType.Direct,
                            1,
                            hireDate));
                }
            }

            dbContext.EmployeeReportingLines.Add(
                EmployeeReportingLine.Create(
                    tenantId,
                    companyId,
                    staff.Id,
                    opsManager.Id,
                    ReportingLineType.Direct,
                    1,
                    hireDate));
        }

        return Task.FromResult(seededEmployees);
    }

    private static List<ShiftDefinition> SeedShiftDefinitions(AppDbContext dbContext, Guid tenantId, Guid companyId)
    {
        var dayShift = ShiftDefinition.Create(
            tenantId,
            companyId,
            "SHIFT-DAY-09",
            "กะปกติ (09:00 - 18:00)",
            ShiftType.Normal,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            isOvernight: false,
            gracePeriodMinutes: 15,
            lateAfterMinutes: 15,
            standardWorkMinutes: 480);

        var nightShift = ShiftDefinition.Create(
            tenantId,
            companyId,
            "SHIFT-NIGHT-22",
            "กะดึกข้ามคืน (22:00 - 07:00)",
            ShiftType.Overnight,
            new TimeOnly(22, 0),
            new TimeOnly(7, 0),
            isOvernight: true,
            gracePeriodMinutes: 10,
            lateAfterMinutes: 10,
            standardWorkMinutes: 480);

        var earlyShift = ShiftDefinition.Create(
            tenantId,
            companyId,
            "SHIFT-EARLY-07",
            "กะเช้า (07:00 - 16:00)",
            ShiftType.Normal,
            new TimeOnly(7, 0),
            new TimeOnly(16, 0),
            isOvernight: false,
            gracePeriodMinutes: 10,
            lateAfterMinutes: 10,
            standardWorkMinutes: 480);

        var midShift = ShiftDefinition.Create(
            tenantId,
            companyId,
            "SHIFT-MID-13",
            "กะบ่าย (13:00 - 22:00)",
            ShiftType.Normal,
            new TimeOnly(13, 0),
            new TimeOnly(22, 0),
            isOvernight: false,
            gracePeriodMinutes: 15,
            lateAfterMinutes: 15,
            standardWorkMinutes: 480);

        var flexShift = ShiftDefinition.Create(
            tenantId,
            companyId,
            "SHIFT-FLEX-10",
            "กะยืดหยุ่น (10:00 - 19:00)",
            ShiftType.Flexible,
            new TimeOnly(10, 0),
            new TimeOnly(19, 0),
            isOvernight: false,
            gracePeriodMinutes: 60,
            lateAfterMinutes: 60,
            standardWorkMinutes: 480);

        var shifts = new List<ShiftDefinition> { dayShift, nightShift, earlyShift, midShift, flexShift };
        dbContext.ShiftDefinitions.AddRange(shifts);

        dbContext.ShiftBreakRules.AddRange(
            ShiftBreakRule.Create(tenantId, dayShift.Id, new TimeOnly(12, 0), new TimeOnly(13, 0), 60, isPaid: false),
            ShiftBreakRule.Create(tenantId, nightShift.Id, new TimeOnly(2, 0), new TimeOnly(3, 0), 60, isPaid: false),
            ShiftBreakRule.Create(tenantId, earlyShift.Id, new TimeOnly(11, 0), new TimeOnly(12, 0), 60, isPaid: false),
            ShiftBreakRule.Create(tenantId, midShift.Id, new TimeOnly(17, 0), new TimeOnly(18, 0), 60, isPaid: false),
            ShiftBreakRule.Create(tenantId, flexShift.Id, new TimeOnly(13, 0), new TimeOnly(14, 0), 60, isPaid: false));

        return shifts;
    }

    private static void SeedRostersAndAttendance(
        AppDbContext dbContext,
        Guid tenantId,
        Guid companyId,
        Guid branchId,
        List<Employee> employees,
        List<ShiftDefinition> shifts)
    {
        if (employees.Count == 0 || shifts.Count == 0)
        {
            return;
        }

        ShiftDefinition defaultShift = shifts[0];
        var workDate = new DateOnly(2026, 11, 16);
        int targetCount = Math.Min(5, employees.Count);

        for (int i = 0; i < targetCount; i++)
        {
            Employee emp = employees[i];
            ShiftDefinition shift = shifts[i % shifts.Count];

            dbContext.EmployeeShiftRosters.Add(
                EmployeeShiftRoster.Create(tenantId, companyId, emp.Id, shift.Id, workDate, RosterDayType.Workday));

            var checkInUtc = new DateTimeOffset(2026, 11, 16, 2, i * 5, 0, TimeSpan.Zero); // 09:00..09:20 BKK
            var checkOutUtc = new DateTimeOffset(2026, 11, 16, 11, 0, 0, TimeSpan.Zero); // 18:00 BKK

            dbContext.AttendanceLogs.Add(
                AttendanceLog.Create(
                    tenantId,
                    companyId,
                    emp.Id,
                    branchId,
                    checkInUtc,
                    PunchType.CheckIn,
                    PunchSource.Web,
                    13.7563309m,
                    100.5017651m,
                    IPAddress.Loopback,
                    isWithinGeofence: true));

            int lateMinutes = i == 4 ? 20 : 0;
            string status = i == 4 ? AttendanceStatus.Late : AttendanceStatus.Present;

            dbContext.AttendanceDailySummaries.Add(
                AttendanceDailySummary.Create(
                    tenantId,
                    companyId,
                    emp.Id,
                    workDate,
                    defaultShift.Id,
                    checkInUtc,
                    checkOutUtc,
                    workedMinutes: 480 - lateMinutes,
                    lateMinutes: lateMinutes,
                    earlyOutMinutes: 0,
                    otMinutes: i == 0 ? 60 : 0,
                    attendanceStatus: status));
        }
    }

    private static void SeedLeaveAndWorkflows(
        AppDbContext dbContext,
        Guid tenantId,
        Guid companyId,
        Dictionary<string, Role> roles,
        List<Employee> employees,
        Department deptOps,
        Department deptIt)
    {
        var annualLeave = LeaveType.Create(tenantId, companyId, "ANNUAL", "ลาพักร้อนประจำปี", true, true, false, false);
        var sickLeave = LeaveType.Create(tenantId, companyId, "SICK", "ลาป่วย", true, true, true, true);
        var personalLeave = LeaveType.Create(tenantId, companyId, "PERSONAL", "ลากิจ", true, true, true, false);
        var maternityLeave = LeaveType.Create(tenantId, companyId, "MATERNITY", "ลาคลอดบุตร", true, false, false, true);
        var ordinationLeave = LeaveType.Create(tenantId, companyId, "ORDINATION", "ลาอุปสมบท", true, false, false, true);
        dbContext.LeaveTypes.AddRange(annualLeave, sickLeave, personalLeave, maternityLeave, ordinationLeave);

        dbContext.LeaveEntitlementPolicies.AddRange(
            LeaveEntitlementPolicy.Create(tenantId, companyId, annualLeave.Id, null, 0, 35, 6.0m, ProrateRule.MonthlyProrate, 3.0m),
            LeaveEntitlementPolicy.Create(tenantId, companyId, annualLeave.Id, null, 36, null, 10.0m, ProrateRule.None, 5.0m),
            LeaveEntitlementPolicy.Create(tenantId, companyId, sickLeave.Id, null, 0, null, 30.0m, ProrateRule.None, 0m),
            LeaveEntitlementPolicy.Create(tenantId, companyId, personalLeave.Id, null, 0, null, 6.0m, ProrateRule.MonthlyProrate, 0m),
            LeaveEntitlementPolicy.Create(tenantId, companyId, maternityLeave.Id, null, 0, null, 98.0m, ProrateRule.None, 0m),
            LeaveEntitlementPolicy.Create(tenantId, companyId, ordinationLeave.Id, null, 12, null, 15.0m, ProrateRule.None, 0m));

        int balanceCount = Math.Min(5, employees.Count);
        for (int i = 0; i < balanceCount; i++)
        {
            dbContext.EmployeeLeaveBalances.Add(
                EmployeeLeaveBalance.Create(
                    tenantId,
                    companyId,
                    employees[i].Id,
                    annualLeave.Id,
                    quotaYear: 2026,
                    carriedOverDays: 2.0m,
                    entitledDays: 6.0m,
                    usedDays: i * 0.5m,
                    pendingDays: 0m));
        }

        var leaveFlow = WorkflowDefinition.Create(tenantId, companyId, WorkflowRequestType.Leave, null, "สายอนุมัติใบลามาตรฐาน (หัวหน้า -> HR)");
        var otFlow = WorkflowDefinition.Create(tenantId, companyId, WorkflowRequestType.Overtime, null, "สายอนุมัติ OT มาตรฐาน");
        var adjustFlow = WorkflowDefinition.Create(tenantId, companyId, WorkflowRequestType.TimeAdjustment, null, "สายอนุมัติขอแก้ไขเวลา");
        var opsLeaveFlow = WorkflowDefinition.Create(tenantId, companyId, WorkflowRequestType.Leave, deptOps.Id, "สายอนุมัติใบลาฝ่ายปฏิบัติการ");
        var itOtFlow = WorkflowDefinition.Create(tenantId, companyId, WorkflowRequestType.Overtime, deptIt.Id, "สายอนุมัติ OT ฝ่ายไอที");
        dbContext.WorkflowDefinitions.AddRange(leaveFlow, otFlow, adjustFlow, opsLeaveFlow, itOtFlow);

        Role hrRole = roles["HR"];

        dbContext.WorkflowSteps.AddRange(
            WorkflowStep.Create(tenantId, leaveFlow.Id, 1, ApproverStrategy.DirectManager),
            WorkflowStep.Create(tenantId, leaveFlow.Id, 2, ApproverStrategy.SpecificRole, specificRoleId: hrRole.Id),
            WorkflowStep.Create(tenantId, otFlow.Id, 1, ApproverStrategy.DirectManager),
            WorkflowStep.Create(tenantId, adjustFlow.Id, 1, ApproverStrategy.DirectManager),
            WorkflowStep.Create(tenantId, opsLeaveFlow.Id, 1, ApproverStrategy.DirectManager),
            WorkflowStep.Create(tenantId, itOtFlow.Id, 1, ApproverStrategy.DirectManager));

        if (employees.Count >= 5)
        {
            Employee actor = employees[0];
            var now = new DateTimeOffset(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

            for (int i = 0; i < 5; i++)
            {
                string reqType = i % 3 == 0
                    ? WorkflowRequestType.Leave
                    : i % 3 == 1
                        ? WorkflowRequestType.Overtime
                        : WorkflowRequestType.TimeAdjustment;

                WorkflowDefinition wf = reqType == WorkflowRequestType.Leave
                    ? leaveFlow
                    : reqType == WorkflowRequestType.Overtime
                        ? otFlow
                        : adjustFlow;

                var req = ApprovalRequest.Create(
                    tenantId,
                    companyId,
                    employees[i].Id,
                    reqType,
                    Guid.NewGuid(),
                    wf.Id,
                    $"{{\"note\":\"Demo request #{i + 1}\"}}",
                    now.AddHours(i));

                if (i < 2)
                {
                    req.AdvanceApproval(1, now.AddHours(i + 1));
                }

                dbContext.ApprovalRequests.Add(req);
                dbContext.ApprovalActionLogs.Add(
                    ApprovalActionLog.Create(
                        tenantId,
                        req.Id,
                        stepOrder: 1,
                        actor.Id,
                        ApprovalActionType.Approve,
                        $"Demo action log #{i + 1}",
                        now.AddHours(i + 1)));
            }
        }
    }

    private static List<SalaryComponent> SeedSalaryComponents(AppDbContext dbContext, Guid tenantId, Guid companyId)
    {
        var baseSalary = SalaryComponent.Create(tenantId, companyId, "BASE_SALARY", "เงินเดือนพื้นฐาน", SalaryComponentType.Earning, true, true, CalculationFormulaType.Fixed);
        var ot15 = SalaryComponent.Create(tenantId, companyId, "OT_1_5X", "ค่าล่วงเวลา (OT 1.5x)", SalaryComponentType.Earning, true, false, CalculationFormulaType.OvertimeHourly);
        var posAllowance = SalaryComponent.Create(tenantId, companyId, "POSITION_ALLOWANCE", "ค่าตำแหน่ง", SalaryComponentType.Earning, true, true, CalculationFormulaType.Fixed);
        var bonus = SalaryComponent.Create(tenantId, companyId, "BONUS", "โบนัสประจำปี", SalaryComponentType.Earning, true, false, CalculationFormulaType.Fixed);
        var lateDeduction = SalaryComponent.Create(tenantId, companyId, "LATE_DEDUCTION", "หักมาสาย", SalaryComponentType.Deduction, false, false, CalculationFormulaType.AttendanceDeduction);

        var components = new List<SalaryComponent> { baseSalary, ot15, posAllowance, bonus, lateDeduction };
        dbContext.SalaryComponents.AddRange(components);
        return components;
    }

    private static void SeedPayrollDemoData(
        AppDbContext dbContext,
        Guid tenantId,
        Guid companyId,
        List<Employee> employees,
        List<SalaryComponent> components)
    {
        var periods = new List<PayrollPeriod>();
        for (int month = 7; month <= 11; month++)
        {
            var start = new DateOnly(2026, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var cutoff = end.AddDays(-5);
            var payDate = end;

            var period = PayrollPeriod.Create(
                tenantId,
                companyId,
                $"2026-{month:D2}",
                start,
                end,
                cutoff,
                payDate);

            if (month < 11)
            {
                period.MarkCalculated(DateTimeOffset.UtcNow);
            }

            periods.Add(period);
        }

        dbContext.PayrollPeriods.AddRange(periods);

        if (employees.Count == 0 || components.Count == 0)
        {
            return;
        }

        PayrollPeriod targetPeriod = periods[3]; // 2026-10 (Calculated)
        SalaryComponent baseComp = components[0];

        int payslipCount = Math.Min(5, employees.Count);
        for (int i = 0; i < payslipCount; i++)
        {
            decimal gross = 30000m + (i * 10000m);
            decimal sso = 750m;
            decimal tax = i * 500m;
            decimal totalDeductions = sso + tax;
            decimal net = gross - totalDeductions;

            var payslip = Payslip.Create(
                tenantId,
                companyId,
                targetPeriod.Id,
                employees[i].Id,
                gross,
                totalDeductions,
                sso,
                tax,
                net,
                isLocked: false);

            dbContext.Payslips.Add(payslip);
            dbContext.PayslipItems.Add(
                PayslipItem.Create(
                    tenantId,
                    payslip.Id,
                    baseComp.Id,
                    baseComp.Name,
                    gross,
                    units: 1m));
        }
    }

    private static string HashPassword(string plainPassword)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return $"PBKDF2$100000${Convert.ToBase64String(salt)}${Convert.ToBase64String(subkey)}";
    }
}
