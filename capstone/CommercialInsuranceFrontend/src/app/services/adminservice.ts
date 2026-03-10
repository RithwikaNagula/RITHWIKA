// Wraps all admin API calls: manage agents and claims officers, fetch admin dashboard metrics, and retrieve the user directory.
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDto } from './authservice';

export interface RegisterDto {
    fullName: string;
    email: string;
    password?: string;
}

export interface AdminDashboardDto {
    totalUsers: number;
    totalAgents: number;
    totalClaimsOfficers: number;
    totalPolicies: number;
    totalClaims: number;
    totalRevenue: number;
    activePolicies: number;
    pendingClaims: number;
}

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private apiUrl = 'http://localhost:5202/api/Admin';

    constructor(private http: HttpClient) { }

    // this function sends a request to create a new agent account with the provided details
    // Admin-only: provisions a new Agent account; requires full name, email, and a temporary password.
  createAgent(dto: RegisterDto): Observable<UserDto> {
        return this.http.post<UserDto>(`${this.apiUrl}/create-agent`, dto);
    }

    // Admin-only: provisions a new ClaimsOfficer account with the same required fields as createAgent.
  createClaimsOfficer(dto: RegisterDto): Observable<UserDto> {
        return this.http.post<UserDto>(`${this.apiUrl}/create-claims-officer`, dto);
    }

    getAgents(): Observable<UserDto[]> {
        return this.http.get<UserDto[]>(`${this.apiUrl}/agents`);
    }

    getClaimsOfficers(): Observable<UserDto[]> {
        return this.http.get<UserDto[]>(`${this.apiUrl}/claims-officers`);
    }

    // this function removes a user from the system based on their unique identity
    // Admin-only: permanently removes a user; the backend reassigns their customers/policies before deletion.
  deleteUser(userId: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/users/${userId}`);
    }

    // this function retrieves the overall summary of the business such as total money earned and active users
    // Fetches KPI totals (users per role, active policies, revenue) for the admin dashboard header cards.
  getDashboardStats(): Observable<AdminDashboardDto> {
        return this.http.get<AdminDashboardDto>(`${this.apiUrl}/dashboard-stats`);
    }
}




