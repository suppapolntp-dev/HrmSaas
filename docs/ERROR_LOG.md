# HrmSaas — Runtime & API Error Diagnostic Log

> **Purpose:** บันทึกเหตุการณ์ Error จริงที่เจอระหว่างรันระบบ (404 / 500 / Docker / Proxy / Build) พร้อมสาเหตุ หลักฐานการตรวจ และวิธีแก้ไข เพื่อใช้ไล่ตรวจซ้ำได้ทันที  
> **Logging Rules:** **Newest First** (insert new entries directly below `<!-- ERROR_LOG_ENTRIES_START -->`) · **Append-Only**

<!-- ERROR_LOG_ENTRIES_START -->

## [2026-11-20 02:45] แก้ปัญหา `404 (Not Found)` ทุก Endpoint ใหม่บน `localhost:4200/api/v1/*`

- **อาการที่พบ (Symptom):** เปิดเว็บ Angular (`localhost:4200`) แล้วหน้า Dashboard / Attendance / Leaves / Overtime โหลดข้อมูลไม่ขึ้น โดย DevTools Console แสดง `404 (Not Found)` สำหรับ:
  - `/api/v1/attendance/my-today`
  - `/api/v1/attendance/my-history`
  - `/api/v1/rosters/my-upcoming?days=7`
  - `/api/v1/dashboard/my-summary`
  - `/api/v1/dashboard/hr-overview`
  - `/api/v1/dashboard/payroll-readiness`
  - `/api/v1/leaves/my-requests`
  - `/api/v1/overtime/my-requests`
- **สาเหตุ (Root Cause):**
  1. `frontend/src/proxy.conf.json` ชี้ `/api` ไปที่ `http://localhost:5153`
  2. คอนเทนเนอร์ Docker `hrmsaas-backend` (`0.0.0.0:5153->8080/tcp`) ค้าง Image เก่าที่ Build ไว้เมื่อ **10 ชั่วโมงก่อน** ซึ่งเป็นเวอร์ชันก่อนเพิ่ม `DashboardController` และ Endpoint ใหม่ใน `AttendanceController`, `RostersController`, `LeavesController`, `OvertimeController`, `ApprovalsController`
  3. เมื่อ Backend คืน `404`, `<app-state-container>` เข้าสู่ Error State ตามกฎห้าม Hardcode ข้อมูลจำลอง จึงไม่แสดงข้อมูล UI
- **การตรวจวินิจฉัย (Diagnostic Commands & Evidence):**
  - `docker ps` -> พบ `hrmsaas-backend Up 10 hours` ผูกพอร์ต `0.0.0.0:5153->8080/tcp`
  - `docker inspect hrmsaas-backend` -> รันด้วย `docker-compose.yml` + `.env.local`
- **วิธีแก้ไขที่ทำ (Fix Applied):**
  1. สั่ง Rebuild & Recreate เฉพาะคอนเทนเนอร์ Backend โดยไม่แตะ Postgres:
     ```powershell
     $env:BACKEND_PORT="5153"; $env:POSTGRES_HOST_PORT="5433"; docker compose --env-file .env.local up -d --build --no-deps backend
     ```
  2. เพิ่ม Structured Console Error Logger + Diagnostic Hint (`404`, `403`, `503`, `500`) ใน `frontend/src/app/core/http/problem-details.interceptor.ts` เพื่อให้ DevTools แสดง `[HrmSaas API Error]` พร้อมคำแนะนำแก้ปัญหาทันที
- **ผลตรวจหลังแก้ (Verification):**
  - `docker logs hrmsaas-backend --tail 40`: `Application started.` (ไม่มี Error)
  - ยิงตรวจทั้ง 10 เส้นทางบน `http://localhost:5153`: ทุกเส้นทางตอบกลับ `HTTP 401` (Route ลงทะเบียนครบและติด Auth Guard ปกติ, **0 เส้นทางที่ได้ 404**)
