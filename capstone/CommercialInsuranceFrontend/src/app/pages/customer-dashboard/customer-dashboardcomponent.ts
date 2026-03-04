// Centralized portal for customers to view their active policies, manage claims, and access their notifications and profile.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PolicyService, PolicyDto } from '../../services/policyservice';
import { AuthService } from '../../services/authservice';
import { ClaimService, ClaimDto } from '../../services/claimservice';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './customer-dashboardcomponent.html',
  styleUrl: './customer-dashboardcomponent.css'
})
export class CustomerDashboardComponent implements OnInit {
  private policyService = inject(PolicyService);
  authService = inject(AuthService);
  private claimService = inject(ClaimService);

  policies = signal<PolicyDto[]>([]);
  claims = signal<ClaimDto[]>([]);

  activePolicies = computed(() => this.policies().filter(p => p.status === 'Active'));
  pendingPolicies = computed(() => this.policies().filter(p => p.status !== 'Active' && p.status !== 'Cancelled' && p.status !== 'Expired'));
  expiredPolicies = computed(() => this.policies().filter(p => p.status === 'Expired'));

  // Helper: policy expiring within 30 days
  isExpiringSoon(endDate: string): boolean {
    const end = new Date(endDate);
    const diff = (end.getTime() - Date.now()) / (1000 * 60 * 60 * 24);
    return diff >= 0 && diff <= 30;
  }

  // Helper for tracking timelines
  getClaimProgress(status: string): number {
    switch (status) {
      case 'Submitted': return 25;
      case 'UnderInvestigation': return 50;
      case 'Approved': return 75;
      case 'Rejected': return 100;
      case 'Settled': return 100;
      default: return 0;
    }
  }

  getClaimStatusDate(claim: ClaimDto, status: string): string | null {
    if (status === 'Submitted') {
      return claim.createdAt;
    }

    if (!claim.history || claim.history.length === 0) return null;

    // Sort ascending, find the status
    const log = claim.history
      .slice()
      .sort((a, b) => new Date(a.changedAt).getTime() - new Date(b.changedAt).getTime())
      .find(h => h.status === status);

    return log ? log.changedAt : null;
  }

  openClaimLog(id: string) {
    alert('Detailed claim logs are currently being integrated and will be available in the next system release.');
  }

  ngOnInit() {
    this.loadPolicies();
  }

  loadPolicies() {
    this.policyService.getMyPolicies().subscribe({
      next: (res) => {
        this.policies.set(res);
        this.loadClaims(res);
      },
      error: (err) => console.error('Failed to load portfolio', err)
    });
  }

  loadClaims(policies: PolicyDto[]) {
    policies.forEach(p => {
      this.claimService.getClaimsByPolicy(p.id).subscribe({
        next: (cls) => {
          if (cls.length > 0) {
            this.claims.update(current => [...current, ...cls]);
          }
        }
      });
    });
  }

  toggleAutoRenew(policyId: string) {
    this.policyService.toggleAutoRenew(policyId).subscribe({
      next: (updatedPolicy) => {
        this.policies.update(list => list.map(p => p.id === policyId ? updatedPolicy : p));
      },
      error: () => alert('Could not update Auto-Renew at this time.')
    });
  }

  renewPolicy(policyId: string, policyNumber: string) {
    if (!confirm(`Renew policy ${policyNumber}? A new policy will be created starting from the next period.`)) return;
    this.policyService.renewPolicy(policyId).subscribe({
      next: () => {
        alert('Policy renewed successfully! The new policy is pending payment.');
        this.loadPolicies();
      },
      error: (err) => {
        const msg = err?.error?.message || err?.message || 'Failed to renew policy.';
        alert('Error: ' + msg);
      }
    });
  }
}







