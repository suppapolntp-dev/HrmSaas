# HrmSaas — Design System & UI/UX Specification

> **Product:** Empeo-Style Multi-Tenant Cloud HRMS SaaS (`HrmSaas`)  
> **Visual Direction:** Enterprise High-Density SaaS (**Clean, Sharp, Calm, Zero-Clutter**)  
> **Source Alignment:** Derived strictly from [`plan/05-ux-ui-design-rules.md`](../plan/05-ux-ui-design-rules.md), [`plan/01-frontend-angular.md`](../plan/01-frontend-angular.md), and [`plan/06-testing-and-api-toggle.md`](../plan/06-testing-and-api-toggle.md).

---

## 1. Visual Philosophy & Core Mandates

ระบบ B2B HRMS เป็นซอฟต์แวร์ที่ฝ่าย HR, บัญชี และผู้จัดการสาขาต้องเปิดใช้งานต่อเนื่องหลายชั่วโมงต่อวัน หน้าจอจึงต้องลดความล้าของสายตา จัดระเบียบตัวเลขและตารางข้อมูลหลายสิบคอลัมน์ให้กวาดสายตาได้เร็ว และปราศจากองค์ประกอบตกแต่งเกินจำเป็น

1. **60-30-10 Color Architecture (Light & Dark Themes):**
   - **60% Surface & Elevation:** โหมดสว่างใช้โทน Slate สะอาด (`#f8fafc` / `#ffffff`) ส่วนโหมดมืดใช้ Material Surface Elevation Scale 10 ระดับจาก Figma (`#121212`, `#1e1e1e`, `#232323`, `#252525`, `#272727`, `#2c2c2c`, `#2e2e2e`, `#333333`, `#363636`, `#383838`) เพื่อแยกชั้นความลึกโดยไม่ต้องพึ่งเส้นขอบหนา
   - **30% Typography & Structural Borders:** ตัวอักษรคอนทราสต์สูง (`#0f172a` บน Light / `#f5f5f5` บน Dark) คู่กับเส้นขอบบาง `1px` (`#e2e8f0` / `#2e2e2e`)
   - **10% Brand & Interactive Actions (Sunset Orange):** ชุดสีส้ม Sunset Orange (`#f97316`, `#fb923c`, `#fff7ed`) ใช้กับปุ่ม Primary CTA, Focus Ring และรายการที่ถูกเลือก (Selected) เพื่อคุมจุดโฟกัสของสายตา
2. **Strict "No Badge" Policy (กฎเหล็กห้ามใช้ Badge / Pill เด็ดขาด):**
   - ห้ามใช้กล่องสีพื้นหลังทึบหรือพาสเทลขอบมนล้อมรอบข้อความสถานะในตารางและ Card โดยเด็ดขาด
   - สื่อสารสถานะด้วย 3 รูปแบบมาตรฐานเท่านั้น: **Status Dot `6px × 6px` + Semantic Text**, **Row/Card Left-Border Accent (`3px`)**, และ **Typography Weight & Contrast Hierarchy**
3. **Zero Random Variables & Zero Raw Hex:**
   - ห้ามเขียนค่าสีดิบ (`#...`, `rgb(...)`, `hsl(...)`), Inline `style="..."` ที่กำหนดสี/ระยะห่างเฉพาะกิจ หรือสร้าง CSS Variable เฉพาะหน้าใน `.component.css` / `.component.scss`
   - ทุก Component ต้องใช้ตัวแปรจาก `src/styles/_tokens.css` และคลาสกลางจาก `src/styles/_utilities.css` เท่านั้น
4. **8pt Spatial Grid & High-Density Ergonomics:**
   - ระยะห่างทั้งหมดยึดฐาน 4px/8px (`4px, 8px, 12px, 16px, 24px, 32px, 48px`) เพื่อให้จังหวะช่องไฟสม่ำเสมอทั้ง 22 หน้าจอ

---

## 2. Typography & Font System (ระบบฟอนต์และลำดับชั้นตัวอักษร)

