import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuditEntry, PagedQuery, PagedResult } from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class AuditService extends ApiBase {
  /** Note: the API responds with Cache-Control: no-store for audit data. */
  list(query?: PagedQuery): Observable<PagedResult<AuditEntry>> {
    return this.http.get<PagedResult<AuditEntry>>(this.url('/audit'), { params: toParams(query) });
  }
}