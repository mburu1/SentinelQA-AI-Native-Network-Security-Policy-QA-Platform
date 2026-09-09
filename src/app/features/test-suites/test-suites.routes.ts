import { Routes } from '@angular/router';
import { TestSuiteDetailComponent } from './test-suite-detail.component';
import { TestSuiteListComponent } from './test-suite-list.component';

export const TEST_SUITES_ROUTES: Routes = [
  { path: '', component: TestSuiteListComponent },
  { path: ':id', component: TestSuiteDetailComponent }
];