### 2.1 Font Stack & Google Fonts Import

| Token | Font Stack | บทบาทและเหตุผลในการเลือกใช้ |
|---|---|---|
| `--font-sans` | `'Inter', 'Anuphan', system-ui, -apple-system, sans-serif` | **`Inter`** ให้ความคมชัดของตัวอักษรอังกฤษและตัวเลขในขนาดเล็ก (`12px–14px`) ขณะที่ **`Anuphan`** เป็นฟอนต์ไทยไร้หัวแบบ Humanist-Geometric ที่โปร่ง นุ่มนวล ใกล้เคียงสไตล์ `emsume` และสระไม่ชนขอบในแถวตารางสูง `44px–48px` |
| `--font-mono` | `'JetBrains Mono', ui-monospace, monospace` | ใช้สำหรับรหัสพนักงาน (`EMP-0001`), Correlation ID, IP Address, JSON Snapshot (`Old` vs `New`) ใน Audit Logs และโค้ดอ้างอิง |

**HTML `<head>` Snippet สำหรับ `frontend/src/index.html`:**

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Anuphan:wght@400;500;600&family=Inter:wght@400;500;600&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
```

### 2.2 Typography Scale & Weights

| Token | Size (`rem` / `px`) | Line Height | Default Weight | การใช้งานมาตรฐานใน HrmSaas |
|---|---|---|---|---|
| `--text-xs` | `0.75rem` (`12px`) | `1rem` (`16px`) | `500` | หัวคอลัมน์ตาราง `<th>`, Timestamp, Helper Text, Metadata Captions |
| `--text-sm` | `0.875rem` (`14px`) | `1.25rem` (`20px`) | `400` / `500` | **ขนาดมาตรฐานของแอป (Default UI Body)**: เซลล์ตาราง, Form Inputs, Sidebar Links, ปุ่มกด |
| `--text-base` | `1rem` (`16px`) | `1.5rem` (`24px`) | `500` / `600` | หัวข้อ Card, หัวข้อหมวดใน Slide-over Drawer, Form Section Titles |
| `--text-lg` | `1.125rem` (`18px`) | `1.75rem` (`28px`) | `600` | หัวข้อรองของหน้า, Modal Title, ชื่อพนักงานบนหน้า Profile 360 |
| `--text-xl` | `1.25rem` (`20px`) | `1.75rem` (`28px`) | `600` | ชื่อหน้าหลัก (Page Title), ตัวเลข KPI สำคัญ, ยอดสรุปเงินเดือนสุทธิ |

**Font Weight Tokens:**
- `--font-weight-regular: 400` — ข้อความทั่วไปในตาราง, ข้อมูลรอง, สถานะ Inactive/Locked/Archived
- `--font-weight-medium: 500` — รายการ Active, Navigation Link, Form Label, ข้อความสถานะใน `<app-status-indicator>`
- `--font-weight-semibold: 600` — หัวข้อหน้า, หัวข้อ Section, ยอดรวมทางการเงิน, ปุ่ม Primary CTA

### 2.3 Numeric Discipline (`.tabular-nums` Mandate)

ทุกตัวเลขที่ต้องเปรียบเทียบในแนวตั้ง **ต้อง** ใส่คลาส `.tabular-nums` (`font-variant-numeric: tabular-nums;`) เพื่อล็อกความกว้างของตัวเลข `0–9` ให้เท่ากันทุกหลัก:
- **เวลาและกะงาน:** `08:30`, `17:30`, สาย `15` นาที, OT `2.50` ชม.
- **วันลาและโควตา:** สิทธิ์ `6.00`, ใช้ไป `1.50`, คงเหลือ `4.50` วัน
- **เงินเดือน ภาษี และประกันสังคม (`numeric(18,4)`):** จัดชิดขวา (`text-align: right`) เสมอ เช่น `45,000.00`, `750.00`, `1,240.50`
- **รหัสพนักงานและรหัสงวด:** ใช้ `--font-mono` คู่กับ `.tabular-nums`

---

## 3. Color System — 60-30-10 Token Architecture (Light & Dark Themes)

ใช้ 3-Tier Semantic Token Convention (`--<category>-<property>-<variant>`) ทั้งระบบ โดยสลับโหมดสว่าง/มืดผ่าน Attribute `data-theme="dark"` บน `<html>` (`:root[data-theme='dark']`)

### 3.1 60% Surface & Elevation Tokens (พื้นหลังและเลเยอร์)

| Token | Light Mode (`:root`) | Dark Mode (`[data-theme='dark']`) | บทบาทและการใช้งาน |
|---|---|---|---|
| `--surface-page` | `#f8fafc` | `#121212` | พื้นหลังหลักของหน้าจอ (0dp Base Canvas จาก Figma) |
| `--surface-card` | `#ffffff` | `#1e1e1e` | พื้นหลัง Card, Sidebar, Data Table Wrapper, กล่องฟอร์มหลัก (1dp) |
| `--surface-elevated` | `#ffffff` | `#333333` | พื้นหลัง Slide-over Drawer, Confirm Dialog, Modal Overlay (12dp) |
| `--surface-subtle` | `#f1f5f9` | `#232323` | พื้นหลังหัวตาราง `<thead>`, แถบ Filter Bar, และสีฐานของ Skeleton Loading (2dp) |
| `--surface-hover` | `#f1f5f9` | `#252525` | พื้นหลังแถวตาราง `<tr>` หรือรายการ Sidebar เมื่อ Hover (3dp) |
| `--surface-selected` | `#fff7ed` | `rgba(249, 115, 22, 0.14)` | พื้นหลังแถวที่ถูกเลือก, เมนูหน้าปัจจุบันใน Sidebar, หรือช่องที่เลือกใน Roster Matrix |
| `--surface-overlay` | `rgba(15, 23, 42, 0.45)` | `rgba(0, 0, 0, 0.65)` | Backdrop โปร่งแสงด้านหลัง Slide-over Drawer และ Confirm Dialog |

