import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomePage } from './pages/home/home.page';

const routes: Routes = [
  { path: "", redirectTo: "/home", pathMatch: "full" },
  { path: "home", component: HomePage }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
