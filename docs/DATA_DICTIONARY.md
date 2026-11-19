# HrmSaas — Data Dictionary (พจนานุกรมข้อมูลฐานข้อมูล PostgreSQL)

เอกสารนี้อ้างอิงตรงกับ Entity และ Fluent API Configurations ใน `backend/src/HrmSaas.Domain` และ `backend/src/HrmSaas.Infrastructure/Persistence/Configurations` ทั้งหมด **34 ตาราง (7 กลุ่ม)**

---

## 1. ภาพรวมสถาปัตยกรรมฐานข้อมูล (Architecture & Conventions)

| หัวข้อ | มาตรฐานที่ใช้ในโปรเจกต์ |
|---|---|
| **Engine** | PostgreSQL 16+ (Managed by EF Core Code-First + Npgsql, และ Dapper สำหรับ Complex Reporting Read-Only) |
| **Multi-Tenancy (2-Layer Defense)** | 1) **Application Layer:** EF Core Global Query Filters (`TenantId` + `ActiveCompanyId` + `!IsDeleted`)<br>2) **Database Layer:** PostgreSQL Row-Level Security (`ENABLE ROW LEVEL SECURITY`) ใช้ Policy `tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid` คู่กับ Non-Superuser DB Role (`hrm_app_user`) |
| **Naming Convention** | ใช้ `snake_case` ทั้งชื่อตาราง คอลัมน์ Primary Key (`pk_<table>`), Foreign Key (`fk_...`), Unique (`uq_...`), Index (`ix_...`), และ Check Constraint (`ck_...`) |
| **Primary Key** | `uuid` สร้างจาก Application ด้วย **UUIDv7** (`Guid.CreateVersion7()`) เพื่อเรียงลำดับ B-Tree Index ต่อเนื่อง |
| **Date & Time** | `timestamptz` เก็บเป็น **UTC เสมอ** ส่วนวันที่/เวลากะงานที่ไม่ขึ้นกับ Timezone ใช้ `date` และ `time` |
| **Numeric Precision** | เงิน/เงินเดือน/ภาษี: `numeric(18,4)` · วันลา/หน่วย: `numeric(10,2)` · อัตรา %: `numeric(7,4)` · พิกัด GPS: `numeric(10,7)` (ห้ามใช้ `float`/`double`) |
| **Status / Enum** | เก็บเป็น Semantic `varchar(...)` ตัวพิมพ์ใหญ่ พร้อม `CHECK` Constraint (ห้ามใช้ Magic Integer) |
| **Optimistic Concurrency** | ตาราง `employee_leave_balances`, `approval_requests`, `payroll_periods` ใช้ PostgreSQL system column **`xmin`** (`.IsRowVersion()`) กัน Race Condition |
| **Standard Audit Columns** | ตาราง Core มี `created_at` (`timestamptz`), `created_by` (`uuid NULL`), `updated_at` (`timestamptz NULL`), `updated_by` (`uuid NULL`), `is_deleted` (`boolean DEFAULT false`), `deleted_at` (`timestamptz NULL`) |

---

## Group 1: Tenant & Organization Structure (โครงสร้างผู้เช่าและองค์กร — 6 ตาราง)

### 1.1 `tenants`
ข้อมูลผู้เช่าหลักระดับ Holding / Account เจ้าของ SaaS (ไม่มี RLS `tenant_id` เพราะเป็นตารางแม่ของทุก Tenant)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_tenants`) | รหัส Tenant (UUIDv7) |
| `code` | `varchar(50)` | NO | UQ (`uq_tenants_code`) | รหัส Tenant สำหรับอ้างอิง |
| `name` | `varchar(200)` | NO | — | ชื่อองค์กร/กลุ่มบริษัทหลัก |
| `subscription_plan` | `varchar(50)` | NO | — | แพ็กเกจการใช้งาน SaaS |
| `status` | `varchar(30)` | NO | CHECK (`ACTIVE`, `SUSPENDED`, `TRIAL`) | สถานะบัญชีผู้เช่า |
| `created_at` | `timestamptz` | NO | — | วันเวลาที่สร้าง (UTC) |
| `updated_at` | `timestamptz` | YES | — | วันเวลาที่แก้ไขล่าสุด (UTC) |

### 1.2 `companies` *(RLS Enabled)*
นิติบุคคล / บริษัทย่อยภายใต้ Tenant

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_companies`) | รหัสบริษัท |
| `tenant_id` | `uuid` | NO | FK -> `tenants(id)`, UQ1 | รหัส Tenant เจ้าของ |
| `code` | `varchar(50)` | NO | UQ1 (`uq_companies_tenant_code`) | รหัสบริษัท (ไม่ซ้ำใน Tenant) |
| `legal_name` | `varchar(250)` | NO | — | ชื่อนิติบุคคลตามกฎหมาย |
| `tax_id` | `varchar(20)` | NO | — | เลขประจำตัวผู้เสียภาษี |
| `sso_employer_code` | `varchar(30)` | NO | — | รหัสนายจ้างประกันสังคม |
| `base_currency` | `varchar(3)` | NO | — | สกุลเงินหลัก (เช่น `THB`) |
| *Audit Columns* | *6 columns* | — | — | `created_at`, `created_by`, `updated_at`, `updated_by`, `is_deleted`, `deleted_at` |

