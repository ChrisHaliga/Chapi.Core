import { Inject, Injectable } from "@angular/core";
import { BehaviorSubject, map, Observable, take } from "rxjs";

import { DOCUMENT } from "@angular/common";
import { AuthService as Auth0AuthService, User as Auth0User } from "@auth0/auth0-angular";
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from "@angular/router";
import { User } from "../models/user";

@Injectable({
  providedIn: "root"
})

export class AuthService {
  private userSubject = new BehaviorSubject<User>(new User());

  public user$: Observable<User> = this.userSubject.asObservable();
  public isAuthenticated$: Observable<boolean>;

  constructor(@Inject(DOCUMENT) public document: Document, public auth: Auth0AuthService) {
    auth.user$.subscribe((user: Auth0User | null | undefined) => {
      const updatedUser: User = new User();
      updatedUser.email = user?.name ?? "";
      this.userSubject.next(updatedUser);
    });

    this.isAuthenticated$ = auth.isAuthenticated$;
  }

  public login(redirectPath: string = '/'): void {
    this.auth.loginWithRedirect({ appState: { target: redirectPath } });
  }

  public logout(): void {
    this.auth.logout({ logoutParams: { returnTo: this.document.location.origin } });
  }
}

@Injectable({
  providedIn: "root"
})
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return this.auth.isAuthenticated$.pipe(
      take(1),
      map((isAuthenticated: boolean) => {
        if (!isAuthenticated) {
          this.auth.login(state.url);
          return false;
        }
        return true;
      })
    );
  }
}
