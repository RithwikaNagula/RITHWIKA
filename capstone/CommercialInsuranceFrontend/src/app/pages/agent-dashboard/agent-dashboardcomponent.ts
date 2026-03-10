// Agent dashboard: shows KPI cards (policies, conversion rate, commission, customers), policy portfolio table with approve/issue actions, and a claims overview table.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PolicyService, PolicyDto } from '../../services/policyservice';
import { AgentService, AgentDashboardDto } from '../../services/agentservice';
import { ClaimService, ClaimDto } from '../../services/claimservice';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './agent-dashboardcomponent.html',
  styleUrl: './agent-dashboardcomponent.css'
})
export class AgentDashboardComponent implements OnInit {
  private policyService = inject(PolicyService);
  private agentService = inject(AgentService);
  private claimService = inject(ClaimService);

  // State and data property: policies
  policies = signal<PolicyDto[]>([]);
  // State and data property: claims
  claims = signal<ClaimDto[]>([]);
  // State and data property: agentStats
  agentStats = signal<AgentDashboardDto | null>(null);
  // State and data property: processingId
  processingId = signal<string | null>(null);

  // Derives unique customer count from policies
  uniqueCustomers = computed(() => {
    const ids = this.policies().map(p => p.userId);
    return new Set(ids).size;
  });
  // Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.loadData();
  }
  // Centralized function responsible for fetching server state and populating local component signals

  loadData() {
    this.agentService.getDashboardStats().subscribe({
      next: (res) => this.agentStats.set(res),
      error: (err) => console.error('Failed to load agent stats', err)
    });

    this.policyService.getMyPolicies().subscribe({
      next: (res) => {
        this.policies.set(res);
        this.claims.set([]);
        res.forEach(p => {
          this.claimService.getClaimsByPolicy(p.id).subscribe({
            next: (cls) => {
              if (cls.length > 0) {
                this.claims.update(current => [...current, ...cls]);
              }
            }
          });
        });
      },
      error: (err) => console.error('Failed to load agent policies', err)
    });
  }

  // Executes core logic for pendingPoliciesCount
  pendingPoliciesCount() {
    return this.policies().filter(p => p.status === 'QuoteAccepted').length;
  }

  // Event listener hook triggered by onApprovePolicy
  onApprovePolicy(policy: PolicyDto) {
    if (!confirm(`Reviewing request for ${policy.customerName}. Industry: ${policy.industry}, Revenue: ₹${policy.annualRevenue}. Approve this business profile and documents?`)) return;

    this.processingId.set(policy.id);
    this.policyService.approvePolicy(policy.id).subscribe({
      next: () => {
        this.processingId.set(null);
        this.loadData();
      },
      error: (err) => {
        this.processingId.set(null);
        alert(err.error?.message || 'Failed to approve policy');
      }
    });
  }

  // Event listener hook triggered by onIssuePolicy
  onIssuePolicy(policy: PolicyDto) {
    this.processingId.set(policy.id);
    this.policyService.issuePolicy(policy.id).subscribe({
      next: () => {
        this.processingId.set(null);
        this.loadData(); // Refresh
      },
      error: (err) => {
        this.processingId.set(null);
        alert(err.error?.message || 'Failed to issue policy');
      }
    });
  }

  // Retrieves and populates required data for getStatusClass
  getStatusClass(status: string) {
    switch (status) {
      case 'Active': return 'bg-emerald-50 text-emerald-600 ring-emerald-100 dark:bg-emerald-900/20 dark:text-emerald-400 dark:ring-emerald-900/30';
      case 'PendingReview': return 'bg-amber-50 text-amber-600 ring-amber-100 dark:bg-amber-900/20 dark:text-amber-400 dark:ring-amber-900/30';
      case 'Approved': return 'bg-brand-50 text-brand-600 ring-brand-100 dark:bg-brand-900/20 dark:text-brand-400 dark:ring-brand-900/30';
      case 'QuoteAccepted': return 'bg-brand-50 text-brand-700 ring-brand-100 dark:bg-brand-900/20 dark:text-brand-300 dark:ring-brand-900/30';
      case 'QuoteGenerated': return 'bg-brand-50 text-brand-600 ring-brand-100 dark:bg-brand-900/20 dark:text-brand-400 dark:ring-brand-900/30';
      case 'Cancelled': return 'bg-rose-50 text-rose-600 ring-rose-100 dark:bg-rose-900/20 dark:text-rose-400 dark:ring-rose-900/30';
      default: return 'bg-brand-50 text-brand-500 ring-brand-100 dark:bg-brand-900/20 dark:text-brand-400 dark:ring-brand-900/30';
    }
  }
}




