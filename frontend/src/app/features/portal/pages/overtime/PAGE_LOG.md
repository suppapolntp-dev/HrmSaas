# Work Log — OvertimePageComponent (`/portal/overtime`)

- **Component / Module:** `OvertimePageComponent`
- **Path:** `frontend/src/app/features/portal/pages/overtime/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าคำขอ OT ของฉัน (`/portal/overtime`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `OvertimePageComponent` + `PortalOvertimeService` แสดงประวัติคำขอ OT และฟอร์มยื่นขอทำ OT ผ่าน SlideDrawer (แปลง TimeOnly เป็น `HH:mm:ss`) | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | เพิ่ม `GET /api/v1/overtime/my-requests` ใน `OvertimeController` / `LeaveAndWorkflowService` และใช้ `POST /api/v1/overtime/requests` เดิม | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
