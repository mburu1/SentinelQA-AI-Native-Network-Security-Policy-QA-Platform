import { Routes } from '@angular/router';
import { TestRunDetailComponent } from './test-run-detail.component';
import { TestRunListComponent } from './test-run-list.component';

export const TEST_RUNS_ROUTES: Routes = [
  { path: '', component: TestRunListComponent },
  { path: ':id', component: TestRunDetailComponent }
];