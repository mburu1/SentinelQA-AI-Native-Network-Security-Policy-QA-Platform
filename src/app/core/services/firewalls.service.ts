import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateFirewallRequest, Firewall, PagedQuery, PagedResult, UpdateFirewallRequest
} from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class FirewallsService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<Firewall>> {
    return this.http.get<PagedResult<Firewall>>(this.url('/firewalls'), { params: toParams(query) });
  }

  get(id: string): Observable<Firewall> {
    return this.http.get<Firewall>(this.url(`/firewalls/${id}`));
  }

  create(req: CreateFirewallRequest): Observable<Firewall> {
    return this.http.post<Firewall>(this.url('/firewalls'), req);
  }

  update(id: string, req: UpdateFirewallRequest): Observable<Firewall> {
    return this.http.put<Firewall>(this.url(`/firewalls/${id}`), req);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/firewalls/${id}`));
  }
}