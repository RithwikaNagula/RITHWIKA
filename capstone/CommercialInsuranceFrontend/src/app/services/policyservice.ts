// Wraps all policy lifecycle API calls: request quote, accept quote, renew, fetch by user/agent, and retrieve payment schedule.
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreatePolicyDto {
    customerId: string;
    planId: string;
    premiumAmount: number;
    startDate: string;
    endDate: string;
}

export interface PolicyDocumentDto {
    id: string;
    fileName: string;
    fileType: string;
    fileSize: number;
    filePath: string;
    uploadedAt: string;
}

export interface DocumentDto {
    id: string;
    fileName: string;
    fileType: string;
    fileSize: number;
    filePath: string;
    uploadedAt: string;
}

export interface PolicyDto {
    id: string;
    policyNumber: string;
    userId: string;
    customerName: string;
    planId: string;
    planName: string;
    agentId?: string;
    agentName?: string;
    planImageUrl?: string;
    selectedCoverageAmount: number;
    remainingCoverageAmount: number;
    premiumAmount: number;
    startDate: string;
    endDate: string;
    status: string;
    businessName: string;
    industry: string;
    employeeCount: number;
    annualRevenue: number;
    autoRenew: boolean;
    paymentFrequency: string;
    createdAt: string;
    documents: DocumentDto[];
}

export interface PaymentDto {
    id: string;
    policyId: string;
    policyNumber: string;
    amount: number;
    paymentDate: string;
    status: string;
    transactionId: string;
    invoiceInfo: string;
}

export interface PremiumCalculationDto {
    quoteId: string;
    quoteNumber: string;
    basePremium: number;
    industryMultiplier: number;
    employeeCountMultiplier: number;
    revenueMultiplier: number;
    agentCommissionPercentage: number;
    agentCommissionAmount: number;
    monthlyPremium: number;
    yearlyPremium: number;
    paymentFrequency: string;
    finalPremium: number;
}

@Injectable({
    providedIn: 'root'
})
export class PolicyService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5202/api/Policy';

    // this function sends a request to the server to create a new insurance policy for a customer
    // Submits a new policy request (multipart: form fields + document files) to the backend.
    createPolicy(formData: FormData): Observable<PolicyDto> {
        return this.http.post<PolicyDto>(`${this.apiUrl}/request-purchase`, formData);
    }

    getMyPolicies(): Observable<PolicyDto[]> {
        return this.http.get<PolicyDto[]>(`${this.apiUrl}/my-policies`);
    }

    getPolicy(id: string): Observable<PolicyDto> {
        return this.http.get<PolicyDto>(`${this.apiUrl}/${id}`);
    }

    // this function calculates how much the insurance will cost based on the plan and coverage details
    // Sends coverage amount and plan info to the backend calculation engine and returns a premium breakdown.
    calculatePremium(planId: string, coverageAmount: number, businessProfileId?: string, paymentFrequency: string = 'Monthly'): Observable<PremiumCalculationDto> {
        return this.http.post<PremiumCalculationDto>(`${this.apiUrl}/calculate-premium`, {
            planId,
            selectedCoverageAmount: coverageAmount,
            businessProfileId,
            paymentFrequency
        });
    }

    // Sends coverage amount and plan info to the backend calculation engine and returns a premium breakdown.
    agentCalculatePremium(planId: string, customerId: string, coverageAmount: number, paymentFrequency: string = 'Monthly'): Observable<PremiumCalculationDto> {
        return this.http.post<PremiumCalculationDto>(`${this.apiUrl}/agent/calculate-premium`, {
            planId,
            customerId,
            selectedCoverageAmount: coverageAmount,
            paymentFrequency
        });
    }

    issuePolicy(id: string): Observable<PolicyDto> {
        return this.http.post<PolicyDto>(`${this.apiUrl}/${id}/activate`, {});
    }

    // Agent action: moves a PendingReview policy to Approved, triggering a notification to the customer.
    approvePolicy(id: string): Observable<PolicyDto> {
        return this.http.post<PolicyDto>(`${this.apiUrl}/${id}/approve`, {});
    }

    rejectPolicy(id: string, reason: string): Observable<PolicyDto> {
        return this.http.post<PolicyDto>(`${this.apiUrl}/${id}/reject`, { reason });
    }

    getPaymentsForPolicy(policyId: string): Observable<PaymentDto[]> {
        return this.http.get<PaymentDto[]>(`http://localhost:5202/api/Payment/policy/${policyId}`);
    }

    toggleAutoRenew(id: string): Observable<PolicyDto> {
        return this.http.post<PolicyDto>(`${this.apiUrl}/${id}/toggle-autorenew`, {});
    }

    // Requests renewal of an Active or Expired policy; returns the new renewal policy record.
    renewPolicy(id: string): Observable<PolicyDto> {
        return this.http.post<PolicyDto>(`${this.apiUrl}/${id}/renew`, {});
    }

    getAllPolicies(): Observable<PolicyDto[]> {
        return this.http.get<PolicyDto[]>(`${this.apiUrl}/all`);
    }

    // this function requests the server to delete a specific policy from the database
    deletePolicy(id: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
