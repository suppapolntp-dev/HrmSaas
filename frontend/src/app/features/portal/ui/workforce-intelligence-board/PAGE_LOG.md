# Work Log — Workforce Intelligence Board

- **Component / Module:** `WorkforceIntelligenceBoardComponent`
- **Path:** `frontend/src/app/features/portal/ui/workforce-intelligence-board/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] สร้างและจัดวาง Layout ตาม WorkPulse Dribbble (`27170104`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ขอ Dashboard template ตามหน้า WorkPulse (`27170104`) | สร้าง `workforce-intelligence-board.component.ts`, `.html`, `.css` จัดโครง 8-col ซ้าย (3 KPI Cards + Summary Card), 4-col ขวา (Payroll Priority + 7-day Calendar Strip + Alerts), และ Full-Width Bottom 3-Tab Table (`เวลาทำงาน`, `รออนุมัติ`, `ครบกำหนด 30 วัน`) | **Error ที่เจอ:** `ng build` ล้มเหลว `src/styles/_shared-layout.css exceeded maximum budget (8.00 kB) by 429 bytes (8.43 kB)` เพราะ `@import _shared-layout.css` เกินความจำเป็น<br>**วิธีแก้:** ลบ `@import _shared-layout.css` ออกจาก `workforce-intelligence-board.component.css`<br>**ผลลัพธ์สุดท้าย:** `ng build` PASS (เหลือ warning 7.72 kB < 8 kB), `ng test` 13/13 passed | `ไม่มี — รับ input() จาก PortalDashboardPageComponent` | `ไม่มี` |
