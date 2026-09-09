import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateTestCaseRequest, CreateTestSuiteRequest, PagedQuery, PagedResult,
  TestCase, TestSuite
} from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class TestSuitesService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<TestSuite>> {
    return this.http.get<PagedResult<TestSuite>>(this.url('/test-suites'), { params: toParams(query) });
  }

  get(id: string): Observable<TestSuite> {
    return this.http.get<TestSuite>(this.url(`/test-suites/${id}`));
  }

  create(req: CreateTestSuiteRequest): Observable<TestSuite> {
    return this.http.post<TestSuite>(this.url('/test-suites'), req);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/test-suites/${id}`));
  }

  listCases(suiteId: string): Observable<TestCase[]> {
    return this.http.get<TestCase[]>(this.url(`/test-suites/${suiteId}/test-cases`));
  }

  addCase(req: CreateTestCaseRequest): Observable<TestCase> {
    return this.http.post<TestCase>(this.url('/test-cases'), req);
  }

  removeCase(caseId: string): Observable<void> {
    return this.http.delete<void>(this.url(`/test-cases/${caseId}`));
  }
}