# Work Log — DepartmentsPositionsPageComponent (`/org/departments-positions`)

- **Component / Module:** `DepartmentsPositionsPageComponent`
- **Path:** `frontend/src/app/features/organization/pages/departments-positions/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าแผนกและตำแหน่ง (`/org/departments-positions`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `DepartmentsPositionsPageComponent` จัดการแผนกแบบ Tree Hierarchy (พร้อม ConfirmDialog ลบแผนก) ตำแหน่งงาน และระดับพนักงาน | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `/api/v1/organizations/{departments,departments/tree,positions,job-grades}` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
