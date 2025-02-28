// user-edit.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../../../models/user';
import { ApiService } from '../../../services/apiService';
import { UserCacheService } from '../../../services/caching/userCacheService';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import { UserFormComponent } from '../user-form/user-form.component';

@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.scss'],
  standalone: true,
  imports: [CommonModule, UserFormComponent]
})
export class UserEditComponent implements OnInit {
  user: User = new User();
  email: string = '';
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private userCacheService: UserCacheService
  ) { }

  ngOnInit(): void {
    // Get email from route parameter
    this.email = this.route.snapshot.paramMap.get('id') || '';
    this.loadUserData();
  }

  loadUserData(): void {
    if (!this.email) {
      this.error = 'User email not provided';
      this.loading = false;
      return;
    }

    console.log('Loading user data for email:', this.email);
    this.loading = true;
    this.error = null;

    // Try to get user from cache first
    this.userCacheService.getUserByEmail(this.email)
      .pipe(
        catchError(error => {
          console.error('Error fetching user data:', error);
          this.error = 'Failed to load user data. Please try again later.';
          return of(new User());
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(user => {
        console.log('Received user data in UserEditComponent:', user);
        this.user = { ...user }; // Create a copy to ensure change detection
      });
  }

  saveUser(user: User): void {
    console.log('Saving updated user:', user);
    this.apiService.Put<User>('users', user).subscribe({
      next: (updatedUser) => {
        // Update the cache with the updated user
        this.userCacheService.addOrUpdateUser(updatedUser);

        // Navigate to the user view page after update
        this.router.navigate(['/user', this.email]);
      },
      error: (error) => {
        console.error('Error updating user:', error);
        alert('Failed to update user. Please try again.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/user', this.email]);
  }
}
