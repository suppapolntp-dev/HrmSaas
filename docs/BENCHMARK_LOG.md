# HrmSaas — Implementation Benchmark Log

> **Policy Source:** [`plan/07-implementation-benchmark-rules.md`](../plan/07-implementation-benchmark-rules.md)  
> **Logging Rules:** **Newest First** (insert new entries directly below the start marker below) · **Append-Only** (never overwrite past entries) · **Exact 5 Columns** · **Update co-located `PAGE_LOG.md` for page work** · **Must also display the same table at the end of the chat response**.

<!-- BENCHMARK_ENTRIES_START -->

## [2026-11-20] Rebuild Stale `hrmsaas-backend` Container & Add Error Diagnostic Logging (`docs/ERROR_LOG.md`)

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `problemDetailsInterceptor` (`frontend/src/app/core/http/problem-details.interceptor.ts`) + `docs/ERROR_LOG.md` + Docker `hrmsaas-backend` | แก้ปัญหาหน้าเว็บ UI ไม่ขึ้นเพราะ Proxy (`localhost:5153`) ยิงเข้าคอนเทนเนอร์ `hrmsaas-backend` ตัวเก่าที่ค้างอยู่ 10 ชม. จนได้ `404` ทุก Endpoint ใหม่ พร้อมเพิ่ม Structured Error Log ใน Console และไฟล์ `docs/ERROR_LOG.md` เพื่อไล่สาเหตุได้ทันที | `ไม่มีการแก้โค้ด Backend / DB — Rebuild Docker Image ของ hrmsaas-backend บนพอร์ต 5153 (--no-deps) เพื่อให้ Endpoint ใหม่ที่สร้างไว้รันจริง` | โค้ด Controller/Service ฝั่ง Backend มีอยู่ครบแล้วใน Repo แต่ Runtime container บนพอร์ต `5153` ยังเป็น Image เก่า จึงต้อง Rebuild เฉพาะ Service `backend` โดยไม่แตะ `postgres` และเพิ่ม Diagnostic Hint ฝั่ง Interceptor | `docker ps & logs: PASS (Up, Application started) \| 10 API probes: HTTP 401 (0x 404) \| ng build: PASS \| ng test: 10 files, 35 passed \| API: OFF: PASS` |

## [2026-11-20] Complete All Remaining 18 Angular Pages (22/22 Screens) + Backend Workflow Endpoints

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| ทั้ง 18 หน้าที่เหลือ (`portal/{leaves,overtime,payslips}`, `org/{companies-branches,departments-positions}`, `employees/{list,:id,onboarding}`, `attendance/{shift-patterns,roster-matrix,daily-logs}`, `workflows/{inbox,leave-policies,approval-routes}`, `payroll/{periods,run/:periodId}`, `settings/{roles-permissions,audit-logs}`) + lazy routes ใน `app.routes.ts` + `_page-patterns.css` + `http-error.helper.ts` | ปิด Placeholder ทั้งหมดให้ครบทั้ง 22 หน้าจอตาม `plan/01-frontend-angular.md` โดยทุกหน้าเชื่อม API จริง ใช้ Shared UI (`app-data-table`, `app-state-container`, `app-status-indicator`, `app-slide-drawer`, `app-confirm-dialog`, `app-form-field`) และแยก global page layout ออกใน `_page-patterns.css` เพื่อไม่ให้ Component CSS เกิน budget | **เพิ่ม Backend Endpoint (ไม่แก้ DB Schema):** เพิ่ม `GET /api/v1/leaves/my-requests`, `GET /api/v1/overtime/my-requests`, `GET /api/v1/approvals/history`, `GET/POST/PUT /api/v1/approvals/routes`, เพิ่ม `mineOnly` ใน `PayrollController.GetPayslips`, เพิ่ม `WorkflowDefinition.Update()` และ Query/Command ใน `LeaveAndWorkflowService` | เดิมยังขาด Endpoint สำหรับดึงรายการคำขอของตนเอง, ประวัติอนุมัติ, และ CRUD สายอนุมัติ จึงเติมเฉพาะส่วนที่ขาดโดยใช้ตาราง DB และ Entity เดิมที่มีอยู่แล้วทั้งหมด | `dotnet build HrmSaas.sln: PASS (0 errors) \| ng build: PASS (0 new CSS budget warn) \| ng test: 10 files, 35 passed \| API: OFF: PASS (all 18 new pages verified via all-pages-killswitch.spec.ts, zero fallback) \| CSS Tokens & No Badge: PASS` |

