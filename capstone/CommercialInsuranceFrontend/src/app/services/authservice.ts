// this service handles everything related to user login registration and identity management
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

    register(dto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/register`, dto);
    }

    // this function clears the saved user data to log them out of the application
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        this.currentUser.set(null);
    }

    getToken(): string | null {
        return localStorage.getItem('token');
    }

    isAuthenticated(): boolean {
        return !!this.getToken();
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
}
