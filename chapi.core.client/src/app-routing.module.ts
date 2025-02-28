import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomePage } from './pages/home/home.page';
import { GroupsPage } from './pages/groups/groups.page';
import { OrganizationsPage } from './pages/organizations/organizations.page';
import { ApplicationsPage } from './pages/applications/applications.page';
import { SettingsPage } from './pages/settings/settings.page';
import { AuthGuard } from './services/authService';

const routes: Routes = [
  { path: "", redirectTo: "/home", pathMatch: "full" },
  { path: "home", component: HomePage },
  {
    path: 'user',
    children: [
      {
        path: 'search',
        loadComponent: () => import('./pages/users/user-search/user-search.component')
          .then(c => c.UserSearchComponent)
      },
      {
        path: 'create',
        loadComponent: () => import('./pages/users/user-create/user-create.component')
          .then(c => c.UserCreateComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./pages/users/user-view/user-view.component')
          .then(c => c.UserViewComponent)
      },
      {
        path: ':id/edit',
        loadComponent: () => import('./pages/users/user-edit/user-edit.component')
          .then(c => c.UserEditComponent)
      }
    ],
    canActivate: [AuthGuard]
  },
  { path: "groups", component: GroupsPage, canActivate: [AuthGuard] },
  { path: "organizations", component: OrganizationsPage, canActivate: [AuthGuard] },
  { path: "applications", component: ApplicationsPage, canActivate: [AuthGuard] },
  { path: "settings", component: SettingsPage, canActivate: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