### 3.2 30% Text & Structural Border Tokens (ตัวอักษรและเส้นแบ่ง)

| Token | Light Mode | Dark Mode | บทบาทและการใช้งาน |
|---|---|---|---|
| `--text-primary` | `#0f172a` | `#f5f5f5` | ข้อความหลัก, ชื่อพนักงาน, หัวข้อหน้า, ยอดเงินสุทธิ |
| `--text-secondary` | `#334155` | `#d4d4d4` | ข้อมูลรองในตาราง (แผนก, ตำแหน่ง, สาขา), คำอธิบายฟอร์ม |
| `--text-muted` | `#64748b` | `#a3a3a3` | หัวคอลัมน์ตาราง, Placeholder, Timestamp, ข้อมูลที่ถูก Lock/Archived |
| `--text-inverse` | `#ffffff` | `#ffffff` | ข้อความสีขาวบนพื้นหลังปุ่มหลัก `--action-primary-bg` |
| `--text-brand` | `#c2410c` | `#fb923c` | ข้อความเน้นของเมนูที่ถูกเลือกคู่กับ `--surface-selected` |
| `--border-subtle` | `#e2e8f0` | `#2e2e2e` | เส้นคั่นแถวตาราง `1px solid`, ขอบ Card, เส้นแบ่ง Sidebar และ Top Header |
| `--border-strong` | `#cbd5e1` | `#383838` | ขอบช่อง Input/Select สถานะปกติ และเส้นแบ่งแถวสรุปยอดรวม |
| `--border-focus` | `#f97316` | `#fb923c` | ขอบและ Focus Ring `2px solid` สำหรับ Keyboard Navigation |

### 3.3 10% Brand & Interactive Action Tokens (Sunset Orange)

