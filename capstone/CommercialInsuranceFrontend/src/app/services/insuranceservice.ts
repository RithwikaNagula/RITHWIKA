// Implementing core module functionality and external dependencies.
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InsuranceTypeDto {
    id: string;
    typeName: string;
    description: string;
    isActive: boolean;
    createdAt: string;
}

export interface CreateInsuranceTypeDto {
    typeName: string;
    description: string;
}

export interface PlanDto {
    id: string;
    planName: string;
    description: string;
    minCoverageAmount: number;
    maxCoverageAmount: number;
    basePremium: number;
    durationInMonths: number;
    insuranceTypeId: string;
    insuranceTypeName?: string;
    isActive: boolean;
    createdAt: string;
}

export interface CreatePlanDto {
    planName: string;
    description: string;
    minCoverageAmount: number;
    maxCoverageAmount: number;
    basePremium: number;
    durationInMonths: number;
    insuranceTypeId: string;
}

@Injectable({
    providedIn: 'root'
})
export class InsuranceService {
    private typeUrl = 'http://localhost:5202/api/InsuranceType';
    private planUrl = 'http://localhost:5202/api/Plan';

    constructor(private http: HttpClient) { }

    createInsuranceType(dto: CreateInsuranceTypeDto): Observable<InsuranceTypeDto> {
        return this.http.post<InsuranceTypeDto>(this.typeUrl, dto);
    }

    getAllInsuranceTypes(): Observable<InsuranceTypeDto[]> {
        return this.http.get<InsuranceTypeDto[]>(this.typeUrl);
    }

    createPlan(dto: CreatePlanDto): Observable<PlanDto> {
        return this.http.post<PlanDto>(this.planUrl, dto);
    }

    getAllPlans(): Observable<PlanDto[]> {
        return this.http.get<PlanDto[]>(this.planUrl);
    }

    getPlansByType(typeId: string): Observable<PlanDto[]> {
        return this.http.get<PlanDto[]>(`${this.planUrl}/by-type/${typeId}`);
    }

    deleteInsuranceType(id: string): Observable<any> {
        return this.http.delete(`${this.typeUrl}/${id}`);
    }

    deletePlan(id: string): Observable<any> {
        return this.http.delete(`${this.planUrl}/${id}`);
    }
}




