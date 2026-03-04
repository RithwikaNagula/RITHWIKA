// Implementing core module functionality and external dependencies.
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateClaimDto {
    description: string;
    claimAmount: number;
}

export interface ClaimDto {
    id: string;
    claimNumber: string;
    policyId: string;
    policyNumber: string;
    description: string;
    claimAmount: number;
    status: string;
    claimsOfficerId?: string;
    claimsOfficerName?: string;
    createdAt: string;
    resolvedAt?: string;
    history: ClaimHistoryLogDto[];
    documents: DocumentDto[];
}

export interface ClaimHistoryLogDto {
    id: string;
    claimId: string;
    status: string;
    remarks: string;
    changedByUserId: string;
    changedByUserName: string;
    changedAt: string;
}

export interface DocumentDto {
    id: string;
    fileName: string;
    fileType: string;
    fileSize: number;
    filePath: string;
    uploadedAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class ClaimService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5202/api/Claim';

    fileClaim(policyId: string, data: FormData): Observable<ClaimDto> {
        return this.http.post<ClaimDto>(`${this.apiUrl}/${policyId}`, data);
    }

    getClaimsByPolicy(policyId: string): Observable<ClaimDto[]> {
        return this.http.get<ClaimDto[]>(`${this.apiUrl}/by-policy/${policyId}`);
    }

    getMyReviews(): Observable<ClaimDto[]> {
        return this.http.get<ClaimDto[]>(`${this.apiUrl}/my-assignments`);
    }

    getAllClaims(): Observable<ClaimDto[]> {
        return this.http.get<ClaimDto[]>(this.apiUrl);
    }

    reviewClaim(claimId: string, status: string, remarks: string = "Status updated"): Observable<ClaimDto> {
        return this.http.put<ClaimDto>(`${this.apiUrl}/${claimId}/status`, { status, remarks });
    }
}




