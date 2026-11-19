# Work Log — Attendance Application Service

- **Component / Module:** `AttendanceService`
- **Path:** `backend/src/HrmSaas.Application/Features/Attendance/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] เพิ่ม `GetMyAttendanceHistoryAsync` สำหรับหน้า `/portal/my-attendance`

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| งั้นทำหน้าต่อไปก่อนแล้วกกัน (`/portal/my-attendance`) | เพิ่ม `GetMyAttendanceHistoryAsync(startDate, endDate, status)` ใน `AttendanceService.cs` ดึง `AttendanceDailySummaries` เฉพาะพนักงานปัจจุบัน (Default ย้อนหลัง 30 วันตาม Branch Timezone) และ reuse `BuildSummaryDto` | รอบแรก `dotnet build` เจอ `CS0246: DomainValidationException` เปลี่ยนเป็น `ValidationAppException` แล้ว `dotnet build` PASS (0 Errors) | เพิ่ม Method ใน `AttendanceService.cs` (ไม่แก้ DB Schema) | ทดสอบ Query จริงกับ PostgreSQL ผ่าน Docker |
