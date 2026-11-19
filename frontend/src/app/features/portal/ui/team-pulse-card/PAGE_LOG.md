# Work Log — Team Pulse Card

- **Component / Module:** `TeamPulseCardComponent`
- **Path:** `frontend/src/app/features/portal/ui/team-pulse-card/`
- **Status:** `Done (ถูกแทนที่บนหน้าหลักด้วย WorkforceIntelligenceBoardComponent)`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] สร้าง TeamPulseCardComponent + SVG Circular Ring

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| เพิ่มการ์ดภาพรวมเวลาทำงานทีมและปรับสไตล์ Mindful Metrics (`25512609`) | สร้าง `team-pulse-card.component.ts`, `.html`, `.css` แสดง SVG Circular Ring `% เข้างาน`, KPI 5 สถานะ, รายการ Exceptions และ Who's Out | `ng build`: PASS<br>`ng test`: 13/13 passed (Zero Errors) | `ไม่มี — รับ TeamPulseDto จาก GET /api/v1/dashboard/team-pulse` | ต่อมาหน้าหลักเปลี่ยนไปใช้ `WorkforceIntelligenceBoardComponent` ตามคำสั่ง Template `27170104` |