### 1.3 `branches` *(RLS Enabled)*
สาขา สถานที่ทำงาน Timezone และขอบเขตเช็กอิน GPS / IP Whitelist

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_branches`) | รหัสสาขา |
| `tenant_id` | `uuid` | NO | IX (`ix_branches_tenant_company`) | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1, IX | รหัสบริษัท |
| `code` | `varchar(50)` | NO | UQ1 (`uq_branches_company_code`) | รหัสสาขา |
| `name` | `varchar(200)` | NO | — | ชื่อสาขา |
| `timezone` | `varchar(64)` | NO | — | เขตเวลา IANA (เช่น `Asia/Bangkok`) |
| `latitude` | `numeric(10,7)` | NO | — | ละติจูดจุดศูนย์กลางสาขา |
| `longitude` | `numeric(10,7)` | NO | — | ลองจิจูดจุดศูนย์กลางสาขา |
| `geofence_radius_meters` | `integer` | NO | — | รัศมี Geofence ที่อนุญาตให้เช็กอิน (เมตร) |
| `allowed_ips` | `text[]` | NO | — | รายการ IP Address ที่อนุญาตให้ลงเวลา |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 1.4 `departments` *(RLS Enabled)*
โครงสร้างแผนกแบบลำดับชั้น (Adjacency List)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_departments`) | รหัสแผนก |
| `tenant_id` | `uuid` | NO | IX (`ix_departments_tenant_company`) | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1, IX | รหัสบริษัท |
| `parent_department_id` | `uuid` | YES | FK -> `departments(id)` | แผนกแม่ (NULL หากอยู่ระดับบนสุด) |
| `code` | `varchar(50)` | NO | UQ1 (`uq_departments_company_code`) | รหัสแผนก |
| `name` | `varchar(200)` | NO | — | ชื่อแผนก |
| `head_employee_id` | `uuid` | YES | — | รหัสพนักงานที่เป็นหัวหน้าแผนก |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 1.5 `positions` *(RLS Enabled)*
ตำแหน่งงานภายในบริษัท

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_positions`) | รหัสตำแหน่ง |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1 | รหัสบริษัท |
| `code` | `varchar(50)` | NO | UQ1 (`uq_positions_company_code`) | รหัสตำแหน่ง |
| `title` | `varchar(150)` | NO | — | ชื่อตำแหน่งงาน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 1.6 `job_grades` *(RLS Enabled)*
ระดับพนักงาน (Job Grade) สำหรับผูกเงื่อนไขสิทธิ์วันลาและสายอนุมัติ

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_job_grades`) | รหัสระดับงาน |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1 | รหัสบริษัท |
| `code` | `varchar(30)` | NO | UQ1 (`uq_job_grades_company_code`) | รหัส Job Grade |
| `level_order` | `integer` | NO | — | ลำดับความอาวุโส (>= 1) |
| `name` | `varchar(100)` | NO | — | ชื่อระดับงาน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

---

## Group 2: Identity, Server Sessions & RBAC (ผู้ใช้งาน เซสชัน และสิทธิ์ — 6 ตาราง)

### 2.1 `users` *(RLS Enabled)*
บัญชีผู้ใช้งานสำหรับเข้าสู่ระบบ

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_users`) | รหัสผู้ใช้ |
| `tenant_id` | `uuid` | NO | FK -> `tenants(id)`, UQ1 | รหัส Tenant |
| `email` | `varchar(255)` | NO | UQ1 (`uq_users_tenant_email`) | อีเมลเข้าสู่ระบบ (lowercase) |
| `password_hash` | `text` | NO | — | รหัสผ่านที่แฮชแล้ว (PBKDF2-SHA256) |
| `mfa_enabled` | `boolean` | NO | — | เปิดใช้การยืนยันตัวตน 2 ชั้นหรือไม่ |
| `status` | `varchar(30)` | NO | CHECK (`ACTIVE`, `LOCKED`, `INACTIVE`) | สถานะบัญชีผู้ใช้ |
| `last_login_at` | `timestamptz` | YES | — | เวลาเข้าสู่ระบบล่าสุด (UTC) |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 2.2 `user_sessions` *(RLS Enabled)*
Server-Backed Session Store สำหรับยืนยัน HttpOnly Session Cookie และสั่ง Revoke / Force Logout ทันที

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_user_sessions`) | รหัส Session |
| `user_id` | `uuid` | NO | FK -> `users(id)`, IX (`ix_user_sessions_active`) | รหัสผู้ใช้เจ้าของ Session |
| `tenant_id` | `uuid` | NO | FK -> `tenants(id)` | รหัส Tenant |
| `session_token_hash` | `varchar(128)` | NO | UQ (`uq_user_sessions_token_hash`) | SHA-256 Hash ของ Session Token (ห้ามเก็บ Token ดิบ) |
| `active_company_id` | `uuid` | YES | — | บริษัทที่กำลังเลือกใช้งานใน Session นี้ |
| `active_branch_id` | `uuid` | YES | — | สาขาที่กำลังเลือกใช้งานใน Session นี้ |
| `ip_address` | `inet` | NO | — | IP จริงของ Client ที่สร้าง Session |
| `user_agent` | `text` | NO | — | ข้อมูล Browser/Device |
| `created_at` | `timestamptz` | NO | — | เวลาที่สร้าง Session |
| `last_accessed_at` | `timestamptz` | NO | — | เวลาใช้งานล่าสุด (Throttled update) |
| `expires_at` | `timestamptz` | NO | IX (`ix_user_sessions_active`) | เวลาหมดอายุ (Partial Index `WHERE is_revoked = false`) |
| `is_revoked` | `boolean` | NO | — | `true` เมื่อ Logout หรือถูกสั่ง Force Revoke |

