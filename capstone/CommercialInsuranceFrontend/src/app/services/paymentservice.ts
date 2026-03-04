// Implementing core module functionality and external dependencies.
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ───── Interfaces ─────
export interface CreatePaymentDto {
    policyId: string;
    paymentFrequency: string;
    paymentMode: string;
    paidByUserId?: string;
}

export interface PaymentResponseDto {
    id: string;
    policyId: string;
    policyNumber: string;
    invoiceNumber: string;
    amount: number;
    annualPremium: number;
    totalPremium: number;
    paymentFrequency: string;
    paymentMode: string;
    installmentNumber: number;
    totalInstallments: number;
    dueDate: string;
    paymentDate: string;
    status: string;
    transactionId: string;
    customerName: string;
    businessName: string;
    planName: string;
    insuranceTypeName: string;
    selectedCoverageAmount: number;
}

export interface InvoiceDto {
    invoiceNumber: string;
    policyNumber: string;
    customerName: string;
    businessName: string;
    insuranceTypeName: string;
    planName: string;
    selectedCoverageAmount: number;
    annualPremium: number;
    installmentAmount: number;
    installmentNumber: number;
    totalInstallments: number;
    paymentFrequency: string;
    paymentMode: string;
    paymentDate: string;
    dueDate: string;
    transactionId: string;
    status: string;
}

export interface InstallmentDto {
    paymentId: string;
    installmentNumber: number;
    invoiceNumber: string;
    amount: number;
    dueDate: string;
    status: string;
    paidAt?: string;
}

export interface PaymentScheduleDto {
    policyId: string;
    policyNumber: string;
    annualPremium: number;
    installmentAmount: number;
    paymentFrequency: string;
    totalInstallments: number;
    schedule: InstallmentDto[];
}

export interface PremiumBreakdown {
    annual: number;
    monthly: number;
    quarterly: number;
    halfYearly: number;
    annually: number;
}

@Injectable({
    providedIn: 'root'
})
export class PaymentService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5202/api/Payment';

    initiatePayment(dto: CreatePaymentDto): Observable<PaymentResponseDto> {
        return this.http.post<PaymentResponseDto>(this.apiUrl, dto);
    }

    getPaymentById(id: string): Observable<PaymentResponseDto> {
        return this.http.get<PaymentResponseDto>(`${this.apiUrl}/${id}`);
    }

    getInvoice(paymentId: string): Observable<InvoiceDto> {
        return this.http.get<InvoiceDto>(`${this.apiUrl}/${paymentId}/invoice`);
    }

    getPaymentsByPolicy(policyId: string): Observable<PaymentResponseDto[]> {
        return this.http.get<PaymentResponseDto[]>(`${this.apiUrl}/policy/${policyId}/all`);
    }

    getPaymentSchedule(policyId: string): Observable<PaymentScheduleDto> {
        return this.http.get<PaymentScheduleDto>(`${this.apiUrl}/policy/${policyId}/schedule`);
    }

    calculatePremium(coverageAmount: number): PremiumBreakdown {
        const annual = (coverageAmount / 100) * 5;
        return {
            annual,
            monthly: Math.round((annual / 12) * 100) / 100,
            quarterly: Math.round((annual / 4) * 100) / 100,
            halfYearly: Math.round((annual / 2) * 100) / 100,
            annually: annual
        };
    }
}
