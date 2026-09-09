import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HealthStatus } from '@core/models';
import { ApiBase } from './api-base.service';

@Injectable({ providedIn: 'root' })
export class HealthService extends ApiBase {
  live(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>(this.url('/health/live'));
  }

  ready(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>(this.url('/health/ready'));
  }
}