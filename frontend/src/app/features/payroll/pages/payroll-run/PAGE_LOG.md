# Work Log — PayrollRunPageComponent (`/payroll/run/:periodId`)

- **Component / Module:** `PayrollRunPageComponent`
- **Path:** `frontend/src/app/features/payroll/pages/payroll-run/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าคำนวณเงินเดือนประจำงวด (`/payroll/run/:periodId`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `PayrollRunPageComponent` แสดง KPI สรุปงวด ปุ่มคำนวณเงินเดือน ปุ่มล็อกงวดผ่าน ConfirmDialog ตารางสลิปรายคน และ SlideDrawer รายละเอียดรายการ | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `calculate`, `lock`, `payslips`, `periods` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