## [2026-11-20] Rename `/portal/my-*` Routes & Finalize `/portal/attendance` (`AttendancePageComponent`)

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `/portal/attendance` (`AttendancePageComponent`, `PortalAttendanceService`, `portal.routes.ts`, `navigation.config.ts`, Portal links) | ถอดคำนำหน้า `my-` ออกจาก Route กลุ่ม Portal (`attendance`, `leaves`, `overtime`, `payslips`) ชื่อโฟลเดอร์/Component และเพิ่มปุ่ม `ล้างตัวกรอง` ในหน้า `/portal/attendance` ให้ใช้งานครบจบ | `ไม่มี — ใช้ API เดิม GET /api/v1/attendance/my-history และ POST /api/v1/attendance/time-adjustments` | เป็นการปรับ Naming & Routing ฝั่ง Frontend ให้สั้นตรงตามคำสั่ง โดยไม่ต้องแก้ Contract ของ Backend API หรือโครงสร้าง DB | `ng build: PASS \| ng test: 9 files, 15 passed \| API: OFF: PASS (unit spec 503 ProblemDetails, zero fallback) \| CSS Tokens & No Badge: PASS` |

## [2026-11-20] Build `/portal/my-attendance` (`MyAttendancePageComponent`) + `GET /api/v1/attendance/my-history`

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `/portal/my-attendance` (`MyAttendancePageComponent`, `MyAttendanceService`, `portal.routes.ts`) + `AttendanceController` / `AttendanceService` | สร้างหน้าเวลาทำงานของฉันตามลำดับ `plan/01` เพื่อดูประวัติการลงเวลาเข้า-ออกรายวัน กรองช่วงวันที่/สถานะ/ค้นหา เรียงลำดับ ใช้ `app-data-table` ที่เพิ่งอัปเกรด พร้อมยื่นคำขอแก้ไขเวลาผ่าน `app-slide-drawer` | **เพิ่ม Backend Endpoint:** `GET /api/v1/attendance/my-history` (`[RequirePermission("portal.attendance.read")]`) ใน `AttendanceController` + `GetMyAttendanceHistoryAsync` ใน `AttendanceService` และใช้ `POST /api/v1/attendance/time-adjustments` เดิม (ไม่แก้ DB Schema) | `GET /api/v1/attendance/daily-summary` เดิมล็อกสิทธิ์ `attendance.log.read` สำหรับ HR/Operation เท่านั้น จึงต้องเปิด `my-history` ให้พนักงานทั่วไปดึงเฉพาะข้อมูลตนเอง (`portal.attendance.read`) โดยไม่ต้องสร้างตาราง DB ใหม่ | `dotnet build HrmSaas.sln: PASS (0 errors) \| ng build: PASS \| ng test: 9 files, 15 passed \| API: OFF: PASS (unit spec 503 ProblemDetails, zero fallback) \| CSS Tokens & No Badge: PASS` |

## [2026-11-20] Upgrade Shared `DataTableComponent` (`app-data-table`) from `Satifaction2` Pattern

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `DataTableComponent` (`frontend/src/app/shared/ui/data-table/*`) | อัปเกรดตารางกลางของระบบให้รองรับ Head Section (`title`, `subtitle`, `statusLabel`, Search + Clear, `slot-header-actions`), Filter Bar (`slot-filters` + Sort Dropdown), 3-state sort (`asc -> desc -> reset`), Row Actions Dropdown (`DataTableAction`), Zebra rows, Empty row และ Jump-to-Page input ตามแบบ `Satifaction2` โดยคุมตาม CSS Tokens และกฎ No-Badge | `ไม่มี — เป็น Shared Presentational UI Component ไม่แตะ Backend/DB` | ขยาย `app-data-table` ตัวเดิมแบบ Backward-compatible ด้วย optional `input()`/`output()` signals แทนการสร้างตารางใหม่ซ้ำซ้อน และเลี่ยง `<span class="badge">` โดยใช้ `<app-status-indicator>` แทน | `ng build: PASS (CSS budget warn 6.64 kB) \| ng test: 8 files, 13 passed \| API: OFF: N/A \| CSS Tokens & No Badge: PASS` |

