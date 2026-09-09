import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar.component';
import { TopbarComponent } from './topbar.component';
import { ConfirmHostComponent } from '@shared/components/confirm-host.component';
import { ToastHostComponent } from '@shared/components/toast-host.component';

@Component({
  selector: 'sq-shell',
  imports: [RouterOutlet, SidebarComponent, TopbarComponent, ToastHostComponent, ConfirmHostComponent],
  template: `
    <div class="shell">
      <sq-sidebar />
      <div class="shell-main">
        <sq-topbar />
        <main class="shell-content">
          <router-outlet />
        </main>
      </div>
    </div>
    <sq-toast-host />
    <sq-confirm-host />
  `
})
export class ShellComponent {}