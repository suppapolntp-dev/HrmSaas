# Work Log — Core HTTP Interceptors

- **Component / Module:** `problemDetailsInterceptor`
- **Path:** `frontend/src/app/core/http/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] เพิ่ม Structured Error Logging + Diagnostic Hint ใน `problemDetailsInterceptor`

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| พี่ทำ log ตรวจ error หน่อย ui เว็บไม่ขึ้น (`404 Not Found` หลายเส้นทาง) | เพิ่ม `console.error('[HrmSaas API Error]', ...)` พร้อม `resolveDiagnosticHint` (บอกสาเหตุ 404/403/503/500 ชัดเจนใน Console) ใน `problem-details.interceptor.ts` | `ng build` PASS · `ng test` 10 files / 35 passed · Backend rebuilt on port `5153` (ทั้ง 10 เส้นทางตอบ `HTTP 401` ไม่มี 404) | Rebuild คอนเทนเนอร์ `hrmsaas-backend` บนพอร์ต `5153` ด้วย `docker compose --env-file .env.local up -d --build --no-deps backend` (ไม่แก้โค้ด BE หรือ DB Schema) | ใช้งานบนเบราว์เซอร์จริง (`localhost:4200`) |
