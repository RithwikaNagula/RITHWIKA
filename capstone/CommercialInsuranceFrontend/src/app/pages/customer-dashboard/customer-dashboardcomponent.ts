// Customer dashboard: displays active/pending/expired policies, claims lifecycle tracker with stepper progress, metric cards, and a profile edit panel.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PolicyService, PolicyDto } from '../../services/policyservice';
import { AuthService } from '../../services/authservice';
import { ClaimService, ClaimDto } from '../../services/claimservice';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './customer-dashboardcomponent.html',
  styleUrl: './customer-dashboardcomponent.css'
})
export class CustomerDashboardComponent implements OnInit {
  private policyService = inject(PolicyService);
  authService = inject(AuthService);
  private claimService = inject(ClaimService);

  // State and data property: policies
  policies = signal<PolicyDto[]>([]);
  // State and data property: claims
  claims = signal<ClaimDto[]>([]);
  // State and data property: isEditingProfile
  isEditingProfile = signal(false);
  activeTab = signal<'overview' | 'policies' | 'claims'>('overview');
  profileForm = {
    fullName: '',
    email: ''
  };
  loading = signal(false);

  // State and data property: activePolicies
  activePolicies = computed(() => this.policies().filter(p => p.status === 'Active'));
  // State and data property: pendingPolicies
  pendingPolicies = computed(() => this.policies().filter(p => p.status !== 'Active' && p.status !== 'Cancelled' && p.status !== 'Expired'));
  // State and data property: expiredPolicies
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
      // State and data property: default
      default: return 0;
    }
  }

  // Retrieves and populates required data for getClaimStatusDate
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

  // Executes core logic for openClaimLog
  openClaimLog(id: string) {
    alert('Detailed claim logs are currently being integrated and will be available in the next system release.');
  }

  getSteps(claim: ClaimDto): { label: string; done: boolean; rejected: boolean; date: string | null }[] {
    const status = claim.status;
    const progress = this.getClaimProgress(status);
    const isRejected = status === 'Rejected';

    return [
      { label: 'Submitted', done: progress >= 25, rejected: false, date: this.getClaimStatusDate(claim, 'Submitted') },
      { label: 'Review', done: progress >= 50, rejected: false, date: this.getClaimStatusDate(claim, 'UnderInvestigation') },
      { label: isRejected ? 'Rejected' : 'Approved', done: progress >= 75, rejected: isRejected && progress >= 75, date: this.getClaimStatusDate(claim, isRejected ? 'Rejected' : 'Approved') },
      { label: 'Settled', done: progress === 100 && !isRejected, rejected: false, date: this.getClaimStatusDate(claim, 'Settled') }
    ];
  }
  // Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.authService.getProfile().subscribe();
    this.loadPolicies();
  }
  // Fetches the policy list scoped to current role (Agent or Customer) to render within dashboard tables

  loadPolicies() {
    this.policyService.getMyPolicies().subscribe({
      next: (res) => {
        this.policies.set(res);
        this.loadClaims(res);
      },
      error: (err) => console.error('Failed to load portfolio', err)
    });
  }

  // Retrieves and populates required data for loadClaims
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

  // Toggles visibility or state flag for toggleAutoRenew
  toggleAutoRenew(policyId: string) {
    this.policyService.toggleAutoRenew(policyId).subscribe({
      next: (updatedPolicy) => {
        this.policies.update(list => list.map(p => p.id === policyId ? updatedPolicy : p));
      },
      error: () => alert('Could not update Auto-Renew at this time.')
    });
  }

  // Executes core logic for renewPolicy
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

  // Toggles visibility or state flag for toggleProfileEdit
  toggleProfileEdit() {
    const user = this.authService.currentUser();
    if (user) {
      this.profileForm = {
        fullName: user.fullName,
        email: user.email
      };
      this.isEditingProfile.set(!this.isEditingProfile());
    }
  }

  // Processes form submission and persists changes for onSubmit
  onSubmit() {
    this.loading.set(true);
    this.authService.updateProfile(this.profileForm).subscribe({
      next: (updatedUser) => {
        this.loading.set(false);
        alert('Profile updated successfully!');
        this.isEditingProfile.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message || err?.message || 'Failed to update profile.';
        alert('Error: ' + msg);
      }
    });
  }
}






