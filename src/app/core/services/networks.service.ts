import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateNetworkRequest, Network, NetworkGroup, PagedQuery, PagedResult, ServiceDefinition
} from '@core/models';
import { ApiBase } from './api-base.service';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class NetworksService extends ApiBase {
  list(query?: PagedQuery): Observable<PagedResult<Network>> {
    return this.http.get<PagedResult<Network>>(this.url('/networks'), { params: toParams(query) });
  }

  create(req: CreateNetworkRequest): Observable<Network> {
    return this.http.post<Network>(this.url('/networks'), req);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/networks/${id}`));
  }

  groups(): Observable<NetworkGroup[]> {
    return this.http.get<NetworkGroup[]>(this.url('/network-groups'));
  }

  services(): Observable<ServiceDefinition[]> {
    return this.http.get<ServiceDefinition[]>(this.url('/services'));
  }
}