import { HttpClient, inject, Injectable } from '@angular/core';
import { environment } from '@env/environment';

@Injectable({ providedIn: 'root' })
export abstract class ApiBase {
  protected readonly http = inject(HttpClient);
  protected readonly api = environment.apiUrl;

  protected url(path: string): string {
    return `${this.api}${path.startsWith('/') ? path : `/${path}`}`;
  }
}