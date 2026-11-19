export interface ShiftBreakRuleDto {
  id: string;
  breakStart: string;
  breakEnd: string;
  durationMinutes: number;
  isPaid: boolean;
}

export interface ShiftDefinitionDto {
  id: string;
  code: string;
  name: string;
  shiftType: string;
  startTime: string;
  endTime: string;
  isOvernight: boolean;
  gracePeriodMinutes: number;
  lateAfterMinutes: number;
  standardWorkMinutes: number;
  breakRules: ShiftBreakRuleDto[];
}

export interface ShiftBreakRuleInput {
  breakStart: string;
  breakEnd: string;
  durationMinutes: number;
  isPaid: boolean;
}

export interface CreateShiftRequest {
  code: string;
  name: string;
  shiftType: string;
  startTime: string;
  endTime: string;
  isOvernight: boolean;
  gracePeriodMinutes: number;
  lateAfterMinutes: number;
  standardWorkMinutes: number;
  breakRules: ShiftBreakRuleInput[];
}

export interface UpdateShiftRequest {
  name: string;
  shiftType: string;
  startTime: string;
  endTime: string;
  isOvernight: boolean;
  gracePeriodMinutes: number;
  lateAfterMinutes: number;
  standardWorkMinutes: number;
  breakRules: ShiftBreakRuleInput[];
}

export interface RosterItemDto {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  workDate: string;
  shiftDefinitionId: string | null;
  shiftCode: string | null;
  shiftName: string | null;
  dayType: string;
}

export interface BulkAssignRosterRequest {
  employeeIds: string[];
  startDate: string;
  endDate: string;
  shiftDefinitionId: string | null;
  dayType: string;
}