### 2.3 `roles` *(RLS Enabled)*
บทบาทผู้ใช้ระดับ Tenant หรือเฉพาะบริษัท

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_roles`) | รหัส Role |
| `tenant_id` | `uuid` | NO | FK -> `tenants(id)`, UQ1 | รหัส Tenant |
| `company_id` | `uuid` | YES | FK -> `companies(id)`, UQ1 | รหัสบริษัท (NULL = ใช้ร่วมกันทั้ง Tenant) |
| `code` | `varchar(50)` | NO | UQ1 (`uq_roles_tenant_company_code`) | รหัส Role (เช่น `USER`, `OPERATION`, `HR`, `ACCOUNTING`, `ADMIN`, `IT`) |
| `name` | `varchar(120)` | NO | — | ชื่อบทบาท |
| `is_system_role` | `boolean` | NO | — | Role มาตรฐานของระบบหรือไม่ |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 2.4 `permissions`
Master Dictionary ของสิทธิ์ทั้งหมดในระบบ (Global Lookup)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_permissions`) | รหัส Permission |
| `code` | `varchar(100)` | NO | UQ (`uq_permissions_code`) | รหัสสิทธิ์ (เช่น `payroll.calculate`, `workflow.approve`) |
| `module` | `varchar(50)` | NO | — | กลุ่มโมดูล (เช่น `PORTAL`, `PAYROLL`) |
| `description` | `varchar(250)` | NO | — | คำอธิบายสิทธิ์ |

### 2.5 `role_permissions`
ตารางเชื่อมความสัมพันธ์ระหว่าง Role กับ Permission

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `role_id` | `uuid` | NO | PK, FK -> `roles(id)` | รหัส Role |
| `permission_id` | `uuid` | NO | PK, FK -> `permissions(id)` | รหัส Permission |

