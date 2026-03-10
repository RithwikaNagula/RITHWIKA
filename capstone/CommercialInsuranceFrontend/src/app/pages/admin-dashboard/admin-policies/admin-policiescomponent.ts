// Admin view of all policies in the system; supports filtering by status and reassigning policies to agents.
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PolicyService, PolicyDto } from '../../../services/policyservice';
import { ClaimService, ClaimDto } from '../../../services/claimservice';

@Component({
    selector: 'app-admin-policies',
    styleUrl: './admin-policiescomponent.css',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './admin-policiescomponent.html'
})
export class AdminPoliciesComponent implements OnInit {
// State and data property: policies
    policies = signal<PolicyDto[]>([]);
// State and data property: claims
    claims = signal<ClaimDto[]>([]);
// State and data property: currentTab
    currentTab: 'Policies' | 'Claims' = 'Policies';

    constructor(
        private policyService: PolicyService,
        private claimService: ClaimService
    ) { }
// Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        this.loadData();
    }
// Centralized function responsible for fetching server state and populating local component signals

    loadData() {
        this.policyService.getAllPolicies().subscribe({
            next: (res) => this.policies.set(res),
            error: (err) => console.error('Failed to load policies', err)
        });
        this.claimService.getAllClaims().subscribe({
            next: (res) => this.claims.set(res),
            error: (err) => console.error('Failed to load claims', err)
        });
    }

// Handles secure deletion or clearance sequence for deletePolicy
    deletePolicy(id: string, number: string) {
        if (confirm(`Are you sure you want to permanently delete policy ${number || id}?`)) {
            this.policyService.deletePolicy(id).subscribe({
                next: () => {
                    this.loadData();
                },
                error: (err) => {
                    alert('Failed to delete policy: ' + (err.error?.message || err.message));
                }
            });
        }
    }
}

