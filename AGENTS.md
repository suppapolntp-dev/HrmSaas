# HrmSaas — Mandatory Agent Rules

## 1. Required Reading (`plan/01` – `plan/07`)

Before writing code, creating files, running migrations, or proposing architecture changes in this repository, read **all 7 specification files** in `plan/`:

1. [`plan/01-frontend-angular.md`](./plan/01-frontend-angular.md) — Angular v18/v19+ Standalone Components, Signals-first state, `OnPush`, Strict TS, 22-screen inventory
2. [`plan/02-backend-dotnet.md`](./plan/02-backend-dotnet.md) — ASP.NET Core (.NET 8/9+) Clean Architecture, HttpOnly Cookie Sessions, XSRF, exact Middleware pipeline order
3. [`plan/03-database-postgres.md`](./plan/03-database-postgres.md) — PostgreSQL 16+ schema, EF Core + Dapper (read-only reporting), Multi-Tenant Row-Level Security (RLS)
4. [`plan/04-oop-clean-architecture-rules.md`](./plan/04-oop-clean-architecture-rules.md) — Junior-friendly readable code, Zero-Hardcode, Rich Domain Entities, Smart/Dumb component split
5. [`plan/05-ux-ui-design-rules.md`](./plan/05-ux-ui-design-rules.md) — UI/UX Pro Max, central CSS tokens (`src/styles/_tokens.css`), Strict "No Badge" policy, 5 UI states
6. [`plan/06-testing-and-api-toggle.md`](./plan/06-testing-and-api-toggle.md) — Strict build order (`Database -> Backend API -> Frontend`), Seeder, Navbar `API: ON / OFF` kill-switch test
7. [`plan/07-implementation-benchmark-rules.md`](./plan/07-implementation-benchmark-rules.md) — Mandatory 5-column Implementation Benchmark & Traceability log

---

## 2. Non-Negotiable Architecture & Build Rules

- **Strict Build Order:** Always implement and verify in order: **Database (PostgreSQL + RLS + Seeder) -> Backend API (.NET) -> Frontend (Angular)**. Never build Frontend pages with mock data.
- **Junior-Friendly & Zero-Hardcode:** Write explicit step-by-step logic (`if / else`, `switch`, `foreach`/`for`, separate `signal()` fields). Never use nested ternaries, complex generic state wrappers, Specification classes, or hardcoded business constants/roles.
- **UI/UX Pro Max & No Badge:** Never create `<span class="badge">` or pill backgrounds; use `<app-status-indicator>` (6px dot + semantic text), left-border accents, or typography hierarchy. Never use raw hex/rgb colors or random local CSS variables; consume `src/styles/_tokens.css`, `_utilities.css`, and `_shared-layout.css`. Every async view must handle `Default`, `Hover/Focus`, `Loading Skeleton`, `Empty`, and `Error` states.
- **Security & Multi-Tenancy:** Use HttpOnly Server-Backed Session Cookies + XSRF tokens + EF Core Global Query Filters + PostgreSQL RLS. Never store auth tokens in `localStorage` or `sessionStorage`.
- **Kill-Switch Verification:** Every Angular page MUST pass the Top Navbar `API: ON / OFF` kill-switch check (`503 ProblemDetails` enters clean Error State with zero hardcoded fallback data).

---

## 3. Mandatory Implementation Benchmark & Per-Page Log Rules (`plan/07-implementation-benchmark-rules.md`)

After completing **ANY** code, database, or project rule/architecture change, you MUST record the project-wide benchmark, update the per-page work log (for Angular page work), and show the benchmark table in chat:

```markdown
| หน้า / Component ที่ทำ | ทำทำไม | เพิ่ม/แก้ Backend / DB ไหม | เหตุผลที่ต้องใส่ / ไม่ใส่ | ผลตรวจ (Build / API: OFF / Zero-Hardcode) |
| :--- | :--- | :--- | :--- | :--- |
```

1. **Update [`docs/BENCHMARK_LOG.md`](./docs/BENCHMARK_LOG.md):** Prepend a new dated section (`## [YYYY-MM-DD] <ชื่อฟีเจอร์ / รอบงาน>`) directly below `<!-- BENCHMARK_ENTRIES_START -->` (newest first, append-only).
2. **Per-Folder Work Log (`PAGE_LOG.md`):** Whenever creating or editing files in any code folder (Frontend `pages/`, `ui/`, `core/layout/`, or Backend `Features/`, `Controllers/`), create on-demand (do NOT pre-create empty logs for untouched folders) and prepend a dated entry directly below `<!-- PAGE_LOG_ENTRIES_START -->` in `<folder>/PAGE_LOG.md` using the 5-column table:
   `| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |`
3. **Update [`docs/CHAT_LOG.md`](./docs/CHAT_LOG.md) (Every Turn):** Prepend every user prompt/request (including Q&A and read-only checks) directly below `<!-- CHAT_LOG_ENTRIES_START -->` (newest first, append-only), while keeping `BENCHMARK_LOG.md` and `PAGE_LOG.md` strictly for actual code/DB/rule changes.
4. **Display in Chat:** Output the exact same 5-column Benchmark table at the end of your response.
5. **Column Discipline:**
   - Never leave `เพิ่ม/แก้ Backend / DB ไหม` blank or write `-`; if no Backend/DB change was made, state `ไม่มี — ใช้ API เดิม <METHOD /api/v1/...>`.
   - Explain concrete architectural/trade-off reasons in `เหตุผลที่ต้องใส่ / ไม่ใส่` (why new code/endpoint/table was needed, or why existing API/component was reused to avoid bloat).
   - Report only actual executed build/test/`API: OFF` results in the verification column; never fabricate `PASS`.
