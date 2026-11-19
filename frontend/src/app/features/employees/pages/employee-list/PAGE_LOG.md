# Work Log — EmployeeListPageComponent (`/employees/list`)

- **Component / Module:** `EmployeeListPageComponent`
- **Path:** `frontend/src/app/features/employees/pages/employee-list/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้ารายชื่อพนักงาน (`/employees/list`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `EmployeeListPageComponent` + `EmployeesService` ทำ Server-side Pagination/Sort/Filter (สาขา แผนก สถานะ) และลิงก์ไป `/employees/:id` กับ `/employees/onboarding` | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `GET /api/v1/employees` และ `/api/v1/organizations/{branches,departments}` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
