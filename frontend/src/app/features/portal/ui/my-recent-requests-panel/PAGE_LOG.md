# Work Log — My Recent Requests Panel

- **Component / Module:** `MyRecentRequestsPanelComponent`
- **Path:** `frontend/src/app/features/portal/ui/my-recent-requests-panel/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] สร้าง MyRecentRequestsPanelComponent

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| เพิ่มรายการติดตามคำขอล่าสุดของฉันบน Dashboard ESS | สร้าง `my-recent-requests-panel.component.ts`, `.html`, `.css` แสดง 5 คำขอล่าสุดพร้อม Step, `app-status-indicator` และเส้นเน้นซ้าย `3px` สำหรับ `PENDING` | `ng build`: PASS<br>`ng test`: 13/13 passed (Zero Errors) | `ไม่มี — รับ MyDashboardRequestItemDto[] จาก GET /api/v1/dashboard/my-summary` | `ไม่มี` |
