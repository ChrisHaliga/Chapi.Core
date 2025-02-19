import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomePage } from './pages/home/home.page';
import { UsersPage } from './pages/users/users.page';
import { GroupsPage } from './pages/groups/groups.page';
import { OrganizationsPage } from './pages/organizations/organizations.page';
import { ApplicationsPage } from './pages/applications/applications.page';
import { SettingsPage } from './pages/settings/settings.page';

const routes: Routes = [
  { path: "", redirectTo: "/home", pathMatch: "full" },
  { path: "home", component: HomePage },
  { path: "users", component: UsersPage },
  { path: "groups", component: GroupsPage },
  { path: "organizations", component: OrganizationsPage },
  { path: "applications", component: ApplicationsPage },
  { path: "settings", component: SettingsPage }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