| Token | Light Mode | Dark Mode | บทบาทและการใช้งาน |
|---|---|---|---|
| `--action-primary-bg` | `#f97316` | `#f97316` | พื้นหลังปุ่ม Primary CTA (บันทึก, เช็กอิน, อนุมัติ, คำนวณเงินเดือน) |
| `--action-primary-hover` | `#fb923c` | `#fb923c` | พื้นหลังปุ่ม Primary CTA เมื่อเอาเมาส์ชี้ |
| `--action-primary-text` | `#ffffff` | `#ffffff` | ตัวอักษรบนปุ่ม Primary CTA |
| `--action-secondary-bg` | `#ffffff` | `#272727` | พื้นหลังปุ่มรอง (Filter, Export, ยกเลิก, ปุ่ม `API: ON / OFF`) ใช้คู่กับขอบ `1px solid var(--border-strong)` |
| `--action-secondary-hover` | `#f8fafc` | `#2c2c2c` | พื้นหลังปุ่มรองเมื่อเอาเมาส์ชี้ |
| `--action-secondary-text` | `#0f172a` | `#f5f5f5` | ตัวอักษรบนปุ่มรอง |

### 3.4 Semantic Status Tokens (สีสื่อสารสถานะ — ห้ามใช้ทำ Badge Background)

| Token | Light Mode | Dark Mode | ความหมายและการใช้งาน |
|---|---|---|---|
| `--status-success-fg` | `#15803d` | `#4ade80` | สถานะสำเร็จ / ปกติ / ใช้งานอยู่ (`ACTIVE`, `APPROVED`, `PRESENT`, `ON_TIME`, `API: ON`) |
| `--status-warning-fg` | `#b45309` | `#fbbf24` | สถานะรอตรวจสอบ / เตือน (`PENDING`, `LATE`, `EARLY_OUT`, `MISSING_PUNCH`, `CALCULATED`) |
| `--status-danger-fg` | `#b91c1c` | `#f87171` | สถานะผิดปกติ / ปฏิเสธ / ข้อผิดพลาด (`REJECTED`, `ABSENT`, `SUSPENDED`, `REVOKED`, `API: OFF`, Error Banner) |
| `--status-info-fg` | `#0369a1` | `#38bdf8` | สถานะข้อมูลเพิ่มเติม / อยู่ระหว่างดำเนินการเฉพาะ (`ON_LEAVE`, `OVERTIME`, `TRIAL`) |
| `--status-neutral-fg` | `#64748b` | `#a3a3a3` | สถานะกลาง / ปิดรอบถาวร / ไม่เคลื่อนไหว (`DRAFT`, `LOCKED`, `CANCELLED`, `DAY_OFF`, `RESIGNED`) |

---

## 4. Strict "No Badge" Policy & Status Mapping

### 4.1 3 รูปแบบที่อนุญาตสำหรับแสดงสถานะ

1. **Status Dot + Semantic Text (`<app-status-indicator>`):**
   - จุดกลม `6px × 6px` (`border-radius: var(--radius-full)`) คู่กับข้อความ ระยะห่าง `var(--space-2)` (`8px`)
   - พื้นหลังโปร่งใส (`background: transparent`) ไม่มีเส้นขอบ ไม่มีกล่องสีล้อมรอบ
   - ใช้ร่วมกับปุ่ม `API: ON / OFF` บน Top Utility Header ด้วย
2. **Row / Card Left-Border Accent:**
   - ใช้ `border-left: 3px solid var(--status-warning-fg)` หรือ `var(--status-danger-fg)` บนแถวตาราง/รายการใน Approval Inbox เพื่อดึงความสนใจไปยังเคสสาย, ขาดงาน, ลืมสแกน หรือรายการเร่งด่วน
3. **Typography Weight & Contrast Hierarchy:**
   - รายการ Active/สำคัญ: `font-weight: var(--font-weight-medium)` + `color: var(--text-primary)`
   - รายการ Inactive/Locked/Archived: `font-weight: var(--font-weight-regular)` + `color: var(--text-muted)`

### 4.2 Domain Status Mapping Matrix

