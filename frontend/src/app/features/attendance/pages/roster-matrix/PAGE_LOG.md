# Work Log — RosterMatrixPageComponent (`/attendance/roster-matrix`)

- **Component / Module:** `RosterMatrixPageComponent`
- **Path:** `frontend/src/app/features/attendance/pages/roster-matrix/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าตารางกะ (`/attendance/roster-matrix`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `RosterMatrixPageComponent` แสดงตาราง Matrix พนักงาน × วันที่ พร้อมเลือกพนักงานหลายรายเพื่อจัดกะแบบกลุ่มผ่าน SlideDrawer | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `GET /api/v1/rosters`, `POST /api/v1/rosters/bulk-assign`, `/api/v1/shifts` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
