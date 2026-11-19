# HrmSaas — Multi-Tenant Cloud HRMS SaaS

> **MANDATORY AI / AGENT DIRECTIVE — READ BEFORE ANY ACTION**
>
> Before writing any code, creating any file, running any migration, or proposing any architecture in this repository, **you MUST read ALL files in the [`plan/`](./plan/) directory in full, every single time.**
>
> Do NOT rely on memory, partial reads, or assumptions. Every rule, schema, token, naming convention, and build order in `plan/` is strictly enforced.

---

## Required Reading List (`plan/`)

You must read **all 7 specification files** below before starting any task:

| Order | File | Scope & Mandatory Rules |
|---|---|---|
| 1 | [`plan/01-frontend-angular.md`](./plan/01-frontend-angular.md) | Angular v18/v19+ Standalone Components, Signals-first state, `OnPush`, Strict TypeScript, Functional Guards & Interceptors |
| 2 | [`plan/02-backend-dotnet.md`](./plan/02-backend-dotnet.md) | ASP.NET Core (.NET 8/9+) Clean Architecture, Auth Cookies, Sessions, Middleware ordering, Proxy behavior |
| 3 | [`plan/03-database-postgres.md`](./plan/03-database-postgres.md) | PostgreSQL 16+ schema (28 tables), EF Core Code-First + Dapper (read-only reporting), Multi-Tenant Row-Level Security (RLS) |
| 4 | [`plan/04-oop-clean-architecture-rules.md`](./plan/04-oop-clean-architecture-rules.md) | OOP & Clean Architecture, Junior-friendly readable code, Zero-Hardcode, Shared Components (applies across FE, BE, API, DB) |
| 5 | [`plan/05-ux-ui-design-rules.md`](./plan/05-ux-ui-design-rules.md) | UI/UX Pro Max, CSS Token Architecture, Strict "No Badge" policy (`<app-status-indicator>` only), no raw hex colors |
| 6 | [`plan/06-testing-and-api-toggle.md`](./plan/06-testing-and-api-toggle.md) | End-to-end build order (`Database -> Backend API -> Frontend`), Database Seeder, Navbar `API: ON / OFF` kill-switch test |
| 7 | [`plan/07-implementation-benchmark-rules.md`](./plan/07-implementation-benchmark-rules.md) | Mandatory 5-column Implementation Benchmark log (`docs/BENCHMARK_LOG.md`), per-page `PAGE_LOG.md`, and end-of-response summary |

---

## Non-Negotiable Pre-Flight Rules for AI Agents

1. **Read All 7 Files First:** Never skip reading `plan/01-*.md` through `plan/07-*.md` when starting a session or task.
2. **Follow Strict Build Order:** Always implement and verify in order: **Database (PostgreSQL + RLS + Seeder) -> Backend API (.NET) -> Frontend (Angular)**. Never build Frontend pages with mock data before the Database and Backend API are ready.
3. **Junior-Friendly, Traceable Code:** Write clear, step-by-step code (`if / else`, `switch`, `foreach`/`for`, direct helpers). Avoid over-engineered abstractions, clever one-liners, or hidden magic.
4. **Zero Hardcode & Kill-Switch Compliance:** Never hardcode business constants or mock datasets in UI components. Every Angular page must pass the Top Navbar `API: ON / OFF` kill-switch test.
5. **Strict Design & Security Compliance:** Follow the "No Badge" rule and CSS Token Architecture in `05-ux-ui-design-rules.md`, and never store auth tokens in `localStorage`/`sessionStorage` (use HttpOnly Cookies per `02-backend-dotnet.md`).
6. **Mandatory Implementation Benchmark & Per-Page Log:** After completing any code or architecture change, update [`docs/BENCHMARK_LOG.md`](./docs/BENCHMARK_LOG.md) (newest first), create/update `frontend/src/app/features/<domain>/pages/<page>/PAGE_LOG.md` when working on any Angular page, and display the exact 5-column Benchmark table at the end of your chat response per `07-implementation-benchmark-rules.md`.

---

## Tech Stack Summary

- **Frontend:** Angular v18/v19+ (Standalone, Signals, `OnPush`, Strict TS) — [`frontend/`](./frontend/)
- **Backend:** C# / ASP.NET Core Web API (.NET 8/9+, Clean Architecture, EF Core + Npgsql, Dapper) — [`backend/`](./backend/)
- **Database:** PostgreSQL 16+ with Row-Level Security (RLS)

---

## Project Documentation & Environment

- **Root Agent Rules:** [`AGENTS.md`](./AGENTS.md)
- **Implementation Benchmark Log:** [`docs/BENCHMARK_LOG.md`](./docs/BENCHMARK_LOG.md)
- **Data Dictionary (34 Tables):** [`docs/DATA_DICTIONARY.md`](./docs/DATA_DICTIONARY.md)
- **UI/UX Design System & CSS Tokens:** [`docs/DESIGN_SYSTEM.md`](./docs/DESIGN_SYSTEM.md)
- **Environment Template:** [`.env.example`](./.env.example) (copy to `.env` for local / Docker runs)