| Domain | Backend Code (`varchar`) | Semantic Token | ตัวอย่างคำแสดงผล (UI Label) | รูปแบบเพิ่มเติม |
|---|---|---|---|---|
| **Employee** | `ACTIVE` | `--status-success-fg` | ปฏิบัติงาน (Active) | `font-weight: 500` |
| **Employee** | `PROBATION` | `--status-info-fg` | ทดลองงาน (Probation) | — |
| **Employee** | `RESIGNED` / `TERMINATED` | `--status-neutral-fg` | ลาออก / พ้นสภาพ | ชื่อแถวใช้ `--text-muted` |
| **Attendance** | `PRESENT` / `ON_TIME` | `--status-success-fg` | ปกติ (On Time) | — |
| **Attendance** | `LATE` / `EARLY_OUT` | `--status-warning-fg` | มาสาย / ออกก่อนเวลา | `border-left: 3px solid var(--status-warning-fg)` |
| **Attendance** | `MISSING_PUNCH` | `--status-warning-fg` | ลืมลงเวลา | `border-left: 3px solid var(--status-warning-fg)` |
| **Attendance** | `ABSENT` | `--status-danger-fg` | ขาดงาน (Absent) | `border-left: 3px solid var(--status-danger-fg)` |
| **Attendance** | `ON_LEAVE` | `--status-info-fg` | ลางาน (On Leave) | — |
| **Workflow** | `PENDING` | `--status-warning-fg` | รออนุมัติ (Pending) | — |
| **Workflow** | `APPROVED` | `--status-success-fg` | อนุมัติแล้ว (Approved) | — |
| **Workflow** | `REJECTED` | `--status-danger-fg` | ไม่อนุมัติ (Rejected) | — |
| **Workflow** | `CANCELLED` | `--status-neutral-fg` | ยกเลิกแล้ว (Cancelled) | ข้อความใช้ `--text-muted` |
| **Payroll** | `DRAFT` | `--status-neutral-fg` | ร่าง (Draft) | — |
| **Payroll** | `CALCULATED` | `--status-warning-fg` | คำนวณแล้ว (Calculated) | — |
| **Payroll** | `APPROVED` | `--status-success-fg` | อนุมัติงวด (Approved) | — |
| **Payroll** | `LOCKED` | `--status-neutral-fg` | ปิดงวดถาวร (Locked) | ปิดปุ่มแก้ไข, ข้อความ `--text-muted` |
| **Dev Tool** | `API: ON` / `API: OFF` | `--status-success-fg` / `--status-danger-fg` | `API: ON` / `API: OFF` | `<button>` ขอบเรียบ `1px solid var(--border-subtle)` |

---

## 5. 8pt Spatial Grid, Radius, Shadow & Motion

### 5.1 Spacing Tokens

| Token | Value | px | การใช้งานมาตรฐาน |
|---|---|---|---|
| `--space-1` | `0.25rem` | `4px` | ระยะห่างระหว่าง Label กับ Input error hint, ระยะห่างภายใน Compact control |
| `--space-2` | `0.5rem` | `8px` | ระยะห่างระหว่าง Status Dot กับข้อความ, ระยะห่างไอคอนกับข้อความในปุ่ม, Gap ระหว่างปุ่มติดกัน |
| `--space-3` | `0.75rem` | `12px` | Padding แนวตั้ง/แนวนอนของ Input, Cell Padding แนวตั้งในตาราง |
| `--space-4` | `1rem` | `16px` | Padding ภายใน `.surface-card`, Cell Padding แนวนอนในตาราง, Gap ระหว่าง Form fields |
| `--space-6` | `1.5rem` | `24px` | Page outer padding ของ Main Content Area, Gap ระหว่าง Sections และใน Slide-over Drawer |
| `--space-8` | `2rem` | `32px` | ระยะห่างระหว่าง Major layout blocks, Empty state vertical padding |
| `--space-12` | `3rem` | `48px` | ความสูงมาตรฐานแถวตาราง (`44px–48px`), Empty state icon container spacing |

### 5.2 Border Radius, Shadow & Motion

