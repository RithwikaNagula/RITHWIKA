// Fetches system-level analytics data (revenue trends, claim volumes, user growth) for the admin analytics dashboard.
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AgentPerformanceDto {
    agentName: string;
    totalCommission: number;
}

export interface ClaimsPerformanceDto {
    officerName: string;
    totalSettlements: number;
}

export interface ClaimsAnalyticsDto {
    officerPerformances: ClaimsPerformanceDto[];
    approvalRate: number;
    totalClaims: number;
    approvedClaims: number;
}

export interface RevenueAnalyticsDto {
    totalPremiumCollected: number;
    totalSettlementsPaid: number;
    netRevenue: number;
    totalPolicies: number;
    totalClaims: number;
    currentPeriodRevenue: number;
    previousPeriodRevenue: number;
    growthPercentage: number;
    revenueTrend: { [key: string]: number };
}

@Injectable({
    providedIn: 'root'
})
export class AnalyticsService {
    private apiUrl = 'http://localhost:5202/api/Analytics';

    constructor(private http: HttpClient) { }

    getAgentPerformance(): Observable<AgentPerformanceDto[]> {
        return this.http.get<AgentPerformanceDto[]>(`${this.apiUrl}/agent-performance`);
    }

    getClaimsPerformance(): Observable<ClaimsAnalyticsDto> {
        return this.http.get<ClaimsAnalyticsDto>(`${this.apiUrl}/claims-performance`);
    }

    getRevenueAnalytics(period: 'monthly' | 'yearly'): Observable<RevenueAnalyticsDto> {
        const params = new HttpParams().set('period', period);
        return this.http.get<RevenueAnalyticsDto>(`${this.apiUrl}/revenue`, { params });
    }
}