## [2026-11-19] Dual-Track Logging: Separate `docs/CHAT_LOG.md` for All Prompts

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `docs/CHAT_LOG.md`, `plan/07-implementation-benchmark-rules.md`, `AGENTS.md` | เก็บทุกคำสั่งและคำถามของผู้ใช้ในแชท (รวมรอบถาม-ตอบ/Read-only ที่เดิมถูกยกเว้น) ไว้ในไฟล์แยก โดยไม่ทำให้ `BENCHMARK_LOG.md` และ `PAGE_LOG.md` บวม | `ไม่มี — เป็นไฟล์เอกสาร Markdown (.md) ไม่กระทบโค้ดหรือ DB` | แยก Channel ของ Chat Prompt History (`CHAT_LOG.md`) ออกจาก Code/Architecture Traceability (`BENCHMARK_LOG.md`, `PAGE_LOG.md`) เพื่อให้ได้ทั้งประวัติแชทครบถ้วนและ Log โค้ดที่สะอาด | `Build: N/A (Docs/Rules only) \| API: OFF: N/A \| Rules & Markers Verified` |

## [2026-11-19] Extract TopNavbarComponent (`app-top-navbar`) from AppShellComponent

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `TopNavbarComponent` (`frontend/src/app/core/layout/top-navbar.component.*`) + `AppShellComponent` | แยกส่วน Top Navbar (Mobile menu toggle, Company/Branch context switchers, `API: ON/OFF`, Theme toggle, Logout) ออกเป็น Presentational Component เฉพาะทาง เพื่อลดความหนาแน่นของ `AppShellComponent` และลดขนาด CSS ของ `app-shell.component.css` | `ไม่มี — ใช้ API เดิม POST /api/v1/auth/switch-context และ /api/v1/auth/logout ผ่าน AppShellComponent` | ย้ายเฉพาะ View/Template ของ Topbar มาเป็น Dumb Component (`input()`/`output()` signals) โดยให้ `AppShellComponent` คุม State และ Service call ตามเดิม ไม่ต้องเพิ่ม Endpoint หรือแก้ DB | `ng build: PASS \| ng test: 8 files, 13 passed \| API: OFF: N/A (Layout Presentational) \| CSS Tokens & No Badge: PASS` |

## [2026-11-19] Per-Folder Work Log (`PAGE_LOG.md`) Standard & Backfill

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `AGENTS.md`, `plan/07-implementation-benchmark-rules.md` + `PAGE_LOG.md` ใน 10 โฟลเดอร์ที่ทำงานจริง (`Dashboard`, `Controllers/v1`, `core/layout`, `portal-dashboard`, และ 6 UI cards) | ขยายกฎ Per-Page Log ให้ครอบคลุมทุกโฟลเดอร์ที่สร้าง/แก้ไฟล์จริง พร้อมเพิ่มคอลัมน์ `ผลลัพธ์ / Error ที่เจอ` เพื่อเก็บประวัติคำสั่ง, ไฟล์ที่เขียน, Error ที่เกิดจริง และวิธีแก้ครบถ้วนในทุกโฟลเดอร์ | `ไม่มี — เป็นไฟล์เอกสาร Markdown (.md) ไม่กระทบโค้ดหรือ DB` | ไฟล์ `.md` ไม่ถูก Angular CLI หรือ .NET SDK นำไป compile จึงไม่เพิ่มขนาด Bundle และช่วยให้ไล่ประวัติระดับโฟลเดอร์ได้ทันที | `Docs & Rules Updated \| FE/BE Build: Unchanged (PASS prior)` |

