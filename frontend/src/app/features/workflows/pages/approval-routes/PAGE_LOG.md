# Work Log — ApprovalRoutesPageComponent (`/workflows/approval-routes`)

- **Component / Module:** `ApprovalRoutesPageComponent`
- **Path:** `frontend/src/app/features/workflows/pages/approval-routes/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าตั้งค่าสายอนุมัติ (`/workflows/approval-routes`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `ApprovalRoutesPageComponent` จัดการ WorkflowDefinition และเพิ่ม/ลบ Step ลำดับผู้อนุมัติแบบ Dynamic ผ่าน SlideDrawer | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | เพิ่ม `GET/POST/PUT /api/v1/approvals/routes` ใน `ApprovalsController` / `LeaveAndWorkflowService` + `WorkflowDefinition.Update()` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
