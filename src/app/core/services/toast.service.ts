import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  kind: 'success' | 'error' | 'info' | 'warning';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  readonly toasts = signal<Toast[]>([]);

  success(message: string): void { this.push('success', message); }
  error(message: string): void { this.push('error', message); }
  info(message: string): void { this.push('info', message); }
  warning(message: string): void { this.push('warning', message); }

  dismiss(id: number): void {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }

  private push(kind: Toast['kind'], message: string): void {
    const id = ++this.nextId;
    this.toasts.update(list => [...list, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), 5000);
  }
}