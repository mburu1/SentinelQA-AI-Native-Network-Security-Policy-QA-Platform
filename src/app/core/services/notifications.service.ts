import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Notification, PagedQuery, PagedResult } from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class NotificationsService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<Notification>> {
    return this.http.get<PagedResult<Notification>>(this.url('/notifications'), { params: toParams(query) });
  }
}