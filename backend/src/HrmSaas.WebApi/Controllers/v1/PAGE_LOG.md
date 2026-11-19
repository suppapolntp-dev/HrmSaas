# Work Log — WebApi v1 Controllers

- **Component / Module:** `DashboardController`
- **Path:** `backend/src/HrmSaas.WebApi/Controllers/v1/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] เปิด Endpoint เสริม 6 เส้น + `mineOnly` สำหรับ 18 หน้า Frontend

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | เพิ่ม `GET /api/v1/leaves/my-requests`, `GET /api/v1/overtime/my-requests`, `GET /api/v1/approvals/history`, `GET/POST/PUT /api/v1/approvals/routes` และพารามิเตอร์ `mineOnly` ใน `PayrollController.GetPayslips` | `dotnet build HrmSaas.sln` PASS (0 Errors) | แก้ `LeavesController.cs`, `OvertimeController.cs`, `ApprovalsController.cs`, `PayrollController.cs` (ไม่แก้ DB Schema) | ทดสอบ Runtime ผ่าน HTTP จริง |

## [2026-11-20] เพิ่ม Endpoint `GET /api/v1/attendance/my-history` ใน `AttendanceController`

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| งั้นทำหน้าต่อไปก่อนแล้วกกัน (`/portal/my-attendance`) | เพิ่ม `[HttpGet("my-history")]` ใน `AttendanceController.cs` คุมสิทธิ์ด้วย `[RequirePermission("portal.attendance.read")]` สำหรับหน้า `/portal/my-attendance` | `dotnet build HrmSaas.sln` PASS (0 Errors) | เพิ่ม Action `GetMyHistory` ใน `AttendanceController.cs` (ไม่แก้ DB Schema) | ทดสอบยิงผ่าน HTTP จริงพร้อม Frontend |

## [2026-11-19] เพิ่ม DashboardController (4 Endpoints)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ทำ UI หน้า Dashboard ต่อ API จริง | สร้าง `DashboardController.cs` เปิด 4 Endpoints: `GET /api/v1/dashboard/my-summary`, `team-pulse`, `hr-overview`, `payroll-readiness` คุมสิทธิ์ด้วย `[Authorize]` และ `[RequirePermission]` | `dotnet build HrmSaas.sln` PASS (0 Errors) | เพิ่ม `DashboardController.cs` (ไม่แก้ DB Schema) | รัน `docker compose up -d --build` และทดสอบยิงผ่าน HTTP จริง |
