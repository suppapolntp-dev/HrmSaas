import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';
import { CompaniesBranchesPageComponent } from './pages/companies-branches/companies-branches-page.component';
import { DepartmentsPositionsPageComponent } from './pages/departments-positions/departments-positions-page.component';

export const ORGANIZATION_ROUTES: Routes = [
  {
    path: 'companies-branches',
    canMatch: [permissionGuard('org.branch.manage')],
    component: CompaniesBranchesPageComponent,
    data: { title: 'บริษัทและสาขา' },
  },
  {
    path: 'departments-positions',
    canMatch: [permissionGuard('org.structure.manage')],
    component: DepartmentsPositionsPageComponent,
    data: { title: 'แผนกและตำแหน่ง' },
  },
];
