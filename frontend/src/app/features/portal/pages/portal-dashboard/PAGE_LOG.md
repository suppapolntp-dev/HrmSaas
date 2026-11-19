# Work Log — Portal Dashboard Page (`/portal/dashboard`)

- **Component / Module:** `PortalDashboardPageComponent`, `portal-dashboard-page.component.spec.ts`
- **Path:** `frontend/src/app/features/portal/pages/portal-dashboard/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] เชื่อม WorkforceIntelligenceBoardComponent (Template WorkPulse 27170104)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| เอา template หน้า Workforce Intelligence (`27170104`) แบบจริงตามหน้าเว็บนั้น | แก้ `portal-dashboard-page.component.ts`, `.html` สลับแท็บ `TEAM_ORG` มาเรียก `<app-workforce-intelligence-board>` และอัปเดต `portal-dashboard-page.component.spec.ts` ให้ตรงกับหัวข้อใหม่ | `ng build`: PASS<br>`ng test`: 4/4 passed (รวมเคส `API: OFF` 503) | `ไม่มี — ใช้ API เดิม GET /api/v1/dashboard/{team-pulse,hr-overview,payroll-readiness}` | `ไม่มี` |

## [2026-11-19] อัปเกรด Dashboard ครบ 2 มุมมอง (ESS + Team/Org)

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ทำ UI หน้า Dashboard ต่อจากข้อมูลจริงที่มีในระบบ | แก้ `portal-dashboard-page.component.ts`, `.html`, `.css`, `.spec.ts` และ `portal-dashboard.models.ts`, `portal-dashboard.service.ts` เพิ่ม View Switcher, แถบแจ้งเตือน Missing Punch, สลิปเงินเดือนล่าสุด, คำขอล่าสุด, และภาพรวมทีม/HR/Payroll | **Error ที่เจอระหว่างทำ:**<br>1) รัน `npx vitest run` ตรงๆ พัง `JIT compilation failed for service [class BrowserXhr]` -> เปลี่ยนมารันผ่าน `npx ng test --watch=false`<br>2) `portal-dashboard-page.component.spec.ts` Test 2 fail เพราะใส่ `httpMock.expectOne('/api/v1/dashboard/my-summary')` ซ้ำ 2 รอบ -> ลบบรรทัดซ้ำออก<br>**ผลลัพธ์สุดท้าย:** `ng build` PASS, `ng test` 4/4 passed | เพิ่ม `DashboardController` + `DashboardService` (4 Endpoints) | อัปเดต Docker container เพื่อดูหน้าเว็บจริงผ่าน browser |
