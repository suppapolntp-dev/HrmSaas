import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { APP_CONFIG } from '../../../core/config/app-config.token';
import { SortDirection } from '../../utils/pagination.helper';
import {
  StatusIndicatorComponent,
  StatusTone,
} from '../status-indicator/status-indicator.component';
import {
  DataTableAction,
  DataTableCellTemplates,
  DataTableColumn,
  DataTableSortEvent,
  RowAccentTone,
} from './data-table.models';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [NgTemplateOutlet, StatusIndicatorComponent],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTableComponent<T extends object = Record<string, unknown>> {
  private readonly appConfig = inject(APP_CONFIG);

  // 1. Head Section (Title, Status Indicator, Search, Actions)
  readonly title = input<string | null>(null);
  readonly subtitle = input<string | null>(null);
  readonly statusLabel = input<string | null>(null);
  readonly statusTone = input<StatusTone>('neutral');
  readonly searchable = input<boolean>(false);
  readonly searchPlaceholder = input<string>('ค้นหาข้อมูลในตาราง...');
  readonly searchQuery = input<string>('');

  // 2. Filter Bar & Sort Dropdown
  readonly showSortDropdown = input<boolean>(true);

  // 3. Table Data, Columns & Row Actions
  readonly columns = input.required<readonly DataTableColumn[]>();
  readonly rows = input.required<readonly T[]>();
  readonly trackByKey = input<string>('id');
  readonly cellTemplates = input<DataTableCellTemplates<T>>({});
  readonly rowAccentFn = input<((row: T) => RowAccentTone) | null>(null);
  readonly clickableRows = input<boolean>(false);
  readonly actions = input<readonly DataTableAction<T>[]>([]);
  readonly actionsLabel = input<string>('จัดการ');
  readonly emptyMessage = input<string>('ไม่พบข้อมูลที่ตรงกับเงื่อนไขการค้นหา');

  // 4. Sort & Pagination State
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);
  readonly showPagination = input<boolean>(true);
  readonly page = input<number>(1);
  readonly pageSize = input<number>(20);
  readonly totalCount = input<number>(0);

  // Outputs
  readonly searchChange = output<string>();
  readonly sortChange = output<DataTableSortEvent>();
  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();
  readonly rowClick = output<T>();

  protected readonly pageSizeOptions = this.appConfig.pageSizeOptions;
  protected readonly openActionRowIndex = signal<number | null>(null);
  protected readonly pageInputValue = signal<string>('1');

  protected readonly hasHeadSection = computed(() => {
    return Boolean(
      this.title() || this.subtitle() || this.statusLabel() || this.searchable(),
    );
  });

  protected readonly sortableColumns = computed(() => {
    return this.columns().filter((column) => column.sortable === true);
  });

  protected readonly hasActionsColumn = computed(() => {
    return this.actions().length > 0;
  });

  protected readonly totalColumnCount = computed(() => {
    const baseCount = this.columns().length;
    if (this.hasActionsColumn()) {
      return baseCount + 1;
    }
    return baseCount;
  });

  protected readonly currentSortDropdownValue = computed(() => {
    const colKey = this.sortBy();
    const dir = this.sortDirection();
    if (!colKey || !dir) {
      return '';
    }
    return `${colKey}:${dir}`;
  });

  protected readonly totalPages = computed(() => {
    const total = this.totalCount();
    const size = Math.max(1, this.pageSize());
    if (total <= 0) {
      return 1;
    }
    return Math.ceil(total / size);
  });

  protected readonly rangeStart = computed(() => {
    if (this.totalCount() === 0) {
      return 0;
    }
    return (this.page() - 1) * this.pageSize() + 1;
  });

  protected readonly rangeEnd = computed(() => {
    const calculated = this.page() * this.pageSize();
    return Math.min(calculated, this.totalCount());
  });

  constructor() {
    effect(() => {
      this.pageInputValue.set(String(this.page()));
    });
  }

  @HostListener('document:click')
  protected onDocumentClick(): void {
    if (this.openActionRowIndex() !== null) {
      this.openActionRowIndex.set(null);
    }
  }

  protected trackRow(index: number, row: T): unknown {
    const key = this.trackByKey();
    const record = row as Record<string, unknown>;
    const keyValue = record[key];
    if (keyValue !== undefined && keyValue !== null) {
      return keyValue;
    }
    return index;
  }

  protected getCellValue(row: T, key: string): unknown {
    const record = row as Record<string, unknown>;
    return record[key];
  }

  protected formatCellValue(value: unknown): string {
    if (value === null || value === undefined || value === '') {
      return '—';
    }
    return String(value);
  }

  protected getRowAccentTone(row: T): RowAccentTone {
    const accentFn = this.rowAccentFn();
    if (!accentFn) {
      return null;
    }
    return accentFn(row);
  }

  protected onSearchInput(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.searchChange.emit(inputElement.value);
  }

  protected onClearSearch(): void {
    this.searchChange.emit('');
  }

  protected onSort(column: DataTableColumn): void {
    if (!column.sortable) {
      return;
    }

    const currentKey = this.sortBy();
    const currentDir = this.sortDirection();

    if (currentKey !== column.key) {
      this.sortChange.emit({ sortBy: column.key, sortDirection: 'asc' });
      return;
    }

    if (currentDir === 'asc') {
      this.sortChange.emit({ sortBy: column.key, sortDirection: 'desc' });
      return;
    }

    if (currentDir === 'desc') {
      this.sortChange.emit({ sortBy: null, sortDirection: null });
      return;
    }

    this.sortChange.emit({ sortBy: column.key, sortDirection: 'asc' });
  }

  protected onSortDropdownChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const selectedValue = selectElement.value;

    if (!selectedValue) {
      this.sortChange.emit({ sortBy: null, sortDirection: null });
      return;
    }

    const parts = selectedValue.split(':');
    if (parts.length !== 2) {
      return;
    }

    const columnKey = parts[0];
    const direction = parts[1] === 'desc' ? 'desc' : 'asc';
    this.sortChange.emit({ sortBy: columnKey, sortDirection: direction });
  }

  protected getSortIndicatorSymbol(columnKey: string): string {
    if (this.sortBy() !== columnKey || !this.sortDirection()) {
      return '⇅';
    }
    if (this.sortDirection() === 'asc') {
      return '▲';
    }
    return '▼';
  }

  protected onRowClick(row: T): void {
    if (!this.clickableRows()) {
      return;
    }
    this.rowClick.emit(row);
  }

  protected getVisibleActions(row: T): DataTableAction<T>[] {
    const allActions = this.actions();
    const visible: DataTableAction<T>[] = [];
    for (const action of allActions) {
      if (!action.isVisible || action.isVisible(row)) {
        visible.push(action);
      }
    }
    return visible;
  }

  protected toggleActionMenu(rowIndex: number, event: Event): void {
    event.stopPropagation();
    if (this.openActionRowIndex() === rowIndex) {
      this.openActionRowIndex.set(null);
      return;
    }
    this.openActionRowIndex.set(rowIndex);
  }

  protected onActionClick(
    action: DataTableAction<T>,
    row: T,
    event: Event,
  ): void {
    event.stopPropagation();
    this.openActionRowIndex.set(null);
    action.onClick(row);
  }

  protected onPageChange(nextPage: number): void {
    const clamped = Math.min(Math.max(1, nextPage), this.totalPages());
    if (clamped === this.page()) {
      return;
    }
    this.pageChange.emit(clamped);
  }

  protected onPageInputChange(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const digitsOnly = inputElement.value.replace(/\D/g, '');
    inputElement.value = digitsOnly;
    this.pageInputValue.set(digitsOnly);
  }

  protected onPageInputCommit(): void {
    const raw = this.pageInputValue().trim();
    const parsed = Number(raw);
    if (!Number.isFinite(parsed) || parsed < 1) {
      this.pageInputValue.set(String(this.page()));
      return;
    }

    const clamped = Math.min(Math.max(1, Math.floor(parsed)), this.totalPages());
    this.pageInputValue.set(String(clamped));
    if (clamped !== this.page()) {
      this.pageChange.emit(clamped);
    }
  }

  protected onPageSizeSelect(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const parsedSize = Number(selectElement.value);
    if (!Number.isFinite(parsedSize) || parsedSize <= 0) {
      return;
    }
    this.pageSizeChange.emit(parsedSize);
  }
}
