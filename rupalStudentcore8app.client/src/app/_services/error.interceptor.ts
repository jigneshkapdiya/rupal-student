import { HTTP_INTERCEPTORS, HttpClient, HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from "app/shared/auth/auth.service";
import { environment } from "environments/environment";
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';

/**
 * HTTP Error Interceptor for handling authentication, authorization, and error processing
 * Automatically handles token refresh, validation errors, and standardizes error messages
 */
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private http: HttpClient, private authService: AuthService) {
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    //console.log('intercept request', request);
    // Here you can add the request to the header
    //if (request.url.includes('https://ifsc.razorpay.com')) {
    //  return next.handle(request);
    //}
    request = request.clone({
      setHeaders: {
        'Accept-Language': this.authService.getLanguage(),
        Authorization: "Bearer " + this.authService.getAccessToken(),
      }

    });
    return next.handle(request).pipe(catchError((error: any) => {
      if (error instanceof HttpErrorResponse) {
        // throw 401 error response
        if (error.status === 401) {
          console.log('error.status === 401');
          if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);
            console.log('Refresh Token request');
            return this.http.post(environment.ApiURL + "Account/Refresh", {
              RefreshToken: this.authService.getRefreshToken(),
              UserId: this.authService.getSignInUserId()
            }).pipe(
              switchMap((token: any) => {
                // console.log('Refresh Token switchMap Response', token);
                this.isRefreshing = false;
                if (token && token.accessToken && token.refreshToken) {
                  this.authService.setAccessToken(token.accessToken);
                  this.authService.setRefreshToken(token.refreshToken);
                }
                this.refreshTokenSubject.next(token);
                return next.handle(this.addHeader(request));
              }),
              catchError((err: any) => {
                // console.log('inside catch error', err);
                this.isRefreshing = false;
                this.authService.logout();
                return throwError(err);
              })

            );
          } else {
            console.log('inside else call');
            console.log('token : ', this.refreshTokenSubject.getValue());
            return this.refreshTokenSubject.pipe(
              filter(token => (token !== null && token !== undefined)),
              take(1),
              switchMap(() => {
                // console.log('adding header in else');
                return next.handle(this.addHeader(request));
              }));
          }
        }

        // throw 403 Forbidden error response
        if (error.status == 403) {
          // this.authService.redirectToLogin();
          return throwError(
            "The operation you are attempting is not authorized."
          );
        }

        // throw 404 error response
        if (error.status === 404) {
          // this.authService.logout();
          return throwError(
            "API not found. Contact Administrator."
          );
        }

        // Handle network connectivity errors
        if (error.status === 0) {
          return throwError("There was an error connecting to API.");
        }

        // Handle application-specific errors from response headers
        const applicationError = error.headers.get("Application-Error");
        if (applicationError) {
          return throwError(applicationError);
        }

        // Process server error response and extract meaningful error messages
        return throwError(this.extractErrorMessage(error.error));
      } else {
        console.log('error instanceof HttpErrorResponse else ', error);
        return throwError(error);
      }
    }));
  }

  /**
   * Extracts meaningful error messages from server error responses
   * Handles various error formats including validation errors, message properties, and fallbacks
   * @param serverError - The error object from the server response
   * @returns A formatted error message string
   */
  private extractErrorMessage(serverError: any): string {
    // Return default message if no server error provided
    if (!serverError) {
      return "Server Error";
    }

    // Handle validation errors structure: { errors: { "field": ["message1", "message2"] } }
    if (serverError && typeof serverError === "object" && serverError.errors) {
      const validationErrors = this.extractValidationErrors(serverError.errors);
      if (validationErrors.length > 0) {
        return validationErrors.join(", ");
      }
    }

    // Handle standard error message properties
    if (serverError.message) {
      return serverError.message;
    }

    if (serverError.title) {
      return serverError.title;
    }

    // Handle string errors directly
    if (typeof serverError === 'string') {
      return serverError;
    }

    // Fallback for unhandled error formats
    return "Server Error";
  }

  /**
   * Extracts validation error messages from the errors object
   * @param errors - The errors object containing field validation messages
   * @returns Array of validation error messages
   */
  private extractValidationErrors(errors: any): string[] {
    const validationErrors: string[] = [];

    if (!errors || typeof errors !== "object") {
      return validationErrors;
    }

    // Iterate through each field in the errors object
    for (const field in errors) {
      if (errors.hasOwnProperty(field) && errors[field]) {
        // Handle array of error messages for each field
        if (Array.isArray(errors[field])) {
          validationErrors.push(...errors[field]);
        }
        // Handle single error message
        else if (typeof errors[field] === 'string') {
          validationErrors.push(errors[field]);
        }
      }
    }

    return validationErrors;
  }

  /**
   * Adds authorization header to the request
   * @param request - The HTTP request to modify
   * @returns Modified request with authorization header
   */
  private addHeader(request: HttpRequest<any>): HttpRequest<any> {
    console.log('addHeader');
    const accessToken = this.authService.getAccessToken();
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    });
  }
}

export const ErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true,
};
