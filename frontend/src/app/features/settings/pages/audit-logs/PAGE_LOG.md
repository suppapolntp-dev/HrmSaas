# Work Log — AuditLogsPageComponent (`/settings/audit-logs`)

- **Component / Module:** `AuditLogsPageComponent`
- **Path:** `frontend/src/app/features/settings/pages/audit-logs/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าประวัติการใช้งาน (`/settings/audit-logs`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `AuditLogsPageComponent` แสดงตาราง Audit Logs แบบ Server-side Pagination + ตัวกรอง Entity/Action และ SlideDrawer แสดง Diff JSON Old vs New | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `GET /api/v1/audit-logs` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
