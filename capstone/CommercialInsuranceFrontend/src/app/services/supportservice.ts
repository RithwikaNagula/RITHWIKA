import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface SupportInquiry {
    id: string;
    fullName: string;
    email: string;
    message: string;
    createdAt: string;
    isResolved: boolean;
}

export interface CreateInquiryDto {
    fullName: string;
    email: string;
    message: string;
}

@Injectable({
    providedIn: 'root'
})
export class SupportService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5202/api/SupportInquiry';

    submitInquiry(dto: CreateInquiryDto): Observable<SupportInquiry> {
        return this.http.post<SupportInquiry>(this.apiUrl, dto);
    }

    getAllInquiries(): Observable<SupportInquiry[]> {
        return this.http.get<SupportInquiry[]>(this.apiUrl);
    }

    resolveInquiry(id: string): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}/resolve`, {});
    }
}