| Category | Token | Value | การใช้งาน |
|---|---|---|---|
| **Radius** | `--radius-sm` | `4px` | Checkbox, Skeleton block, Compact inline tags/code blocks |
| **Radius** | `--radius-md` | `6px` | Buttons, Form inputs, Select dropdowns, Sidebar menu items |
| **Radius** | `--radius-lg` | `8px` | Cards (`.surface-card`), Data table wrapper, Confirm dialog |
| **Radius** | `--radius-full` | `9999px` | ใช้เฉพาะ Status Dot `6px × 6px` และ Avatar วงกลมเท่านั้น (ห้ามใช้สร้าง Pill Status) |
| **Shadow** | `--shadow-sm` | `0 1px 2px 0 rgba(15, 23, 42, 0.05)` | เงาบางเบาสำหรับ Card หรือ Dropdown popover |
| **Shadow** | `--shadow-drawer` | `-4px 0 24px rgba(15, 23, 42, 0.08)` | เงาด้านซ้ายของ Slide-over Drawer เพื่อแยกเลเยอร์จากตารางด้านหลัง |
| **Motion** | `--transition-fast` | `150ms cubic-bezier(0.16, 1, 0.3, 1)` | Row hover, Button hover, Drawer slide, Focus ring transitions |

---

## 6. High-Density Enterprise Layout & Shared UI Contracts

### 6.1 Application Shell (`AppShellComponent`)

```text
+--------------------+--------------------------------------------------------------+
|                    | Top Utility Header (56px)                                    |
|                    | [Company / Branch Switcher]       [API: ON/OFF] [User Menu]  |
|  Fixed Sidebar     +--------------------------------------------------------------+
|  (240px)           | Main Content Scroll Area (background: var(--surface-page))   |
|                    |                                                              |
|  - Brand Header    |   Page Header (Title + Primary Action Button)                |
|  - Dynamic Menu    |   Filter Toolbar (Search + Dropdowns)                        |
|    (*appHasPerm)   |   <app-state-container> -> <app-data-table> / Content        |
|                    |                                                              |
+--------------------+--------------------------------------------------------------+
```

- **Fixed Left Sidebar (`240px`):** พื้นหลัง `var(--surface-card)`, ขอบขวา `1px solid var(--border-subtle)`, เมนูซ่อน/แสดงตาม `*appHasPermission` (ไม่ Hardcode ชื่อ Role)
- **Top Utility Header (`56px`):** พื้นหลัง `var(--surface-card)`, ขอบล่าง `1px solid var(--border-subtle)` มีตัวสลับบริษัทย่อย/สาขา (`switch-context`) และปุ่ม **`API: ON / OFF`** (แสดงเมื่อ `APP_CONFIG.enableApiToggle === true`)
- **Auth Shell (`AuthShellComponent`):** กึ่งกลางหน้าจอสำหรับ `/auth/login` และ `/auth/select-company` บนพื้นหลัง `var(--surface-page)` พร้อม Card ความกว้าง `400px–440px`

### 6.2 High-Density Table & Slide-over Drawer

- **ความสูงแถวตาราง (`<tr>`):** `44px–48px` (`min-height: 44px`), ขอบล่าง `1px solid var(--border-subtle)`
- **การจัดแนวข้อมูล:** คอลัมน์ข้อความชิดซ้าย (`text-align: left`), คอลัมน์ตัวเลขเงิน/วันลา/ชั่วโมงชิดขวา (`text-align: right`) พร้อม `.tabular-nums`
- **Slide-over Drawer (`<app-slide-drawer>`):** ความกว้าง `480px–640px` ลอยจากขอบขวา ใช้สำหรับดูรายละเอียด/แก้ไข/อนุมัติคำขอ เพื่อให้ผู้ใช้ยังเห็นบริบทของตารางด้านหลังได้โดยไม่ต้องเปลี่ยนหน้า
- **Minimum Touch/Click Targets:** ปุ่ม Action ในตารางสูงอย่างน้อย `36px`, ปุ่ม Primary CTA / Check-in / Mobile target สูงอย่างน้อย `44px`