### 2.6 `user_company_roles` *(RLS Enabled)*
กำหนดว่า User คนใดมี Role ใดในบริษัทใด และเห็นข้อมูลได้เฉพาะสาขาไหน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_user_company_roles`) | รหัสการผูกสิทธิ์ |
| `tenant_id` | `uuid` | NO | IX (`ix_user_company_roles_tenant_company_user`) | รหัส Tenant |
| `user_id` | `uuid` | NO | FK -> `users(id)`, UQ1, IX | รหัสผู้ใช้ |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1, IX | รหัสบริษัท |
| `role_id` | `uuid` | NO | FK -> `roles(id)`, UQ1 (`uq_user_company_roles_user_comp_role`) | รหัส Role |
| `branch_scope_ids` | `uuid[]` | YES | — | รายการ ID สาขาที่เข้าถึงได้ (NULL = ทุกสาขาในบริษัท) |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

---

## Group 3: Employee Core & Lifecycle (ข้อมูลพนักงาน สัญญา และสายบังคับบัญชา — 3 ตาราง)

### 3.1 `employees` *(RLS Enabled)*
ข้อมูลหลักพนักงานแต่ละคน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_employees`) | รหัสพนักงาน |
| `tenant_id` | `uuid` | NO | IX (`ix_employees_tenant_company_dept`) | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1, IX | รหัสบริษัท |
| `branch_id` | `uuid` | NO | FK -> `branches(id)` | รหัสสาขาประจำ |
| `department_id` | `uuid` | NO | FK -> `departments(id)`, IX | รหัสแผนก |
| `position_id` | `uuid` | NO | FK -> `positions(id)` | รหัสตำแหน่ง |
| `job_grade_id` | `uuid` | NO | FK -> `job_grades(id)` | รหัสระดับพนักงาน |
| `user_id` | `uuid` | YES | FK -> `users(id)` | บัญชีผู้ใช้ที่ผูกกับพนักงาน (ถ้ามี) |
| `employee_code` | `varchar(50)` | NO | UQ1 (`uq_employees_company_code`) | รหัสพนักงานประจำบริษัท |
| `first_name` | `varchar(100)` | NO | — | ชื่อจริง |
| `last_name` | `varchar(100)` | NO | — | นามสกุล |
| `hire_date` | `date` | NO | — | วันที่เริ่มงาน |
| `probation_end_date` | `date` | YES | — | วันครบกำหนดทดลองงาน |
| `employment_status` | `varchar(30)` | NO | CHECK (`PROBATION`, `ACTIVE`, `RESIGNED`, `TERMINATED`) | สถานะการจ้างงาน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 3.2 `employment_contracts` *(RLS Enabled)*
ประวัติสัญญาจ้างและฐานเงินเดือนของพนักงาน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_employment_contracts`) | รหัสสัญญาจ้าง |
| `tenant_id` | `uuid` | NO | IX (`ix_employment_contracts_tenant_company_emp`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, IX | รหัสพนักงาน |
| `contract_type` | `varchar(40)` | NO | CHECK (`FULL_TIME`, `PART_TIME`, `CONTRACT`, `INTERN`) | ประเภทการจ้าง |
| `effective_from` | `date` | NO | — | วันที่มีผลบังคับใช้ |
| `effective_to` | `date` | YES | — | วันสิ้นสุดสัญญา (NULL = สัญญาปัจจุบัน) |
| `base_salary` | `numeric(18,4)` | NO | — | ฐานเงินเดือน |
| `payment_cycle` | `varchar(20)` | NO | CHECK (`MONTHLY`, `BI_WEEKLY`) | รอบการจ่ายเงินเดือน |
| `is_active` | `boolean` | NO | — | สัญญาที่ใช้งานอยู่ปัจจุบันหรือไม่ |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 3.3 `employee_reporting_lines` *(RLS Enabled)*
สายบังคับบัญชาของพนักงานสำหรับส่งคำขอเข้าสู่ระบบอนุมัติอัตโนมัติ

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_employee_reporting_lines`) | รหัสสายบังคับบัญชา |
| `tenant_id` | `uuid` | NO | IX (`ix_employee_reporting_lines_tenant_company_emp`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, IX | พนักงานผู้ใต้บังคับบัญชา |
| `manager_employee_id` | `uuid` | NO | FK -> `employees(id)` | พนักงานที่เป็นหัวหน้างาน |
| `line_type` | `varchar(30)` | NO | CHECK (`DIRECT`, `SECONDARY`) | ประเภทสายบังคับบัญชา |
| `priority_order` | `integer` | NO | — | ลำดับความสำคัญ |
| `effective_from` | `date` | NO | — | วันที่มีผล |
| `effective_to` | `date` | YES | — | วันสิ้นสุด |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

---

## Group 4: Time Attendance & Shift Engine (กะงานและบันทึกเวลา — 5 ตาราง)

### 4.1 `shift_definitions` *(RLS Enabled)*
นิยามรูปแบบกะงานของบริษัท

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_shift_definitions`) | รหัสกะงาน |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1 | รหัสบริษัท |
| `code` | `varchar(50)` | NO | UQ1 (`uq_shift_definitions_company_code`) | รหัสกะงาน |
| `name` | `varchar(150)` | NO | — | ชื่อกะงาน |
| `shift_type` | `varchar(30)` | NO | CHECK (`NORMAL`, `OVERNIGHT`, `FLEXIBLE`) | ประเภทกะ |
| `start_time` | `time` | NO | — | เวลาเริ่มงาน |
| `end_time` | `time` | NO | — | เวลาเลิกงาน |
| `is_overnight` | `boolean` | NO | — | เป็นกะข้ามคืนหรือไม่ |
| `grace_period_minutes` | `integer` | NO | — | นาทีผ่อนผันก่อนถือว่ามาสาย |
| `late_after_minutes` | `integer` | NO | — | เกณฑ์นับว่ามาสาย (นาที) |
| `standard_work_minutes` | `integer` | NO | — | จำนวนนาทีทำงานมาตรฐานต่อวัน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 4.2 `shift_break_rules` *(RLS Enabled)*
ช่วงเวลาพักประจำกะงาน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_shift_break_rules`) | รหัสกฎเวลาพัก |
| `tenant_id` | `uuid` | NO | IX (`ix_shift_break_rules_tenant_shift`) | รหัส Tenant |
| `shift_definition_id` | `uuid` | NO | FK -> `shift_definitions(id)`, IX | รหัสกะงาน |
| `break_start` | `time` | NO | — | เวลาเริ่มพัก |
| `break_end` | `time` | NO | — | เวลาสิ้นสุดพัก |
| `duration_minutes` | `integer` | NO | — | ระยะเวลาพัก (นาที) |
| `is_paid` | `boolean` | NO | — | นับเวลาพักรวมเป็นชั่วโมงทำงานที่จ่ายเงินหรือไม่ |

