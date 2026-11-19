# Work Log — Dashboard Application Service

- **Component / Module:** `DashboardService`, `DashboardDtos`
- **Path:** `backend/src/HrmSaas.Application/Features/Dashboard/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] สร้าง Service และ DTOs สำหรับ Dashboard (ESS + Team/Org)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ลงแรงทำหน้า Dashboard โดยอิง data จริงจาก PostgreSQL ไม่ mock | สร้าง `DashboardDtos.cs` (10 record DTOs) และ `DashboardService.cs` (`GetMySummaryAsync`, `GetTeamPulseAsync`, `GetHrOverviewAsync`, `GetPayrollReadinessAsync`) พร้อมลงทะเบียนใน `DependencyInjection.cs` | **Error แรกที่เจอ:** `CS1061` — `AttendanceDailySummary` ไม่มี `BranchId`/`DepartmentId` และ `DepartmentHeadcountDto` ชื่อ field เป็น `ActiveCount`/`ProbationCount` (ไม่ใช้ `Active`/`Probation`)<br>**วิธีแก้:** Join ตาราง `Employees` เพื่ออ่าน `e.BranchId`/`e.DepartmentId` และแก้ชื่อ property<br>**ผลลัพธ์สุดท้าย:** `dotnet build HrmSaas.sln` PASS (0 Errors) | เพิ่ม Application Service + DTOs (อ่านจากตารางเดิม: `employees`, `departments`, `attendance_daily_summaries`, `employee_shift_rosters`, `approval_requests`, `employment_contracts`, `payroll_periods`, `payslips`) | `docker compose up -d --build` เพื่ออัปเดต image ในคอนเทนเนอร์ |
