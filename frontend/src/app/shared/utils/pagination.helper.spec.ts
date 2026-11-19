import { buildPaginationParams } from './pagination.helper';

describe('buildPaginationParams', () => {
  it('should serialize page, pageSize, sorting, search, and non-empty filters', () => {
    const params = buildPaginationParams({
      page: 2,
      pageSize: 50,
      sortBy: 'employeeCode',
      sortDirection: 'desc',
      search: 'สมชาย',
      filters: {
        branchId: 'branch-1',
        status: 'ACTIVE',
        emptyField: '',
        skippedNull: null,
        skippedUndefined: undefined,
      },
    });

    expect(params.get('page')).toBe('2');
    expect(params.get('pageSize')).toBe('50');
    expect(params.get('sortBy')).toBe('employeeCode');
    expect(params.get('sortDirection')).toBe('desc');
    expect(params.get('search')).toBe('สมชาย');
    expect(params.get('branchId')).toBe('branch-1');
    expect(params.get('status')).toBe('ACTIVE');
    expect(params.has('emptyField')).toBe(false);
    expect(params.has('skippedNull')).toBe(false);
    expect(params.has('skippedUndefined')).toBe(false);
  });
});
