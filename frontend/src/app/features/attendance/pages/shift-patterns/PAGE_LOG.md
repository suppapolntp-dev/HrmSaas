# Work Log — ShiftPatternsPageComponent (`/attendance/shift-patterns`)

- **Component / Module:** `ShiftPatternsPageComponent`
- **Path:** `frontend/src/app/features/attendance/pages/shift-patterns/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้ารูปแบบกะงาน (`/attendance/shift-patterns`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `ShiftPatternsPageComponent` + `AttendanceAdminService` จัดการรูปแบบกะปกติ/ข้ามคืน/ยืดหยุ่น เวลาพัก และเกณฑ์สายผ่าน SlideDrawer | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `GET/POST/PUT /api/v1/shifts` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
