// Wraps agent dashboard API calls: fetch assigned policy portfolio, commission stats, and client list.
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


export interface AgentDashboardDto {
    assignedPolicies: number;
    conversionRate: number;
    commissionEarned: number;
    activeCustomers: number;
}

@Injectable({
    providedIn: 'root'
})
export class AgentService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5202/api/AgentDashboard';

    getDashboardStats(): Observable<AgentDashboardDto> {
        return this.http.get<AgentDashboardDto>(`${this.apiUrl}/stats`);
    }
}




