// Implementing core module functionality and external dependencies.
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface BusinessProfileDto {
    id: string;
    userId: string;
    businessName: string;
    industry: string;
    annualRevenue: number;
    employeeCount: number;
    city: string;
    isProfileCompleted: boolean;
    createdAt: string;
}

export interface CreateBusinessProfileDto {
    businessName: string;
    industry: string;
    annualRevenue: number;
    employeeCount: number;
    city: string;
}

@Injectable({
    providedIn: 'root'
})
export class BusinessProfileService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5202/api/customer/business-profile';

    currentProfile = signal<BusinessProfileDto | null>(null);

    getProfiles(): Observable<BusinessProfileDto[]> {
        return this.http.get<BusinessProfileDto[]>(this.apiUrl);
    }

    getProfile(id?: string): Observable<BusinessProfileDto> {
        const url = id ? `${this.apiUrl}/${id}` : this.apiUrl;
        return this.http.get<BusinessProfileDto>(url).pipe(
            tap(profile => this.currentProfile.set(profile))
        );
    }

    createProfile(dto: CreateBusinessProfileDto): Observable<BusinessProfileDto> {
        return this.http.post<BusinessProfileDto>(this.apiUrl, dto).pipe(
            tap(profile => this.currentProfile.set(profile))
        );
    }

    updateProfile(dto: CreateBusinessProfileDto): Observable<BusinessProfileDto> {
        return this.http.put<BusinessProfileDto>(this.apiUrl, dto).pipe(
            tap(profile => this.currentProfile.set(profile))
        );
    }
}




