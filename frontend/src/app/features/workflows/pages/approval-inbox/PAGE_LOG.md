# Work Log — ApprovalInboxPageComponent (`/workflows/inbox`)

- **Component / Module:** `ApprovalInboxPageComponent`
- **Path:** `frontend/src/app/features/workflows/pages/approval-inbox/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้ากล่องอนุมัติคำขอ (`/workflows/inbox`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `ApprovalInboxPageComponent` + `WorkflowsService` แยกแท็บ รออนุมัติ / ประวัติ พร้อม Drawer กด Approve / Reject และบังคับเหตุผลเมื่อปฏิเสธ | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | เพิ่ม `GET /api/v1/approvals/history` ใน `ApprovalsController` / `LeaveAndWorkflowService` และใช้ `pending`, `decide` เดิม | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
