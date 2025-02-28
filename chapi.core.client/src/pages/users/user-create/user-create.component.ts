// user-create.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../../services/apiService';
import { User } from '../../../models/user';
import { UserFormComponent } from '../user-form/user-form.component';
import { UserCacheService } from '../../../services/caching/userCacheService';

@Component({
  selector: 'app-user-create',
  templateUrl: './user-create.component.html',
  styleUrls: ['./user-create.component.scss'],
  standalone: true,
  imports: [CommonModule, UserFormComponent]
})
export class UserCreateComponent {
  user: User = new User();

  constructor(
    private apiService: ApiService,
    private userCacheService: UserCacheService,
    private router: Router
  ) { }

  saveUser(user: User): void {
    this.apiService.Post<User>('users', user).subscribe({
      next: (result) => {
        // Add the new user to the cache
        this.userCacheService.addOrUpdateUser(result);

        // Navigate to the user view page after creation
        this.router.navigate(['/user', result.email]);
      },
      error: (error) => {
        console.error('Error creating user:', error);
        alert('Failed to create user. Please try again.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/user/search']);
  }
}
