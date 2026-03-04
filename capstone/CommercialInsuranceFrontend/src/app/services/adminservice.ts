// this service provides all the administrative tools required to manage users and view system stats from the dashboard
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
    createAgent(dto: RegisterDto): Observable<UserDto> {
        return this.http.post<UserDto>(`${this.apiUrl}/create-agent`, dto);
    }

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
    deleteUser(userId: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/users/${userId}`);
    }

    // this function retrieves the overall summary of the business such as total money earned and active users
    getDashboardStats(): Observable<AdminDashboardDto> {
        return this.http.get<AdminDashboardDto>(`${this.apiUrl}/dashboard-stats`);
    }
}




