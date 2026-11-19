# Work Log — PayslipsPageComponent (`/portal/payslips`)

- **Component / Module:** `PayslipsPageComponent`
- **Path:** `frontend/src/app/features/portal/pages/payslips/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าสลิปเงินเดือน (`/portal/payslips`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `PayslipsPageComponent` + `PortalPayslipsService` แสดงรายการสลิปย้อนหลัง รายได้ ประกันสังคม ภาษี พร้อม SlideDrawer แสดงรายการรับ-หักและพิมพ์ PDF | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | เพิ่ม query param `mineOnly` ใน `PayrollController.GetPayslips` (`GET /api/v1/payroll/payslips?mineOnly=true`) | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