### 4.3 `employee_shift_rosters` *(RLS Enabled)*
ตารางจัดกะรายวัน (Roster Matrix) ของพนักงานแต่ละคน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_employee_shift_rosters`) | รหัสตารางกะ |
| `tenant_id` | `uuid` | NO | IX (`ix_employee_shift_rosters_tenant_company_date`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, UQ1 | รหัสพนักงาน |
| `shift_definition_id` | `uuid` | YES | FK -> `shift_definitions(id)` | กะงานที่กำหนด (NULL หากเป็นวันหยุด) |
| `work_date` | `date` | NO | UQ1 (`uq_employee_shift_rosters_emp_date`), IX | วันที่ทำงาน |
| `day_type` | `varchar(30)` | NO | CHECK (`WORKDAY`, `DAY_OFF`, `PUBLIC_HOLIDAY`) | ประเภทวัน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 4.4 `attendance_logs` *(RLS Enabled)*
Raw Log แบบ Append-only ทุกครั้งที่พนักงานลงเวลาเข้า-ออก

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_attendance_logs`) | รหัส Log การลงเวลา |
| `tenant_id` | `uuid` | NO | IX (`ix_attendance_logs_tenant_company_emp_time`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, IX | รหัสพนักงาน |
| `branch_id` | `uuid` | NO | FK -> `branches(id)` | สาขาที่ลงเวลา |
| `punch_time_utc` | `timestamptz` | NO | IX | วันเวลาที่สแกน/กดเช็กอิน (UTC) |
| `punch_type` | `varchar(20)` | NO | CHECK (`CHECK_IN`, `CHECK_OUT`) | ประเภทการลงเวลา |
| `source` | `varchar(30)` | NO | CHECK (`WEB`, `MOBILE`, `BIOMETRIC`, `ADJUSTMENT`) | ช่องทางที่ลงเวลา |
| `latitude` | `numeric(10,7)` | YES | — | พิกัดละติจูดตอนกดลงเวลา |
| `longitude` | `numeric(10,7)` | YES | — | พิกัดลองจิจูดตอนกดลงเวลา |
| `ip_address` | `inet` | NO | — | IP Address ของเครื่องที่ลงเวลา |
| `is_within_geofence` | `boolean` | NO | — | อยู่ในรัศมี Geofence / IP Whitelist หรือไม่ |

### 4.5 `attendance_daily_summaries` *(RLS Enabled)*
ตารางสรุปเวลาเข้า-ออกงานรายวันหลังประมวลผลเทียบกับกะงาน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_attendance_daily_summaries`) | รหัสสรุปรายวัน |
| `tenant_id` | `uuid` | NO | IX (`ix_attendance_daily_tenant_company_date`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, UQ1 | รหัสพนักงาน |
| `work_date` | `date` | NO | UQ1 (`uq_attendance_daily_summaries_emp_date`), IX | วันที่ทำงาน |
| `shift_definition_id` | `uuid` | YES | FK -> `shift_definitions(id)` | กะงานที่ใช้อ้างอิง |
| `actual_in_utc` | `timestamptz` | YES | — | เวลาเข้างานจริง (UTC) |
| `actual_out_utc` | `timestamptz` | YES | — | เวลาออกงานจริง (UTC) |
| `worked_minutes` | `integer` | NO | — | นาทีทำงานจริง |
| `late_minutes` | `integer` | NO | — | นาทีที่มาสาย |
| `early_out_minutes` | `integer` | NO | — | นาทีที่ออกก่อนเวลา |
| `ot_minutes` | `integer` | NO | — | นาที OT ที่อนุมัติแล้ว |
| `attendance_status` | `varchar(30)` | NO | CHECK (`PRESENT`, `LATE`, `EARLY_OUT`, `MISSING_PUNCH`, `ABSENT`, `ON_LEAVE`, `DAY_OFF`) | สถานะประจำวัน |
| `is_locked_by_payroll` | `boolean` | NO | — | ล็อกไม่ให้แก้หลังตัดรอบเงินเดือนแล้ว |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

---

## Group 5: Leave, OT & Multi-Step Workflow Engine (วันลาและสายอนุมัติ — 7 ตาราง)

### 5.1 `leave_types` *(RLS Enabled)*
ประเภทวันลาของบริษัท (Data-Driven ไม่มี Hardcode)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_leave_types`) | รหัสประเภทการลา |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1 | รหัสบริษัท |
| `code` | `varchar(50)` | NO | UQ1 (`uq_leave_types_company_code`) | รหัสประเภทลา (เช่น `ANNUAL`, `SICK`) |
| `name` | `varchar(150)` | NO | — | ชื่อประเภทการลา |
| `is_paid` | `boolean` | NO | — | ได้รับค่าจ้างระหว่างลาหรือไม่ |
| `allow_half_day` | `boolean` | NO | — | อนุญาตให้ลาครึ่งวันหรือไม่ |
| `allow_hourly` | `boolean` | NO | — | อนุญาตให้ลารายชั่วโมงหรือไม่ |
| `requires_attachment` | `boolean` | NO | — | บังคับแนบเอกสาร (เช่น ใบรับรองแพทย์) หรือไม่ |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 5.2 `leave_entitlement_policies` *(RLS Enabled)*
นโยบายคำนวณสิทธิ์วันลาตามอายุงานและระดับพนักงาน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_leave_entitlement_policies`) | รหัสนโยบายสิทธิ์วันลา |
| `tenant_id` | `uuid` | NO | IX (`ix_leave_policies_tenant_company_type`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `leave_type_id` | `uuid` | NO | FK -> `leave_types(id)`, IX | รหัสประเภทวันลา |
| `job_grade_id` | `uuid` | YES | FK -> `job_grades(id)` | ระดับพนักงานเฉพาะ (NULL = ใช้ทุก Grade) |
| `min_tenure_months` | `integer` | NO | — | อายุงานขั้นต่ำ (เดือน) |
| `max_tenure_months` | `integer` | YES | — | อายุงานสูงสุด (เดือน) |
| `annual_quota_days` | `numeric(10,2)` | NO | — | จำนวนวันลาที่ได้ต่อปี |
| `prorate_rule` | `varchar(30)` | NO | CHECK (`NONE`, `MONTHLY_PRORATE`, `DAILY_PRORATE`) | สูตรเฉลี่ยวันลาพนักงานเข้าใหม่ |
| `max_carry_over_days` | `numeric(10,2)` | NO | — | จำนวนวันสูงสุดที่ยกยอดข้ามปีได้ |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 5.3 `employee_leave_balances` *(RLS Enabled)*
ยอดวันลาคงเหลือรายปีของพนักงานแต่ละคน

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_employee_leave_balances`) | รหัสยอดวันลา |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `company_id` | `uuid` | NO | — | รหัสบริษัท |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, UQ1 | รหัสพนักงาน |
| `leave_type_id` | `uuid` | NO | FK -> `leave_types(id)`, UQ1 | ประเภทวันลา |
| `quota_year` | `integer` | NO | UQ1 (`uq_employee_leave_balances_emp_type_year`) | ปีโควตา (ค.ศ.) |
| `carried_over_days` | `numeric(10,2)` | NO | — | วันลาที่ยกยอดมาจากปีก่อน |
| `entitled_days` | `numeric(10,2)` | NO | — | สิทธิ์วันลาประจำปี |
| `used_days` | `numeric(10,2)` | NO | — | วันลาที่ใช้และอนุมัติแล้ว |
| `pending_days` | `numeric(10,2)` | NO | — | วันลาที่ยื่นขอและกำลังรออนุมัติ |
| `xmin` | `xid` *(Concurrency)* | NO | RowVersion | ป้องกัน Race Condition ตัดยอดวันลาซ้อน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 5.4 `workflow_definitions` *(RLS Enabled)*
แม่แบบสายอนุมัติแยกตามประเภทคำขอและแผนก

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_workflow_definitions`) | รหัสแม่แบบสายอนุมัติ |
| `tenant_id` | `uuid` | NO | IX (`ix_workflow_definitions_tenant_company_type`) | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, IX | รหัสบริษัท |
| `request_type` | `varchar(40)` | NO | CHECK (`LEAVE`, `OVERTIME`, `TIME_ADJUSTMENT`), IX | ประเภทคำขอ |
| `department_id` | `uuid` | YES | FK -> `departments(id)` | แผนกเฉพาะ (NULL = ใช้ทั้งบริษัท) |
| `name` | `varchar(150)` | NO | — | ชื่อสายอนุมัติ |
| `is_active` | `boolean` | NO | — | เปิดใช้งานอยู่หรือไม่ |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 5.5 `workflow_steps` *(RLS Enabled)*
ลำดับขั้นตอนผู้อนุมัติในแต่ละแม่แบบ

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_workflow_steps`) | รหัสขั้นตอนอนุมัติ |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `workflow_definition_id` | `uuid` | NO | FK -> `workflow_definitions(id)`, UQ1 | รหัสแม่แบบ |
| `step_order` | `integer` | NO | UQ1 (`uq_workflow_steps_def_order`) | ลำดับขั้น (1, 2, 3, ...) |
| `approver_strategy` | `varchar(40)` | NO | CHECK (`DIRECT_MANAGER`, `SECONDARY_MANAGER`, `HR_ROLE`, `SPECIFIC_ROLE`, `SPECIFIC_EMPLOYEE`) | วิธีหาผู้อนุมัติ |
| `specific_role_id` | `uuid` | YES | FK -> `roles(id)` | ระบุ Role ผู้อนุมัติ (ถ้าเลือก `SPECIFIC_ROLE`) |
| `specific_employee_id` | `uuid` | YES | FK -> `employees(id)` | ระบุตัวบุคคล (ถ้าเลือก `SPECIFIC_EMPLOYEE`) |

