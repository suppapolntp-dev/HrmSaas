# Work Log — HR Overview Card

- **Component / Module:** `HrOverviewCardComponent`
- **Path:** `frontend/src/app/features/portal/ui/hr-overview-card/`
- **Status:** `Done (ถูกแทนที่บนหน้าหลักด้วย WorkforceIntelligenceBoardComponent)`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] สร้าง HrOverviewCardComponent

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| เพิ่มการ์ด Workforce Snapshot และแจ้งเตือน 30 วัน | สร้าง `hr-overview-card.component.ts`, `.html`, `.css` แสดงจำนวน Active/Probation/New Hires, สัดส่วนแผนก และแจ้งเตือนครบโปร/หมดสัญญา | `ng build`: PASS<br>`ng test`: 13/13 passed (Zero Errors) | `ไม่มี — รับ HrOverviewDto จาก GET /api/v1/dashboard/hr-overview` | ต่อมาหน้าหลักเปลี่ยนไปใช้ `WorkforceIntelligenceBoardComponent` ตามคำสั่ง Template `27170104` |
