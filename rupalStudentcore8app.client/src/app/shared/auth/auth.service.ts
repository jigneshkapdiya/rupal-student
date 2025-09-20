import { Injectable } from "@angular/core";
import { SafeUrl } from "@angular/platform-browser";
import { Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { BehaviorSubject, Observable } from "rxjs";
import { JwtHelperService } from "@auth0/angular-jwt";

export interface SignInUser {
  id: number;
  fullName: string;
  fullNameAr: string;
  username: string;
  email: string;
  roleName: string;
  phone: string;
  profileImage: string | SafeUrl;
}

// Constants for JWT claims and storage keys
const JWT_CLAIMS = {
  ID: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  USERNAME: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
  EMAIL: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
  FULL_NAME: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname',
  ROLE: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  PHONE: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone',
  PROFILE_IMAGE: 'profile_image'
} as const;

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'access_token',
  REFRESH_TOKEN: 'refresh_token',
  TWO_FACTOR_TOKEN: '2fa_token',
  TWO_FACTOR_USER_ID: '2fa_user_id',
  LANGUAGE: 'lang',
  USERNAME: 'username',
  PERMITTED_MENU: 'permittedMenu'
} as const;

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly jwtHelper = new JwtHelperService();
  private readonly defaultUser: SignInUser = {
    id: 0,
    fullName: '',
    fullNameAr: '',
    username: '',
    email: '',
    roleName: '',
    phone: '',
    profileImage: ''
  };

  // Public observables
  public readonly currentUser$: Observable<SignInUser>;
  private readonly currentUserSubject: BehaviorSubject<SignInUser>;

  constructor(
    private readonly router: Router,
    private readonly toastr: ToastrService
  ) {
    this.currentUserSubject = new BehaviorSubject<SignInUser>(this.defaultUser);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  /**
   * Logs out the user and redirects to login page
   */
  public logout(): void {
    this.currentUserSubject.next(this.defaultUser);
    this.redirectToLogin();
  }

  /**
   * Clears all authentication-related data from localStorage
   */
  public clearLocalStorage(): void {
    // Clear specific authentication keys
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.TWO_FACTOR_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.TWO_FACTOR_USER_ID);
    localStorage.removeItem(STORAGE_KEYS.LANGUAGE);
    localStorage.removeItem(STORAGE_KEYS.USERNAME);
    localStorage.removeItem(STORAGE_KEYS.PERMITTED_MENU);

    // Clear all remaining localStorage items
    localStorage.clear();
  }

  /**
   * Redirects user to login page after clearing storage
   */
  public redirectToLogin(): void {
    this.clearLocalStorage();
    setTimeout(() => {
      this.router.navigate(["/auth/login"]);
    }, 1500);
  }

  /**
   * Checks if the current user is authenticated with a valid token
   */
  public isAuthenticated(): boolean {
    try {
      const token = this.getAccessToken();
      return !!token && !this.jwtHelper.isTokenExpired(token);
    } catch (error) {
      console.error('Error checking authentication status:', error);
      return false;
    }
  }

  /**
   * Sets username in localStorage
   */
  public setUserName(userName: string): void {
    localStorage.setItem(STORAGE_KEYS.USERNAME, userName);
  }

  /**
   * Gets the current user's role name from JWT token
   */
  public getRoleName(): string {
    try {
      const token = this.getAccessToken();
      if (!token) {
        return '';
      }

      const decodedToken = this.jwtHelper.decodeToken(token);
      return decodedToken?.[JWT_CLAIMS.ROLE] || '';
    } catch (error: any) {
      console.error('Error extracting role from JWT token:', error);
      this.toastr.error('Failed to extract user role', 'Authentication Error');
      return '';
    }
  }


  /**
   * Gets the current signed-in user information from JWT token
   */
  public getSignInUser(): SignInUser {
    try {
      const token = this.getAccessToken();
      if (!token) {
        return this.defaultUser;
      }

      const decodedToken = this.jwtHelper.decodeToken(token);
      if (!decodedToken) {
        return this.defaultUser;
      }

      // Extract and map claims with type safety using constants
      const user: SignInUser = {
        id: Number(decodedToken.id) || 0,
        username: decodedToken.unique_name || '',
        email: decodedToken.email || '',
        fullName: decodedToken.given_name || '',
        roleName: decodedToken.role || '',
        phone: decodedToken.phone || '',
        profileImage: decodedToken.profileImage || '',
        fullNameAr: decodedToken.fullNameAr || '' // Fallback to fullName
      };

      // Update the current user subject
      this.currentUserSubject.next(user);
      return user;

    } catch (error: any) {
      console.error('Error decoding JWT token:', error);
      this.toastr.error('Failed to decode user information', 'Authentication Error');
      return this.defaultUser;
    }
  }

  /**
   * Gets the current signed-in user ID from JWT token
   */
  public getSignInUserId(): number {
    try {
      const token = this.getAccessToken();
      if (!token) {
        return 0;
      }

      const decodedToken = this.jwtHelper.decodeToken(token);
      if (!decodedToken) {
        return 0;
      }

      return Number(decodedToken[JWT_CLAIMS.ID]) || 0;

    } catch (error: any) {
      console.error('Error extracting user ID from JWT token:', error);
      this.toastr.error('Failed to extract user ID', 'Authentication Error');
      return 0;
    }
  }

  // Token Management Methods
  /**
   * Sets the access token in localStorage
   */
  public setAccessToken(token: string): void {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, token);
  }

  /**
   * Gets the access token from localStorage
   */
  public getAccessToken(): string {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN) || '';
  }

  /**
   * Sets the refresh token in localStorage
   */
  public setRefreshToken(token: string): void {
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, token);
  }

  /**
   * Gets the refresh token from localStorage
   */
  public getRefreshToken(): string {
    return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN) || '';
  }

  /**
   * Sets the user's preferred language
   */
  public setLanguage(language: string): void {
    localStorage.setItem(STORAGE_KEYS.LANGUAGE, language);
  }

  /**
   * Gets the user's preferred language
   */
  public getLanguage(): string {
    return localStorage.getItem(STORAGE_KEYS.LANGUAGE) || '';
  }

  /**
   * Sets the user's assigned menu permissions
   */
  public setAssignedMenu(menus: any[]): void {
    try {
      localStorage.setItem(STORAGE_KEYS.PERMITTED_MENU, JSON.stringify(menus));
    } catch (error) {
      console.error('Error storing menu permissions:', error);
    }
  }

  /**
   * Gets the user's assigned menu permissions
   */
  public getAssignedMenu(): any[] {
    try {
      const menus = localStorage.getItem(STORAGE_KEYS.PERMITTED_MENU);
      return menus ? JSON.parse(menus) : [];
    } catch (error) {
      console.error('Error parsing menu permissions:', error);
      return [];
    }
  }

  // Two-Factor Authentication Methods
  /**
   * Sets the two-factor authentication token
   */
  public setTwoFactorData(userId: string | number, token: string): void {
    localStorage.setItem(STORAGE_KEYS.TWO_FACTOR_TOKEN, token);
    localStorage.setItem(STORAGE_KEYS.TWO_FACTOR_USER_ID, userId.toString());
  }

  /**
   * Gets the two-factor authentication token
   */
  public getTwoFactorData(): { userId: string, token: string } {
    const userId = localStorage.getItem(STORAGE_KEYS.TWO_FACTOR_USER_ID);
    const token = localStorage.getItem(STORAGE_KEYS.TWO_FACTOR_TOKEN);
    return { userId, token };
  }

  /**
   * Removes the two-factor authentication token
   */
  public removeTwoFactorData(): void {
    localStorage.removeItem(STORAGE_KEYS.TWO_FACTOR_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.TWO_FACTOR_USER_ID);
  }

  /**
   * Gets the current user value from the subject
   */
  public get currentUserValue(): SignInUser {
    return this.currentUserSubject.value;
  }

  /**
   * Updates the current user in the subject
   */
  public updateCurrentUser(user: SignInUser): void {
    this.currentUserSubject.next(user);
  }
}
