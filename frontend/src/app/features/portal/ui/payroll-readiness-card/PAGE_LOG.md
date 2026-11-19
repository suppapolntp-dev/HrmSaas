# Work Log — Payroll Readiness Card

- **Component / Module:** `PayrollReadinessCardComponent`
- **Path:** `frontend/src/app/features/portal/ui/payroll-readiness-card/`
- **Status:** `Done (ถูกแทนที่บนหน้าหลักด้วย WorkforceIntelligenceBoardComponent)`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] สร้าง PayrollReadinessCardComponent

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| เพิ่มการ์ดความพร้อมงวดเงินเดือนและปุ่มซ่อนยอด | สร้าง `payroll-readiness-card.component.ts`, `.html`, `.css` แสดง Cutoff countdown, Checklist (`PENDING`, `MISSING_PUNCH`) และยอดรวมแบบซ่อนตัวเลข | `ng build`: PASS<br>`ng test`: 13/13 passed (Zero Errors) | `ไม่มี — รับ PayrollReadinessDto จาก GET /api/v1/dashboard/payroll-readiness` | ต่อมาหน้าหลักเปลี่ยนไปใช้ `WorkforceIntelligenceBoardComponent` ตามคำสั่ง Template `27170104` |