### 5.6 `approval_requests` *(RLS Enabled)*
State Machine กลางสำหรับคำขอลา, OT, และขอแก้ไขเวลา

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_approval_requests`) | รหัสคำขออนุมัติ |
| `tenant_id` | `uuid` | NO | IX (`ix_approval_requests_tenant_company_status`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `requester_employee_id` | `uuid` | NO | FK -> `employees(id)` | พนักงานผู้ยื่นคำขอ |
| `request_type` | `varchar(40)` | NO | — | ประเภทคำขอ (`LEAVE`, `OVERTIME`, `TIME_ADJUSTMENT`) |
| `reference_id` | `uuid` | NO | — | รหัสอ้างอิงเอกสารคำขอ |
| `workflow_definition_id` | `uuid` | NO | FK -> `workflow_definitions(id)` | สายอนุมัติที่ใช้ |
| `current_step_order` | `integer` | NO | — | ขั้นตอนปัจจุบันที่รออนุมัติ |
| `status` | `varchar(30)` | NO | CHECK (`PENDING`, `APPROVED`, `REJECTED`, `CANCELLED`), IX | สถานะคำขอ |
| `payload_summary` | `jsonb` | NO | — | สรุปรายละเอียดคำขอ (วัน/เวลา/จำนวนวัน) |
| `submitted_at` | `timestamptz` | NO | IX | เวลายื่นคำขอ (UTC) |
| `completed_at` | `timestamptz` | YES | — | เวลาปิดคำขอ (UTC) |
| `xmin` | `xid` *(Concurrency)* | NO | RowVersion | ป้องกันการกดอนุมัติซ้อนพร้อมกัน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 5.7 `approval_action_logs` *(RLS Enabled)*
ประวัติการกดอนุมัติ ปฏิเสธ หรือยกเลิกในแต่ละขั้นตอน (Immutable)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_approval_action_logs`) | รหัส Action Log |
| `tenant_id` | `uuid` | NO | IX (`ix_approval_action_logs_tenant_request`) | รหัส Tenant |
| `approval_request_id` | `uuid` | NO | FK -> `approval_requests(id)`, IX | รหัสคำขออนุมัติ |
| `step_order` | `integer` | NO | — | ลำดับขั้นที่ดำเนินการ |
| `actor_employee_id` | `uuid` | NO | FK -> `employees(id)` | ผู้กดดำเนินการ |
| `action` | `varchar(30)` | NO | CHECK (`APPROVE`, `REJECT`, `CANCEL`) | การกระทำ |
| `comment` | `text` | YES | — | เหตุผล/หมายเหตุประกอบ |
| `acted_at` | `timestamptz` | NO | — | วันเวลาที่ดำเนินการ (UTC) |

---

