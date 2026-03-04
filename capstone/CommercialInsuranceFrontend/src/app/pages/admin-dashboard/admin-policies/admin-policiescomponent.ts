// Administrative view for overseeing all customer policies, monitoring their statuses, and tracking agent assignments.
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
    policies = signal<PolicyDto[]>([]);
    claims = signal<ClaimDto[]>([]);
    currentTab: 'Policies' | 'Claims' = 'Policies';

    constructor(
        private policyService: PolicyService,
        private claimService: ClaimService
    ) { }

    ngOnInit() {
        this.loadData();
    }

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