## [2026-11-19] WorkPulse Dribbble Template Alignment (`27170104` Layout)

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `WorkforceIntelligenceBoardComponent` (`portal-dashboard-page.component.spec.ts`) | ปรับโครง `WorkforceIntelligenceBoardComponent` ให้ตรงตามสัดส่วนหน้า Dribbble `27170104` (ฝั่งซ้าย 8-col: 3 KPI Cards + Summary Card, ฝั่งขวา 4-col: Payroll Priority + Work Calendar 7-day strip + Alerts, ด้านล่าง Full-Width 3-Tab Work Assignment Table) โดยใช้ข้อมูลจริงจาก DB ทั้งหมด | `ไม่มี — ใช้ API เดิม GET /api/v1/dashboard/{team-pulse,hr-overview,payroll-readiness} และ /api/v1/approvals/pending` | จัดสัดส่วน Layout และตาราง 3 แท็บในฝั่ง Presentational Component โดยอิงข้อมูลจริงชุดเดิม ไม่ต้องเพิ่ม Endpoint หรือแก้ DB | `ng build: PASS \| ng test: 8 files, 13 passed \| Zero-Hardcode` |

## [2026-11-19] Add Per-Page Work Log (PAGE_LOG.md) Rule

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `plan/07-implementation-benchmark-rules.md`, `AGENTS.md`, `README.md`, `plan/01-frontend-angular.md`, `docs/BENCHMARK_LOG.md` | เพิ่มกฎสร้างไฟล์ `PAGE_LOG.md` วางติดในโฟลเดอร์ของแต่ละหน้า เพื่อบันทึกประวัติว่าสั่งอะไรไปแล้วและทำอะไรเสร็จแล้วบ้าง แยกตามรายหน้าให้ตามงานง่าย | `ไม่มี — เป็นเอกสารกฎระดับโปรเจกต์` | วาง `PAGE_LOG.md` ใน `features/<domain>/pages/<page>/` ตามความสะดวกเวลาเปิดแก้โค้ด (ไม่กระทบ Angular bundle เพราะ `tsconfig.app.json` รับเฉพาะ `src/**/*.ts`) และบังคับสร้างแบบ On-Demand เพื่อไม่ให้มีไฟล์เปล่าค้าง 19 หน้า (ยังไม่ย้อนสร้างให้ 3 หน้าเดิมตามคำสั่ง) พร้อมแก้หัวไฟล์ `BENCHMARK_LOG.md` ไม่ให้มี marker ซ้ำสองจุดกัน `edit_file` แทรกซ้ำ | `Build: N/A (Docs/Rules only) \| API: OFF: N/A \| Rules, Templates & Zero-Backfill Verified` |

## [2026-11-19] Workforce Intelligence 3-Tier Dashboard Template (`WorkforceIntelligenceBoardComponent`)

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `WorkforceIntelligenceBoardComponent` + `PortalDashboardPageComponent` | จัดโครงหน้า "ภาพรวมทีมและองค์กร" เป็น 3 ชั้นตามแนวทาง Workforce Intelligence (1: 4-Col KPI Pulse Row, 2: 8+4 Col Department & Alerts Grid, 3: Full-Width Actionable Table) พร้อมคงระบบ No-Badge และ Privacy Mask | `ไม่มี — ใช้ API เดิม GET /api/v1/dashboard/{team-pulse,hr-overview,payroll-readiness}` | นำข้อมูลจริงจาก 3 Endpoint ที่สร้างไว้แล้วมาจัดวาง Layout ใหม่ในฝั่ง Presentational Component ไม่ต้องแก้ Backend หรือเพิ่ม DB | `ng build: PASS \| ng test: 8 files, 13 passed \| Zero-Hardcode` |

## [2026-11-19] Sidebar Collapsed State: Header Collapse Button + Profile Expand

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `AppShellComponent` (`app-shell.component.html`, `app-shell.component.css`) | คงปุ่มย่อ `<` ไว้ด้านขวาของ Header เฉพาะตอน Sidebar เปิดอยู่ และซ่อนปุ่มตอนพับให้เหลือโลโก้ `H` เดี่ยวๆ โดยกดที่ Profile Avatar ด้านล่างเพื่อขยายกลับได้ | `ไม่มี — เป็นการปรับ Layout/UI ฝั่ง Frontend เท่านั้น` | ไม่ให้ปุ่มเบียดโลโก้ `H` ในความกว้าง `72px` ตอนพับ และยังคงปุ่มย่อในจุดที่กดง่ายตอน Sidebar กางอยู่ | `ng build: PASS \| ng test: 8 files, 13 passed \| Zero-Hardcode` |

