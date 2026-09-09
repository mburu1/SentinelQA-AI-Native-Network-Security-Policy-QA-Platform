import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ChangeRequest, CreateChangeRequestRequest, PagedQuery, PagedResult } from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class ChangeRequestsService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<ChangeRequest>> {
    return this.http.get<PagedResult<ChangeRequest>>(this.url('/change-requests'), { params: toParams(query) });
  }

  get(id: string): Observable<ChangeRequest> {
    return this.http.get<ChangeRequest>(this.url(`/change-requests/${id}`));
  }

  create(req: CreateChangeRequestRequest): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url('/change-requests'), req);
  }

  submit(id: string): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url(`/change-requests/${id}/submit`), {});
  }

  approve(id: string, comment?: string): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url(`/change-requests/${id}/approve`), { comment });
  }

  reject(id: string, comment?: string): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url(`/change-requests/${id}/reject`), { comment });
  }

  deploy(id: string): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url(`/change-requests/${id}/deploy`), {});
  }

  rollback(id: string): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url(`/change-requests/${id}/rollback`), {});
  }

  cancel(id: string): Observable<ChangeRequest> {
    return this.http.post<ChangeRequest>(this.url(`/change-requests/${id}/cancel`), {});
  }
}