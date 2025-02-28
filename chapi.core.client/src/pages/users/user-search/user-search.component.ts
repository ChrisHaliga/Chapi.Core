// user-search.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { User } from '../../../models/user';
import { FormsModule } from '@angular/forms';
import { CrudTable } from '../../../components/crudTable/crudTable.component';
import { Observable, catchError, finalize } from 'rxjs';
import { ApiService } from '../../../services/apiService';
import { UserCacheService } from '../../../services/caching/userCacheService';

@Component({
  selector: 'app-user-search',
  templateUrl: './user-search.component.html',
  styleUrls: ['./user-search.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, CrudTable]
})
export class UserSearchComponent implements OnInit {
  public users$: Observable<User[]>;
  public selectedUser: User | null = null;
  public modify: boolean = false;
  public isLoading = false;
  public error: string | null = null;

  constructor(
    private readonly userCacheService: UserCacheService,
    private readonly apiService: ApiService,
    private readonly router: Router
  ) {
    // Initialize with empty observable - will be set in ngOnInit
    this.users$ = new Observable<User[]>();
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.error = null;

    // Always get fresh data from the API, but the service still caches individual users
    this.users$ = this.userCacheService.getAllUsers().pipe(
      catchError(error => {
        console.error('Error loading users:', error);
        this.error = 'Failed to load users. Please try again.';
        throw error;
      }),
      finalize(() => {
        this.isLoading = false;
      })
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
    this.router.navigate(['/user/create']);
  };

  public onEdit = (user: User) => {
    // Navigate to edit page - user should already be cached
    this.router.navigate(['/user', user.email, 'edit']);
  };

  public onView = (user: User) => {
    // Navigate to view page - user should already be cached
    this.router.navigate(['/user', user.email]);
  };

  public onDelete = (user: User) => {
    // Remove from cache
    this.userCacheService.removeUser(user.email);

    // API call to delete user
    this.apiService.Delete('users', undefined, { email: user.email }).subscribe({
      next: () => {
        // Refresh the list after deletion
        this.loadUsers();
      },
      error: (err) => {
        console.error('Error deleting user:', err);
        alert('Failed to delete user. Please try again.');
      }
    });
  };

  public onRefresh = () => {
    this.loadUsers();
  };
}
