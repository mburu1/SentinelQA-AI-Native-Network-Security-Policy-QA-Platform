import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NotificationsService } from '@core/services/notifications.service';
import { emptyPage, Notification, PagedResult } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-notification-list',
  imports: [PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Notifications" subtitle="Event-driven email deliveries (Mailpit locally, retries tracked)" />

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()"
                     emptyIcon="✉️" emptyTitle="No notifications sent yet" />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class NotificationListComponent implements OnInit {
  private readonly svc = inject(NotificationsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly result = signal<PagedResult<Notification>>(emptyPage());

  readonly columns: TableColumn<Notification>[] = [
    { key: 'createdAt', header: 'Created', type: 'datetime' },
    { key: 'recipient', header: 'Recipient' },
    { key: 'channel', header: 'Channel', type: 'badge' },
    { key: 'subject', header: 'Subject' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'sentAt', header: 'Sent', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.svc.list({ page: this.page(), pageSize: 15 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: r => { this.result.set(r); this.loading.set(false); },
        error: () => { this.result.set(emptyPage()); this.loading.set(false); }
      });
  }
}