### 6.3 Mandatory 5 UI States (`<app-state-container>`)

ทุกหน้าที่มีการดึงข้อมูลจาก `/api/v1/...` ต้องรองรับครบทั้ง 5 สถานะและเชื่อมกับ Signal จริงเท่านั้น:

1. **Default State:** แสดงข้อมูลจริงจาก Backend API ตามลำดับชั้น Visual Hierarchy
2. **Hover & Focus State:**
   - แถวตารางเปลี่ยนพื้นหลังเป็น `var(--surface-hover)` ด้วย `var(--transition-fast)`
   - Interactive elements ทุกตัวมี Focus Ring: `outline: 2px solid var(--border-focus); outline-offset: 2px;`
3. **Loading State (Zero CLS):**
   - ใช้ `.skeleton-block` ที่มีจำนวนแถวและความสูงเท่ากับตาราง/การ์ดจริง (`44px–48px`) ห้ามใช้ Spinner หมุนกลางหน้าจอโล่งๆ เพื่อป้องกัน Cumulative Layout Shift
4. **Empty State:**
   - เมื่อ API คืน `[]` แสดงไอคอนเส้นบางเรียบ + ข้อความอธิบายบริบทชัดเจน (เช่น "ยังไม่มีคำขอรออนุมัติในสาขานี้") + ปุ่ม Action ทางไปต่อ (เมื่อมีสิทธิ์)
5. **Error State (RFC 7807 `ProblemDetails`):**
   - แสดง Inline Error Banner (`border-left: 3px solid var(--status-danger-fg)`) แสดง `title` และ `detail` จาก Backend พร้อมปุ่ม **"ลองใหม่อีกครั้ง (Retry)"**
   - เมื่อกดปุ่ม `API: OFF` (HTTP `503`) ทุกส่วนของหน้าจอต้องเข้าสู่ Error State ทันทีโดยห้ามมีตัวเลขหรือแถวข้อมูลจำลองหลงเหลืออยู่

---

## 7. Master CSS Reference (`src/styles/`)

### 7.1 `frontend/src/styles/_tokens.css`

