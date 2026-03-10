// Route guard that checks for a valid JWT and the required user role before allowing navigation; redirects to /login if not authenticated.
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/authservice';

export const adminGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isAuthenticated() && authService.isAdmin()) {
        return true;
    }

    router.navigate(['/login']);
    return false;
};



