import { TemplateRef } from '@angular/core';
import { SortDirection } from '../../utils/pagination.helper';

export type DataTableAlign = 'left' | 'right';
export type RowAccentTone = 'warning' | 'danger' | null;

export interface DataTableColumn {
  key: string;
  label: string;
  align?: DataTableAlign;
  isNumeric?: boolean;
  isMono?: boolean;
  sortable?: boolean;
  width?: string;
}

export interface DataTableSortEvent {
  sortBy: string | null;
  sortDirection: SortDirection | null;
}

export interface DataTableAction<T extends object = Record<string, unknown>> {
  key: string;
  label: string;
  variant?: 'default' | 'danger';
  onClick: (row: T) => void;
  isVisible?: (row: T) => boolean;
}

export interface DataTableCellContext<T extends object> {
  $implicit: T;
  row: T;
  column: DataTableColumn;
  value: unknown;
}

export type DataTableCellTemplates<T extends object> = Record<
  string,
  TemplateRef<DataTableCellContext<T>>
>;
