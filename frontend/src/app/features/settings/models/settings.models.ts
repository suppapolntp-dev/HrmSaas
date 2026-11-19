export interface PermissionDto {
  id: string;
  code: string;
  module: string;
  description: string;
}

export interface RoleDto {
  id: string;
  companyId: string | null;
  code: string;
  name: string;
  isSystemRole: boolean;
  permissionCodes: string[];
}

export interface CreateRoleRequest {
  code: string;
  name: string;
  permissionCodes: string[];
}

export interface UpdateRoleRequest {
  name: string;
  permissionCodes: string[];
}

export interface AuditLogQueryParams {
  page: number;
  pageSize: number;
  entityName?: string | null;
  action?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface AuditLogDto {
  id: string;
  companyId: string | null;
  actorUserId: string | null;
  entityName: string;
  entityId: string;
  action: string;
  oldValues: string | null;
  newValues: string | null;
  ipAddress: string | null;
  correlationId: string;
  occurredAt: string;
}
