// Manages session state: stores/retrieves the JWT and user object from localStorage, exposes a reactive currentUser signal, and wraps login/register/logout API calls.
import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface UserDto {
    id: string;
    fullName: string;
    email: string;
    role: string;
    createdAt: string;
    assignedAgentId?: string;
    assignedAgentName?: string;
    assignedClaimsOfficerId?: string;
    assignedClaimsOfficerName?: string;
}

export interface LoginResponse {
    token: string;
    user: UserDto;
}

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private apiUrl = 'http://localhost:5202/api/Auth';

    currentUser = signal<UserDto | null>(null);

    constructor(private http: HttpClient) {
        // on setup we check if there is a saved user in the local storage to keep them logged in
        const user = localStorage.getItem('user');
        if (user) {
            this.currentUser.set(JSON.parse(user));
        }
    }

    // this function sends the user credentials to the server to start a new session
    login(credentials: any): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
            tap(res => {
                localStorage.setItem('token', res.token);
                localStorage.setItem('user', JSON.stringify(res.user));
                this.currentUser.set(res.user);
            })
        );
    }

    // Registers a new customer; does NOT auto-login — redirects to the login page after success.
    register(dto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/register`, dto);
    }

    // this function clears the saved user data to log them out of the application
    // Clears the JWT and user data from localStorage and resets the currentUser signal to null.
    // Invokes the AuthService to safely purge the local JWT session token and securely navigate to the login route
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        this.currentUser.set(null);
    }

    // Reads the raw JWT string from localStorage; used by the auth interceptor to attach it to requests.
    getToken(): string | null {
        return localStorage.getItem('token');
    }

    isAuthenticated(): boolean {
        return !!this.// Reads the raw JWT string from localStorage; used by the auth interceptor to attach it to requests.
            getToken();
    }

    isAdmin(): boolean {
        return this.currentUser()?.role === 'Admin';
    }

    isAgent(): boolean {
        return this.currentUser()?.role === 'Agent';
    }

    isCustomer(): boolean {
        return this.currentUser()?.role === 'Customer';
    }

    isClaimsOfficer(): boolean {
        return this.currentUser()?.role === 'ClaimsOfficer';
    }

    forgotPassword(email: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/forgot-password`, { email });
    }

    resetPassword(dto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/reset-password`, dto);
    }

    updateProfile(dto: any): Observable<UserDto> {
        return this.http.put<UserDto>(`${this.apiUrl}/update-profile`, dto).pipe(
            tap(user => {
                localStorage.setItem('user', JSON.stringify(user));
                this.currentUser.set(user);
            })
        );
    }

    getProfile(): Observable<UserDto> {
        return this.http.get<UserDto>(`${this.apiUrl}/profile`).pipe(
            tap(user => {
                localStorage.setItem('user', JSON.stringify(user));
                this.currentUser.set(user);
            })
        );
    }
}
