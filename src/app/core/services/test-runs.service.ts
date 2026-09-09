import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedQuery, PagedResult, StartTestRunRequest, TestResult, TestRun } from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class TestRunsService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<TestRun>> {
    return this.http.get<PagedResult<TestRun>>(this.url('/test-runs'), { params: toParams(query) });
  }

  get(id: string): Observable<TestRun> {
    return this.http.get<TestRun>(this.url(`/test-runs/${id}`));
  }

  results(id: string): Observable<TestResult[]> {
    return this.http.get<TestResult[]>(this.url(`/test-runs/${id}/results`));
  }

  start(req: StartTestRunRequest): Observable<TestRun> {
    return this.http.post<TestRun>(this.url('/test-runs'), req);
  }

  cancel(id: string): Observable<TestRun> {
    return this.http.post<TestRun>(this.url(`/test-runs/${id}/cancel`), {});
  }
}