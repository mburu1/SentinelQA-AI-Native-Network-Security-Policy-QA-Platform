import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreatePolicyRequest, PagedQuery, PagedResult, Policy, PolicyAnalysis,
  PolicyValidationResult, UpdatePolicyRequest
} from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class PoliciesService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<Policy>> {
    return this.http.get<PagedResult<Policy>>(this.url('/policies'), { params: toParams(query) });
  }

  get(id: string): Observable<Policy> {
    return this.http.get<Policy>(this.url(`/policies/${id}`));
  }

  create(req: CreatePolicyRequest): Observable<Policy> {
    return this.http.post<Policy>(this.url('/policies'), req);
  }

  update(id: string, req: UpdatePolicyRequest): Observable<Policy> {
    return this.http.put<Policy>(this.url(`/policies/${id}`), req);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/policies/${id}`));
  }

  validate(id: string): Observable<PolicyValidationResult> {
    return this.http.post<PolicyValidationResult>(this.url(`/policies/${id}/validate`), {});
  }

  analysis(id: string): Observable<PolicyAnalysis> {
    return this.http.get<PolicyAnalysis>(this.url(`/policies/${id}/analysis`));
  }
}