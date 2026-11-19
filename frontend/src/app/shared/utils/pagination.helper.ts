import { HttpParams } from '@angular/common/http';

export type SortDirection = 'asc' | 'desc';

export interface PaginationQuery {
  page: number;
  pageSize: number;
  sortBy?: string | null;
  sortDirection?: SortDirection | null;
  search?: string | null;
  filters?: Record<string, string | number | boolean | null | undefined>;
}

export function buildPaginationParams(query: PaginationQuery): HttpParams {
  let params = new HttpParams()
    .set('page', String(query.page))
    .set('pageSize', String(query.pageSize));

  if (query.sortBy && query.sortBy.trim().length > 0) {
    params = params.set('sortBy', query.sortBy.trim());
    if (query.sortDirection) {
      params = params.set('sortDirection', query.sortDirection);
    }
  }

  if (query.search && query.search.trim().length > 0) {
    params = params.set('search', query.search.trim());
  }

  if (query.filters) {
    const filterEntries = Object.entries(query.filters);
    for (const [key, value] of filterEntries) {
      if (value === null || value === undefined) {
        continue;
      }

      const textValue = String(value).trim();
      if (textValue.length === 0) {
        continue;
      }

      params = params.set(key, textValue);
    }
  }

  return params;
}
