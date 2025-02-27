import { Component } from '@angular/core';
import { ApiService } from '../../services/apiService';
import { Observable, shareReplay } from 'rxjs';
import { User } from '../../models/user';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CrudTable } from '../../components/crudTable/crudTable.component';

@Component({
  selector: 'users',
  standalone: true,
  imports: [CommonModule, FormsModule, CrudTable],
  templateUrl: './users.page.html',
  styleUrls: ['./users.page.css']
})

export class UsersPage {
  public users$: Observable<User[]>;
  public selectedUser: User | null = null;

  public modify: boolean = false;

  constructor(private readonly apiService: ApiService) {
    this.users$ = apiService.Get<User[]>("users").pipe(
      shareReplay(1) //prevents calling get users every time
    );
  }

  public fields = {
    Name: (user: User) => user.name,
    Organization: (user: User) => user.organization,
    Email: (user: User) => user.email
  };

  public getDisplayName = (user: User): string => {
    return user.email;
  }

  public onCreate = () => {
    this.selectedUser = new User();
    this.modify = true;
  };

  public onEdit = (user: User) => {
    this.selectedUser = user;
    this.modify = true;
  };

  public onView = (user: User) => {
    this.selectedUser = user;
    this.modify = false;
  };

  public onDelete = (user: User) => {
    alert(`Delete user: ${user.name}`);
  };

  public onRefresh = () => {
    alert("Refresh user list");
  };

  public list() {
    this.selectedUser = null;
    this.modify = false;
  }

  public view(selectedUser: User) {
    this.selectedUser = selectedUser;
    this.modify = false;
  }
}
