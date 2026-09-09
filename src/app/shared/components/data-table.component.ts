import { DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { StatusBadgeComponent } from './status-badge.component';
import { SpinnerComponent } from './spinner.component';

export interface TableColumn<T> {
  key: string;
  header: string;
  type?: 'text' | 'badge' | 'date' | 'datetime' | 'mono';
  width?: string;
  align?: 'left' | 'right' | 'center';
  value?: (row: T) => unknown;
}

@Component({
  selector: 'sq-data-table',
  imports: [DatePipe, StatusBadgeComponent, SpinnerComponent],
  template: `
    <div class="table-wrap">
      <table class="table">
        <thead>
          <tr>
            @for (col of columns(); track col.key) {
              <th [style.width]="col.width" [style.text-align]="col.align ?? 'left'">{{ col.header }}</th>
            }
          </tr>
        </thead>
        <tbody>
          @for (row of rows(); track $index) {
            <tr [class.clickable]="clickable()" (click)="clickable() && rowClick.emit(row)">
              @for (col of columns(); track col.key) {
                <td [style.text-align]="col.align ?? 'left'">
                  @switch (col.type ?? 'text') {
                    @case ('badge') { <sq-status-badge [label]="asString(cellValue(row, col))" /> }
                    @case ('date') { {{ asString(cellValue(row, col)) | date: 'dd MMM yyyy' }} }
                    @case ('datetime') { {{ asString(cellValue(row, col)) | date: 'dd MMM yyyy, HH:mm' }} }
                    @case ('mono') { <span class="mono">{{ cellValue(row, col) ?? '—' }}</span> }
                    @default { {{ cellValue(row, col) ?? '—' }} }
                  }
                </td>
              }
            </tr>
          } @empty {
            <tr>
              <td [attr.colspan]="columns().length">
                <sq-empty-state [icon]="emptyIcon()" [title]="emptyTitle()" [message]="emptyMessage()" />
              </td>
            </tr>
          }
        </tbody>
      </table>
      @if (loading()) {
        <div class="table-loading"><sq-spinner /></div>
      }
    </div>
  `
})
export class DataTableComponent<T> {
  readonly columns = input.required<TableColumn<T>[]>();
  readonly rows = input.required<T[]>();
  readonly loading = input(false);
  readonly clickable = input(false);
  readonly emptyIcon = input('📭');
  readonly emptyTitle = input('No records found');
  readonly emptyMessage = input('');
  readonly rowClick = output<T>();

  cellValue(row: T, col: TableColumn<T>): unknown {
    if (col.value) return col.value(row);
    return (row as Record<string, unknown>)[col.key];
  }

  asString(value: unknown): string {
    return value === null || value === undefined ? '—' : String(value);
  }
}