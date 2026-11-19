# Work Log — LeaveAndWorkflowService

- **Component / Module:** `LeaveAndWorkflowService` & `WorkflowDtos`
- **Path:** `backend/src/HrmSaas.Application/Features/Workflows/`
- **Status:** `Done`

<!-- PAGE_LOG_ENTRIES_START -->

## [2026-11-20] เพิ่ม Query & Approval Route CRUD สำหรับหน้า Portal / Workflows

| สิ่งที่สั่ง (Request) | สิ่งที่ทำไปแล้ว / ไฟล์ที่เขียน (Done & Files) | ผลลัพธ์ / Error ที่เจอ (Result & Errors) | Backend / DB ที่แตะ | ค้างทำต่อ / หมายเหตุ (Pending) |
| :--- | :--- | :--- | :--- | :--- |
| ไม่ไหวละทำทุกหน้ามาเลยเดี๋ยวแก้ที่ละหน้าเอา | เพิ่ม `GetMyRequestsAsync`, `GetApprovalHistoryAsync`, `GetWorkflowDefinitionsAsync`, `CreateWorkflowDefinitionAsync`, `UpdateWorkflowDefinitionAsync` ใน `LeaveAndWorkflowService.cs` และ DTOs ใน `WorkflowDtos.cs` พร้อม method `Update` ใน `WorkflowDefinition.cs` | `dotnet build HrmSaas.sln` PASS (0 Errors) | แก้ `WorkflowDefinition.cs`, `WorkflowDtos.cs`, `LeaveAndWorkflowService.cs` (ไม่แก้ DB Schema) | ทดสอบ Runtime ร่วมกับ PostgreSQL ผ่าน Docker |
