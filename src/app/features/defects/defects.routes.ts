import { Routes } from '@angular/router';
import { DefectDetailComponent } from './defect-detail.component';
import { DefectFormComponent } from './defect-form.component';
import { DefectListComponent } from './defect-list.component';

export const DEFECTS_ROUTES: Routes = [
  { path: '', component: DefectListComponent },
  { path: 'new', component: DefectFormComponent },
  { path: ':id', component: DefectDetailComponent }
];