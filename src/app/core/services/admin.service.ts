import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiBase } from './api-base.service';

export interface Tenant { id: string; name: string; isActive: boolean; createdAt: string; }
export interface AppUser { id: string; tenantId: string; email: string; displayName: string; isActive: boolean; roles: string[]; }
export interface AppRole { name: string; description?: string; }

@Injectable({ providedIn: 'root' })
export class AdminService extends ApiBase {
  tenants(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(this.url('/tenants'));
  }

  createTenant(name: string): Observable<Tenant> {
    return this.http.post<Tenant>(this.url('/tenants'), { name });
  }

  users(): Observable<AppUser[]> {
    return this.http.get<AppUser[]>(this.url('/users'));
  }

  roles(): Observable<AppRole[]> {
    return this.http.get<AppRole[]>(this.url('/roles'));
  }
}