export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  [key: string]: unknown;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(
    public status: number,
    public title: string,
    public detail?: string,
    public errors?: Record<string, string[]>,
    public traceId?: string
  ) {
    super(detail ?? title ?? `HTTP ${status}`);
  }

  get fieldErrors(): string[] {
    if (!this.errors) return [];
    return Object.values(this.errors).flat();
  }
}

export interface AuditEntry {
  id: string;
  tenantId: string;
  userId?: string;
  userName?: string;
  action: string;
  resource?: string;
  resourceId?: string;
  ipAddress?: string;
  traceId?: string;
  details?: string;
  createdAt: string;
}

export interface HealthStatus {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  entries?: Record<string, { status: string; description?: string }>;
}

export function emptyPage<T>(): PagedResult<T> {
  return { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 };
}