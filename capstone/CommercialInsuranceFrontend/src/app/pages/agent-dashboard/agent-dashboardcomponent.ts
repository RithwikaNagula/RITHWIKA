// Portal for insurance agents to monitor their assigned client portfolio and track the active policies they manage.
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

  policies = signal<PolicyDto[]>([]);
  claims = signal<ClaimDto[]>([]);
  agentStats = signal<AgentDashboardDto | null>(null);
  processingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

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

  pendingPoliciesCount() {
    return this.policies().filter(p => p.status === 'QuoteAccepted').length;
  }

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

  getStatusClass(status: string) {
    switch (status) {
      case 'Active': return 'bg-green-100 text-green-700';
      case 'PendingReview': return 'bg-orange-100 text-orange-700';
      case 'Approved': return 'bg-taupe-100 text-taupe-700';
      case 'QuoteAccepted': return 'bg-taupe-100 text-taupe-800';
      case 'QuoteGenerated': return 'bg-taupe-100 text-taupe-700';
      case 'Cancelled': return 'bg-red-100 text-red-700';
      default: return 'bg-taupe-50 text-taupe-500';
    }
  }
}








