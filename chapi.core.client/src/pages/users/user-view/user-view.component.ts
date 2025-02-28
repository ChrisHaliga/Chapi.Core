// user-view.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../../../models/user';
import { DeleteConfirmationModalComponent } from '../../../components/delete-confirmation-modal/delete-confirmation-modal.component';
import { ApiService } from '../../../services/apiService';
import { UserCacheService } from '../../../services/caching/userCacheService';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

// Import Bootstrap's Modal functionality
declare var bootstrap: any;

@Component({
  selector: 'app-user-view',
  templateUrl: './user-view.component.html',
  styleUrls: ['./user-view.component.css'],
  standalone: true,
  imports: [CommonModule, DeleteConfirmationModalComponent]
})
export class UserViewComponent implements OnInit {
  user: User = new User();
  email: string = '';
  loading = true;
  error: string | null = null;
  sanitizedUrl: SafeUrl | null = null;

  deleteModalId = 'userDeleteModal';


  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userCacheService: UserCacheService,
    private apiService: ApiService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit(): void {
    this.email = this.route.snapshot.paramMap.get('id') || '';
    this.loadUserDetails();
  }

  ngAfterViewInit(): void {
    // Ensure Bootstrap modal is properly initialized
    const modalElements = document.querySelectorAll('.modal');
    modalElements.forEach(modalElement => {
      new bootstrap.Modal(modalElement);
    });
  }

  loadUserDetails(): void {
    if (!this.email) {
      this.error = 'User email not provided';
      this.loading = false;
      return;
    }

    this.loading = true;
    this.error = null;

    // Try to get from cache first
    this.userCacheService.getUserByEmail(this.email)
      .pipe(
        catchError(error => {
          console.error('Error fetching user details:', error);
          this.error = 'Failed to load user details. Please try again later.';
          return of(new User()); // Return empty user on error
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(user => {
        this.user = user;
        this.sanitizedUrl = this.sanitizer.bypassSecurityTrustUrl(this.user.profilePicture);
      });
  }

  editUser(): void {
    this.router.navigate(['/user', this.email, 'edit']);
  }

  goToUserList(): void {
    this.router.navigate(['/user/search']);
  }

  handleDeleteConfirmation(): void {
    this.apiService.Delete('users', undefined, { email: this.email }).subscribe({
      next: () => {
        // Remove from cache on successful deletion
        this.userCacheService.removeUser(this.email);
        this.router.navigate(['/user/search']);
      },
      error: (err) => {
        console.error('Error deleting user:', err);
        alert('Failed to delete user. Please try again.');
      }
    });
  }

  // Helper method to get display name for the confirmation modal
  getUserDisplayName(): string {
    return this.user.email || this.email;
  }

  getApplicationsList(): string[] {
    if (this.user && this.user.applications && typeof this.user.applications === 'string') {
      return this.user.applications.split(',').filter(app => app.trim() !== '');
    }
    return [];
  }

  getGroupsList(): string[] {
    if (this.user && this.user.groups && typeof this.user.groups === 'string') {
      return this.user.groups.split(',').filter(group => group.trim() !== '');
    }
    return [];
  }
}
