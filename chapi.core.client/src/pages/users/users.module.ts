// users.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'search',
    loadComponent: () => import('./user-search/user-search.component').then(c => c.UserSearchComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./user-create/user-create.component').then(c => c.UserCreateComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./user-view/user-view.component').then(c => c.UserViewComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./user-edit/user-edit.component').then(c => c.UserEditComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersModule { }
