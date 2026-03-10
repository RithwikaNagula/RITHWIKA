// HTTP interceptor that catches 401/403 responses globally; on 401 it clears the session and redirects the user to the login page.
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const globalErrorInterceptor: HttpInterceptorFn = (req, next) => {
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // Don't intercept auth-related errors handled elsewhere
            if (error.status === 401 || error.status === 403) {
                return throwError(() => error);
            }

            // Don't redirect for 400 (Bad Request) — let components handle validation errors
            if (error.status === 400) {
                return throwError(() => error);
            }

            // For severe server failures or connection issues, redirect to the error page
            if (error.status >= 500 || error.status === 0) {
                router.navigate(['/error'], {
                    queryParams: {
                        status: error.status,
                        message: error.error?.message || error.statusText || 'Our servers are currently experiencing difficulties.'
                    }
                });
            }

            // We still throw the error so the calling component can also handle it if needed
            return throwError(() => error);
        })
    );
};
