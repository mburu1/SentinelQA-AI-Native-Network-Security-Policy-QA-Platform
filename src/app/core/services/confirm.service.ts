import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  danger?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private resolver?: (ok: boolean) => void;
  readonly request = signal<ConfirmOptions | null>(null);

  confirm(options: ConfirmOptions): Promise<boolean> {
    this.request.set(options);
    return new Promise(resolve => (this.resolver = resolve));
  }

  respond(ok: boolean): void {
    this.resolver?.(ok);
    this.resolver = undefined;
    this.request.set(null);
  }
}