## Group 6: Payroll, Tax & SSO Engine (เงินเดือน ภาษี และประกันสังคม — 6 ตาราง)

### 6.1 `payroll_periods` *(RLS Enabled)*
งวดการจ่ายเงินเดือนประจำรอบบัญชี

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_payroll_periods`) | รหัสงวดเงินเดือน |
| `tenant_id` | `uuid` | NO | IX (`ix_payroll_periods_tenant_company_start`) | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1, IX | รหัสบริษัท |
| `period_code` | `varchar(30)` | NO | UQ1 (`uq_payroll_periods_company_code`) | รหัสงวด (เช่น `2026-11`) |
| `period_start` | `date` | NO | IX | วันเริ่มต้นงวด |
| `period_end` | `date` | NO | — | วันสิ้นสุดงวด |
| `attendance_cutoff_date` | `date` | NO | — | วันตัดรอบเวลาเข้า-ออกงาน/ลา/OT |
| `pay_date` | `date` | NO | — | วันที่จ่ายเงินเดือน |
| `status` | `varchar(30)` | NO | CHECK (`DRAFT`, `CALCULATED`, `APPROVED`, `LOCKED`) | สถานะงวดเงินเดือน |
| `xmin` | `xid` *(Concurrency)* | NO | RowVersion | ป้องกันการคำนวณ/Lock งวดชนกัน |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 6.2 `salary_components` *(RLS Enabled)*
ประเภทรายได้และรายการหักแบบ Dynamic

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_salary_components`) | รหัสรายการเงินเดือน |
| `tenant_id` | `uuid` | NO | — | รหัส Tenant |
| `company_id` | `uuid` | NO | FK -> `companies(id)`, UQ1 | รหัสบริษัท |
| `code` | `varchar(50)` | NO | UQ1 (`uq_salary_components_company_code`) | รหัสรายการ (เช่น `BASE_SALARY`, `OT_1_5X`) |
| `name` | `varchar(150)` | NO | — | ชื่อรายการ |
| `component_type` | `varchar(20)` | NO | CHECK (`EARNING`, `DEDUCTION`) | ประเภท (`EARNING` / `DEDUCTION`) |
| `is_taxable` | `boolean` | NO | — | นำไปคำนวณภาษีเงินได้หรือไม่ |
| `is_sso_base` | `boolean` | NO | — | นำไปรวมเป็นฐานประกันสังคมหรือไม่ |
| `calculation_formula_type` | `varchar(40)` | NO | — | ประเภทสูตรคำนวณ (`FIXED`, `OVERTIME_HOURLY`, `ATTENDANCE_DEDUCTION`, `CUSTOM_AMOUNT`) |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 6.3 `tax_brackets`
ตารางขั้นบันไดภาษีตามกฎหมาย (Global Statutory Table — ไม่มี Hardcode ใน C#)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_tax_brackets`) | รหัสขั้นบันไดภาษี |
| `tax_year` | `integer` | NO | IX (`ix_tax_brackets_country_year`) | ปีภาษี (ค.ศ.) |
| `country_code` | `varchar(5)` | NO | IX | รหัสประเทศ (เช่น `TH`) |
| `min_net_income` | `numeric(18,4)` | NO | IX | เงินได้สุทธิเริ่มต้นของขั้นนี้ |
| `max_net_income` | `numeric(18,4)` | YES | — | เงินได้สุทธิสูงสุดของขั้นนี้ (NULL = ขั้นสูงสุด) |
| `tax_rate_percent` | `numeric(7,4)` | NO | — | อัตราภาษี (%) |

### 6.4 `sso_configurations`
ตารางอัตราและเพดานประกันสังคมตามช่วงวันที่บังคับใช้ (Global Statutory Table)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_sso_configurations`) | รหัสการตั้งค่าประกันสังคม |
| `country_code` | `varchar(5)` | NO | IX (`ix_sso_configurations_country_effective`) | รหัสประเทศ (เช่น `TH`) |
| `effective_from` | `date` | NO | IX | วันที่มีผลบังคับใช้ |
| `effective_to` | `date` | YES | — | วันสิ้นสุด (NULL = ปัจจุบัน) |
| `employee_rate_percent` | `numeric(7,4)` | NO | — | อัตราสมทบฝั่งลูกจ้าง (%) |
| `employer_rate_percent` | `numeric(7,4)` | NO | — | อัตราสมทบฝั่งนายจ้าง (%) |
| `min_wage_base` | `numeric(18,4)` | NO | — | ฐานค่าจ้างขั้นต่ำ |
| `max_wage_base` | `numeric(18,4)` | NO | — | ฐานค่าจ้างสูงสุด (เพดานประกันสังคม) |

### 6.5 `payslips` *(RLS Enabled)*
ผลลัพธ์สลิปเงินเดือนรายพนักงานต่องวด

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_payslips`) | รหัสสลิปเงินเดือน |
| `tenant_id` | `uuid` | NO | IX (`ix_payslips_tenant_company_period`) | รหัส Tenant |
| `company_id` | `uuid` | NO | IX | รหัสบริษัท |
| `payroll_period_id` | `uuid` | NO | FK -> `payroll_periods(id)`, UQ1, IX | รหัสงวดเงินเดือน |
| `employee_id` | `uuid` | NO | FK -> `employees(id)`, UQ1 (`uq_payslips_period_emp`) | รหัสพนักงาน |
| `gross_income` | `numeric(18,4)` | NO | — | รายได้รวมก่อนหัก |
| `total_deductions` | `numeric(18,4)` | NO | — | รายการหักรวมทั้งหมด |
| `sso_employee_amount` | `numeric(18,4)` | NO | — | เงินสมทบประกันสังคมฝั่งพนักงาน |
| `withholding_tax_amount` | `numeric(18,4)` | NO | — | ภาษีหัก ณ ที่จ่าย (ภ.ง.ด.1) |
| `net_pay` | `numeric(18,4)` | NO | — | เงินได้สุทธิ |
| `is_locked` | `boolean` | NO | — | ล็อกสลิปถาวรเมื่อปิดงวด |
| *Audit Columns* | *6 columns* | — | — | Standard Audit + Soft Delete |

