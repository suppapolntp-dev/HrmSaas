# Work Log — EmployeeDetailPageComponent (`/employees/:id`)

- **Component / Module:** `EmployeeDetailPageComponent`
- **Path:** `frontend/src/app/features/employees/pages/employee-detail/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้าโปรไฟล์พนักงาน 360 (`/employees/:id`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `EmployeeDetailPageComponent` แบบ 4 แท็บ (ข้อมูลพื้นฐาน สัญญาจ้าง สายบังคับบัญชา โควตาวันลา) พร้อม Drawer แก้ไขข้อมูล เพิ่มสัญญา และตั้งค่าหัวหน้างาน | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `GET/PUT /api/v1/employees/{id}`, `POST /contracts`, `PUT /reporting-lines` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
