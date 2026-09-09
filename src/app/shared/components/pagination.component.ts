import { Component, input, output } from '@angular/core';

@Component({
  selector: 'sq-pagination',
  template: `
    <div class="pagination">
      <span class="muted small">Total: {{ total() }}</span>
      <button class="btn btn-sm" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">← Prev</button>
      <span class="small">Page {{ page() }} / {{ Math.max(totalPages(), 1) }}</span>
      <button class="btn btn-sm" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">Next →</button>
    </div>
  `
})
export class PaginationComponent {
  readonly Math = Math;
  readonly page = input(1);
  readonly totalPages = input(0);
  readonly total = input(0);
  readonly pageChange = output<number>();
}