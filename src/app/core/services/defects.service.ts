import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateDefectRequest, Defect, DefectStatus, PagedQuery, PagedResult } from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class DefectsService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<Defect>> {
    return this.http.get<PagedResult<Defect>>(this.url('/defects'), { params: toParams(query) });
  }

  get(id: string): Observable<Defect> {
    return this.http.get<Defect>(this.url(`/defects/${id}`));
  }

  create(req: CreateDefectRequest): Observable<Defect> {
    return this.http.post<Defect>(this.url('/defects'), req);
  }

  update(id: string, req: Partial<CreateDefectRequest>): Observable<Defect> {
    return this.http.put<Defect>(this.url(`/defects/${id}`), req);
  }

  transition(id: string, status: DefectStatus, comment?: string): Observable<Defect> {
    return this.http.post<Defect>(this.url(`/defects/${id}/transition`), { status, comment });
  }
}