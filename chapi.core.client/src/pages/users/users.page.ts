import { Component } from '@angular/core';
import { ApiService } from '../../services/apiService';
import { Observable, shareReplay } from 'rxjs';
import { User } from '../../models/user';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'users',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

  public create() {
    this.selectedUser = new User();
    this.modify = true;
  }


  public edit(selectedUser: User) {
    this.selectedUser = selectedUser;
    this.modify = true;
  }

  public delete(selectedUser: User) {
    alert("TODO")
  }

  public list() {
    this.selectedUser = null;
    this.modify = false;
  }

  public view(selectedUser: User) {
    this.selectedUser = selectedUser;
    this.modify = false;
  }
}
