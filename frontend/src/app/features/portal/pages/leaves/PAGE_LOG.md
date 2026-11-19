# Work Log — LeavesPageComponent (`/portal/leaves`)

- **Component / Module:** `LeavesPageComponent`
- **Path:** `frontend/src/app/features/portal/pages/leaves/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าการลาของฉัน (`/portal/leaves`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `LeavesPageComponent` + `PortalLeavesService` แสดง KPI โควตาวันลา ตารางประวัติคำขอ ยื่นใบลาผ่าน SlideDrawer และยกเลิกคำขอผ่าน ConfirmDialog | `ng build` PASS (0 budget warn) · `all-pages-killswitch.spec.ts` `API: OFF` + happy path PASS | เพิ่ม `GET /api/v1/leaves/my-requests` ใน `LeavesController` / `LeaveAndWorkflowService` (ไม่แก้ DB Schema) | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