## [2026-11-19] Sidebar Collapse Toggle Moved to Profile Footer

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `AppShellComponent` (`app-shell.component.html`, `app-shell.component.css`) | ย้ายปุ่มย่อ/ขยาย Sidebar จากข้างโลโก้ `H` (ที่เบียดกันตอน Sidebar ย่อเหลือ 72px) มาไว้ที่ Profile Footer ด้านล่างแทน ให้หัว Sidebar โล่งสะอาดตา | `ไม่มี — เป็นการปรับ Layout/UI ฝั่ง Frontend เท่านั้น` | แก้ปัญหาปุ่มเบียดโลโก้ตอนยุบเมนูโดยใช้ฟังก์ชัน `toggleSidebarCollapse()` เดิม ไม่ต้องแตะ Backend หรือ DB | `ng build: PASS \| ng test: 8 files, 13 passed \| Zero-Hardcode` |

## [2026-11-19] Portal Dashboard (ESS + Team/Org) & Ticky Sidebar + Mindful Metrics Cards

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `AppShellComponent` (`navigation.config.ts`, `_tokens.css`, `_utilities.css`) | ยกระดับ Sidebar ให้ย่อ-ขยายได้ มีช่องค้นหาเมนู ไอคอน SVG เส้นเน้นซ้าย 3px และ Profile Footer ตามแนวทาง Ticky Admin UX พร้อมเพิ่ม Token `--radius-xl` / `--text-2xl` | `ไม่มี — ใช้โครง Session และ Dynamic Permissions เดิม (/api/v1/auth/me)` | จัดการ Layout, ไอคอน และตัวกรองเมนูฝั่ง UI โดยอิงสิทธิ์จาก Session Store เพื่อไม่ให้ต้องเพิ่ม Endpoint ฝั่ง Backend | `ng build: PASS \| ng test: 8 files, 13 passed \| Zero-Hardcode` |
| `PortalDashboardPageComponent` + 5 Cards (`LatestPayslipCard`, `MyRecentRequestsPanel`, `TeamPulseCard`, `HrOverviewCard`, `PayrollReadinessCard`) | เพิ่มมุมมอง 2 ชั้น (งานของฉัน / ภาพรวมทีมและองค์กร), สลิปเงินเดือนล่าสุดพร้อม Privacy Mask Toggle, วงแหวน % เข้างานทีม, แจ้งเตือนครบโปร/หมดสัญญา 30 วัน และความพร้อมตัดรอบเงินเดือน | `เพิ่ม Backend — DashboardController (4 Endpoints: my-summary, team-pulse, hr-overview, payroll-readiness), DashboardService, DashboardDtos (ไม่แก้ DB Schema)` | งาน Dashboard ต้องการ Aggregate Count/Summary จากหลายตารางใน SQL โดยตรงเพื่อไม่ให้ Angular ยิง Paged API ดึงข้อมูลทั้งบริษัทมาคำนวณฝั่งหน้าบ้าน; ใช้ตารางเดิมครบจึงไม่ต้องทำ DB Migration | `dotnet build: PASS (0 Errors) \| ng build: PASS \| ng test: 4/4 PASS (รวมเคส API: OFF 503)` |

## [2026-11-19] Setup Implementation Benchmark Rule & Root AGENTS.md

| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
| `plan/07-implementation-benchmark-rules.md`, `docs/BENCHMARK_LOG.md`, `AGENTS.md`, `README.md`, `plan/06-testing-and-api-toggle.md` | ตั้งกฎ Benchmark Traceability ให้ทุกงานบันทึกหน้า/Component เหตุผล การแตะ Backend/DB และผลตรวจจริง เพื่อกัน Bloat และย้อนตรวจสอบได้ทั้งโปรเจกต์ | `ไม่มี — เป็นเอกสารกฎระดับโปรเจกต์และ Root Agent Rule` | แยกเป็นไฟล์กฎที่ 7 (`plan/07-*`) + `AGENTS.md` ที่ root เพื่อให้ Antigravity และ Agent อื่นโหลดอัตโนมัติทันทีโดยไม่ซ้ำซ้อน token กับ `GEMINI.md` และไม่กระทบ global plugin `sister` | `Build: N/A (Docs/Rules only) \| API: OFF: N/A \| Rules & Links Verified` |
