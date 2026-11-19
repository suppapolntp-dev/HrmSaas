# Work Log — AttendancePageComponent (`/portal/attendance`)

- **Component / Module:** `AttendancePageComponent`
- **Path:** `frontend/src/app/features/portal/pages/attendance/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] ย้ายจาก `my-attendance` เป็น `attendance` และเพิ่มปุ่มล้างตัวกรอง

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ทำหน้า my-attendance ให้เสร็จและแก้ my- ออกนะพี่ | เปลี่ยนชื่อโฟลเดอร์ `pages/my-attendance/` เป็น `pages/attendance/` (`AttendancePageComponent`), เปลี่ยน `MyAttendanceService` เป็น `PortalAttendanceService`, เพิ่มปุ่ม `ล้างตัวกรอง` ใน Filter bar, และเปลี่ยน Route `/portal/my-*` เป็น `/portal/{attendance,leaves,overtime,payslips}` ใน `portal.routes.ts`, `navigation.config.ts`, `portal-dashboard-page.component.html`, และ Portal UI cards | `ng build` PASS (0 CSS budget warn สำหรับหน้านี้) และ `ng test` 9 files, 15 passed | ไม่มี — ใช้ API เดิม `GET /api/v1/attendance/my-history` และ `POST /api/v1/attendance/time-adjustments` | ทดสอบเปิด `/portal/attendance` บนเบราว์เซอร์จริงร่วมกับ Docker Backend/Postgres |
