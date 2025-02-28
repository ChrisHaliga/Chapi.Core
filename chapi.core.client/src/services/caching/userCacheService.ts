// services/caching/userCacheService.ts
import { Injectable } from '@angular/core';
import { User } from '../../models/user';
import { Observable, of } from 'rxjs';
import { ApiService } from '../apiService';
import { catchError, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserCacheService {
  private usersCache: Map<string, User> = new Map<string, User>();
  
  constructor(private apiService: ApiService) { }
  
  /**
   * Get all users from API, always fresh data
   * Still caches individual users for later retrieval
   */
  getAllUsers(): Observable<User[]> {
    console.log('Fetching all users from API');
    // Always fetch from API for fresh data
    return this.apiService.Get<User[]>('users').pipe(
      tap(users => {
        console.log(`Caching ${users.length} individual users`);
        // Cache individual users for faster access later
        users.forEach(user => {
          this.usersCache.set(user.email, user);
        });
      }),
      catchError(error => {
        console.error('Error fetching users', error);
        throw error;
      })
    );
  }
  
  /**
   * Get a user by email from cache or API
   * @param email User email
   * @param forceRefresh Force refresh from API
   */
  getUserByEmail(email: string, forceRefresh: boolean = false): Observable<User> {
    // Return cached user if available and not forcing refresh
    if (!forceRefresh && this.usersCache.has(email)) {
      console.log(`User with email ${email} found in cache`);
      return of(this.usersCache.get(email)!);
    }
    
    console.log(`Fetching user with email ${email} from API`);
    // Otherwise fetch from API
    return this.apiService.Get<User>('users', undefined, { email }).pipe(
      tap(user => {
        console.log('Caching user from API response:', user);
        // Cache the fetched user
        this.usersCache.set(email, user);
      }),
      catchError(error => {
        console.error(`Error fetching user with email: ${email}`, error);
        throw error;
      })
    );
  }
  
  /**
   * Add or update a user in the cache
   * @param user User to add or update
   */
  addOrUpdateUser(user: User): void {
    console.log('Adding/updating user in cache:', user);
    this.usersCache.set(user.email, { ...user }); // Store a copy to avoid reference issues
  }
  
  /**
   * Remove a user from the cache
   * @param email User email
   */
  removeUser(email: string): void {
    console.log(`Removing user with email ${email} from cache`);
    this.usersCache.delete(email);
  }
  
  /**
   * Clear the entire cache
   */
  clearCache(): void {
    console.log('Clearing entire user cache');
    this.usersCache.clear();
  }
  
  /**
   * Debug method to check what's in the cache
   */
  debugCache(): void {
    console.log('Current cache contents:');
    this.usersCache.forEach((user, email) => {
      console.log(`- ${email}: ${JSON.stringify(user)}`);
    });
  }
}
