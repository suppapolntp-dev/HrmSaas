# Work Log — LeavePoliciesPageComponent (`/workflows/leave-policies`)

- **Component / Module:** `LeavePoliciesPageComponent`
- **Path:** `frontend/src/app/features/workflows/pages/leave-policies/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้านโยบายวันลา (`/workflows/leave-policies`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `LeavePoliciesPageComponent` จัดการประเภทวันลา ขั้นบันไดอายุงาน กฎ Prorate และการยกยอดข้ามปีผ่าน SlideDrawer | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `GET/POST/PUT /api/v1/leaves/policies` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
