# Work Log — AppShell & Navigation Layout

- **Component / Module:** `AppShellComponent`, `navigation.config.ts`
- **Path:** `frontend/src/app/core/layout/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-19] แยก TopNavbarComponent ออกจาก AppShellComponent

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| งั้นลอง เขียน navbar component หน่อย จะได้ดู log ด้วย | สร้าง `top-navbar.component.ts`, `.html`, `.css` (Dumb component รับ `input()`/`output()` signals สำหรับ Mobile Menu, Company/Branch Switcher, `API: ON/OFF`, Theme Toggle, Logout) และปรับ `app-shell.component.ts`, `.html`, `.css` ให้เรียก `<app-top-navbar>` แทน `<header class="topbar">` เดิม | `ng build`: PASS (CSS `app-shell.component.css` ลดจาก `6.68 kB` เหลือ `5.66 kB`, ยังมี budget warning > 4.00 kB)<br>`ng test`: 8 files, 13 passed | `ไม่มี — ใช้ AuthService.switchContext, logout และ ThemeService / ApiToggleService เดิม` | `ไม่มี` |

## [2026-11-19] ซ่อนปุ่มพับ Sidebar ตอนยุบ และกด Profile เพื่อกางกลับ

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ตอนเก็บ sidebar เอาปุ่มด้านขวาออก ให้กดที่ profile เปิดแทน (และตอนพับให้เอาออก จะได้ไม่กินพื้นที่ข้างโลโก้ `H`) | แก้ `app-shell.component.html` และ `app-shell.component.css`: คงปุ่ม `<` ไว้ด้านขวา Header เฉพาะตอนเปิดอยู่, ตอนพับเหลือ `72px` ซ่อนปุ่มออกเหลือโลโก้ `H` เดี่ยวๆ และให้กด Profile Avatar ด้านล่างเพื่อขยายกลับ | `ng build`: PASS (มีเพียง CSS budget warning 6.68 kB > 4.00 kB)<br>`ng test`: 8 files, 13 passed | `ไม่มี — ใช้ Session Store เดิม` | `ไม่มี` |

## [2026-11-19] ปรับ Sidebar ตามแนวทาง Ticky Admin UX

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ทำ sidebar แบบ Ticky Admin UX (`26960899`) | แก้ `navigation.config.ts` (เพิ่ม `NavIconKey` 18 ไอคอน), `app-shell.component.ts` (`isSidebarCollapsed`, `navSearchQuery`, `userInitials`), `app-shell.component.html`, `app-shell.component.css` เพิ่มช่องค้นหาเมนู, ไอคอน SVG, แถบเน้นซ้าย 3px และ Profile Footer | `ng build`: PASS<br>`ng test`: 8 files, 13 passed | `ไม่มี — ใช้ Dynamic Permissions จาก /api/v1/auth/me` | `ไม่มี` |