### 6.6 `payslip_items` *(RLS Enabled)*
รายละเอียดแต่ละบรรทัดในสลิปเงินเดือน (เก็บ Snapshot ชื่อรายการเพื่อป้องกันข้อมูลย้อนหลังเปลี่ยน)

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_payslip_items`) | รหัสรายการย่อยในสลิป |
| `tenant_id` | `uuid` | NO | IX (`ix_payslip_items_tenant_payslip`) | รหัส Tenant |
| `payslip_id` | `uuid` | NO | FK -> `payslips(id)`, IX | รหัสสลิปเงินเดือน |
| `salary_component_id` | `uuid` | NO | FK -> `salary_components(id)` | รหัสประเภทรายการเงินเดือน |
| `component_name_snapshot` | `varchar(150)` | NO | — | ชื่อรายการ ณ เวลาคำนวณงวด |
| `amount` | `numeric(18,4)` | NO | — | จำนวนเงิน |
| `units` | `numeric(10,2)` | YES | — | จำนวนหน่วย (เช่น ชั่วโมง OT หรือวัน) |

---

## Group 7: Audit & Compliance (บันทึกการตรวจสอบย้อนหลัง — 1 ตาราง)

### 7.1 `audit_logs` *(RLS Enabled)*
บันทึกประวัติการเปลี่ยนแปลงข้อมูลสำคัญทั้งหมดในระบบ

| Column | PostgreSQL Type | Nullable | Key / Constraint | คำอธิบาย |
|---|---|---|---|---|
| `id` | `uuid` | NO | PK (`pk_audit_logs`) | รหัส Audit Log |
| `tenant_id` | `uuid` | NO | IX (`ix_audit_logs_tenant_entity_occurred`) | รหัส Tenant |
| `company_id` | `uuid` | YES | — | รหัสบริษัทที่เกิดเหตุการณ์ |
| `actor_user_id` | `uuid` | YES | — | รหัสผู้ใช้ที่ทำการแก้ไข |
| `entity_name` | `varchar(100)` | NO | IX | ชื่อ Entity / Table ที่ถูกกระทำ |
| `entity_id` | `uuid` | NO | IX | รหัส Record ที่ถูกกระทำ |
| `action` | `varchar(30)` | NO | — | การกระทำ (`INSERT`, `UPDATE`, `DELETE`, `LOCK`, ฯลฯ) |
| `old_values` | `jsonb` | YES | — | Snapshot ค่าก่อนแก้ไข |
| `new_values` | `jsonb` | YES | — | Snapshot ค่าหลังแก้ไข |
| `ip_address` | `inet` | YES | — | IP จริงของ Client |
| `correlation_id` | `varchar(64)` | NO | — | Request Correlation ID สำหรับ Trace Log |
| `occurred_at` | `timestamptz` | NO | IX | วันเวลาที่เกิดเหตุการณ์ (UTC) |

---

## 2. สรุป Indexes สำคัญและตารางที่เปิด Row-Level Security (RLS)

### Key Composite & Filtered Indexes
- `ix_employees_tenant_company_dept` ON `employees (tenant_id, company_id, department_id)`
- `ix_attendance_daily_tenant_company_date` ON `attendance_daily_summaries (tenant_id, company_id, work_date)`
- `ix_approval_requests_tenant_company_status` ON `approval_requests (tenant_id, company_id, status, submitted_at)`
- `uq_user_sessions_token_hash` ON `user_sessions (session_token_hash)` (Unique)
- `ix_user_sessions_active` ON `user_sessions (user_id, expires_at) WHERE is_revoked = false` (Partial Index)

### RLS Policy Scope (29 Tenant-Scoped Tables)
เปิด `ENABLE ROW LEVEL SECURITY` และสร้าง `tenant_isolation_policy` บนตารางที่มี `tenant_id` ทั้งหมด 29 ตาราง (ยกเว้น 5 ตารางระดับ Global/Root คือ `tenants`, `permissions`, `role_permissions`, `tax_brackets`, `sso_configurations`):
`companies`, `branches`, `departments`, `positions`, `job_grades`, `users`, `user_sessions`, `roles`, `user_company_roles`, `employees`, `employment_contracts`, `employee_reporting_lines`, `shift_definitions`, `shift_break_rules`, `employee_shift_rosters`, `attendance_logs`, `attendance_daily_summaries`, `leave_types`, `leave_entitlement_policies`, `employee_leave_balances`, `workflow_definitions`, `workflow_steps`, `approval_requests`, `approval_action_logs`, `payroll_periods`, `salary_components`, `payslips`, `payslip_items`, `audit_logs`
