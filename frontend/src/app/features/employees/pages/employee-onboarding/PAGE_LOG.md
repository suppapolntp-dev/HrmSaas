# Work Log — EmployeeOnboardingPageComponent (`/employees/onboarding`)

- **Component / Module:** `EmployeeOnboardingPageComponent`
- **Path:** `frontend/src/app/features/employees/pages/employee-onboarding/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] สร้างหน้ารับพนักงานใหม่ (`/employees/onboarding`)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | สร้าง `EmployeeOnboardingPageComponent` ฟอร์มรับพนักงานใหม่ 3 ส่วน (ข้อมูลสังกัด สัญญาจ้าง บัญชีผู้ใช้) ดึง Dropdown จริงจาก Organization API แล้วนำทางไป `/employees/:id` | `ng build` PASS · `all-pages-killswitch.spec.ts` `API: OFF` PASS | ไม่มี — ใช้ API เดิม `POST /api/v1/employees/onboarding` และ `/api/v1/organizations/*` | ทดสอบบนเบราว์เซอร์จริงร่วมกับ Docker |