```css
:root {
  /* =========================================================================
     1. 60% SURFACE & ELEVATION TOKENS (Calm Enterprise Light Canvas)
     ========================================================================= */
  --surface-page: #f8fafc;
  --surface-card: #ffffff;
  --surface-elevated: #ffffff;
  --surface-subtle: #f1f5f9;
  --surface-hover: #f1f5f9;
  --surface-selected: #fff7ed;
  --surface-overlay: rgba(15, 23, 42, 0.45);

  /* =========================================================================
     2. 30% TEXT & STRUCTURAL BORDER TOKENS
     ========================================================================= */
  --text-primary: #0f172a;
  --text-secondary: #334155;
  --text-muted: #64748b;
  --text-inverse: #ffffff;
  --text-brand: #c2410c;

  --border-subtle: #e2e8f0;
  --border-strong: #cbd5e1;
  --border-focus: #f97316;

  /* =========================================================================
     3. 10% BRAND & INTERACTIVE ACTION TOKENS (Sunset Orange)
     ========================================================================= */
  --action-primary-bg: #f97316;
  --action-primary-hover: #fb923c;
  --action-primary-text: #ffffff;

  --action-secondary-bg: #ffffff;
  --action-secondary-hover: #f8fafc;
  --action-secondary-text: #0f172a;

  /* =========================================================================
     4. SEMANTIC STATUS TOKENS (Text & Indicator Dot/Border Only — NO BADGE BG)
     ========================================================================= */
  --status-success-fg: #15803d;
  --status-warning-fg: #b45309;
  --status-danger-fg: #b91c1c;
  --status-info-fg: #0369a1;
  --status-neutral-fg: #64748b;

  /* =========================================================================
     5. 8PT SPATIAL GRID & RADIUS TOKENS
     ========================================================================= */
  --space-1: 0.25rem;  /* 4px */
  --space-2: 0.5rem;   /* 8px */
  --space-3: 0.75rem;  /* 12px */
  --space-4: 1rem;     /* 16px */
  --space-6: 1.5rem;   /* 24px */
  --space-8: 2rem;     /* 32px */
  --space-12: 3rem;    /* 48px */

  --radius-sm: 4px;
  --radius-md: 6px;
  --radius-lg: 8px;
  --radius-full: 9999px;

  /* =========================================================================
     6. TYPOGRAPHY, SHADOW & MOTION TOKENS
     ========================================================================= */
  --font-sans: 'Inter', 'Anuphan', system-ui, -apple-system, sans-serif;
  --font-mono: 'JetBrains Mono', ui-monospace, monospace;

  --text-xs: 0.75rem;    /* 12px */
  --text-sm: 0.875rem;   /* 14px */
  --text-base: 1rem;     /* 16px */
  --text-lg: 1.125rem;   /* 18px */
  --text-xl: 1.25rem;    /* 20px */

  --font-weight-regular: 400;
  --font-weight-medium: 500;
  --font-weight-semibold: 600;

  --shadow-sm: 0 1px 2px 0 rgba(15, 23, 42, 0.05);
  --shadow-drawer: -4px 0 24px rgba(15, 23, 42, 0.08);
  --transition-fast: 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

:root[data-theme='dark'] {
  /* =========================================================================
     DARK THEME OVERRIDES (Figma Material Surface Elevation + Sunset Orange)
     Reference Scale: #121212, #1E1E1E, #232323, #252525, #272727,
                      #2C2C2C, #2E2E2E, #333333, #363636, #383838
     ========================================================================= */
  --surface-page: #121212;
  --surface-card: #1e1e1e;
  --surface-elevated: #333333;
  --surface-subtle: #232323;
  --surface-hover: #252525;
  --surface-selected: rgba(249, 115, 22, 0.14);
  --surface-overlay: rgba(0, 0, 0, 0.65);

  --text-primary: #f5f5f5;
  --text-secondary: #d4d4d4;
  --text-muted: #a3a3a3;
  --text-inverse: #ffffff;
  --text-brand: #fb923c;

  --border-subtle: #2e2e2e;
  --border-strong: #383838;
  --border-focus: #fb923c;

  --action-primary-bg: #f97316;
  --action-primary-hover: #fb923c;
  --action-primary-text: #ffffff;

  --action-secondary-bg: #272727;
  --action-secondary-hover: #2c2c2c;
  --action-secondary-text: #f5f5f5;

  --status-success-fg: #4ade80;
  --status-warning-fg: #fbbf24;
  --status-danger-fg: #f87171;
  --status-info-fg: #38bdf8;
  --status-neutral-fg: #a3a3a3;

  --shadow-sm: 0 4px 4px 0 rgba(0, 0, 0, 0.25);
  --shadow-drawer: -4px 0 24px rgba(0, 0, 0, 0.45);
}
```

### 7.2 `frontend/src/styles/_utilities.css`

```css
.tabular-nums {
  font-variant-numeric: tabular-nums;
}

.status-indicator {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  background: transparent;
  font-size: var(--text-sm);
  font-weight: var(--font-weight-medium);
  line-height: 1.25rem;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: var(--radius-full);
  flex-shrink: 0;
  background-color: currentColor;
}

.surface-card {
  background-color: var(--surface-card);
  border: 1px solid var(--border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-4);
  box-shadow: var(--shadow-sm);
}

.skeleton-block {
  background: linear-gradient(
    90deg,
    var(--surface-subtle) 25%,
    var(--border-subtle) 50%,
    var(--surface-subtle) 75%
  );
  background-size: 200% 100%;
  border-radius: var(--radius-sm);
  animation: hrm-skeleton-shimmer 1.4s ease-in-out infinite;
}

@keyframes hrm-skeleton-shimmer {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

.focus-ring:focus-visible {
  outline: 2px solid var(--border-focus);
  outline-offset: 2px;
}
```
