import { inject, Injectable } from '@angular/core';
import { catchError, forkJoin, map, Observable, of } from 'rxjs';
import { ChangeRequest, Defect, PagedResult, TestRun } from '@core/models';
import { ChangeRequestsService } from './change-requests.service';
import { DefectsService } from './defects.service';
import { FirewallsService } from './firewalls.service';
import { PoliciesService } from './policies.service';
import { TestRunsService } from './test-runs.service';

export interface DashboardSummary {
  firewalls: number;
  policies: number;
  openChangeRequests: number;
  openDefects: number;
  criticalDefects: number;
  recentChangeRequests: ChangeRequest[];
  recentDefects: Defect[];
  recentTestRuns: TestRun[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly firewalls = inject(FirewallsService);
  private readonly policies = inject(PoliciesService);
  private readonly changeRequests = inject(ChangeRequestsService);
  private readonly defects = inject(DefectsService);
  private readonly testRuns = inject(TestRunsService);

  private safe<T>(source: Observable<PagedResult<T>>) {
    return source.pipe(catchError(() => of({ totalCount: 0, items: [] as T[] } as PagedResult<T>)));
  }

  summary(): Observable<DashboardSummary> {
    return forkJoin({
      firewalls: this.safe(this.firewalls.list({ page: 1, pageSize: 1 })),
      policies: this.safe(this.policies.list({ page: 1, pageSize: 1 })),
      changeRequests: this.safe(this.changeRequests.list({ page: 1, pageSize: 5 })),
      defects: this.safe(this.defects.list({ page: 1, pageSize: 5 })),
      testRuns: this.safe(this.testRuns.list({ page: 1, pageSize: 5 }))
    }).pipe(
      map(({ firewalls, policies, changeRequests, defects, testRuns }) => ({
        firewalls: firewalls.totalCount,
        policies: policies.totalCount,
        openChangeRequests: changeRequests.items.filter(cr =>
          !['Completed', 'Rejected', 'Cancelled', 'RolledBack'].includes(cr.state)
        ).length,
        openDefects: defects.items.filter(d => d.status !== 'Closed' && d.status !== 'Verified').length,
        criticalDefects: defects.items.filter(d => d.severity === 'Critical').length,
        recentChangeRequests: changeRequests.items,
        recentDefects: defects.items,
        recentTestRuns: testRuns.items
      }))
    );
  }
}