// Dedicated workspace for claims officers to review, process, and update the status of submitted customer claims.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClaimService, ClaimDto } from '../../services/claimservice';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-claims-officer-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './claims-officer-dashboardcomponent.html',
  styleUrl: './claims-officer-dashboardcomponent.css'
})
export class ClaimsOfficerDashboardComponent implements OnInit {
  private claimService = inject(ClaimService);

  claims = signal<ClaimDto[]>([]);

  pendingClaimsCount = computed(() => this.claims().filter(c => c.status === 'Submitted' || c.status === 'UnderReview').length);
  resolvedTodayCount = computed(() => this.claims().filter(c => c.status === 'Approved' || c.status === 'Rejected' || c.status === 'Settled').length);

  ngOnInit() {
    this.loadClaims();
  }

  loadClaims() {
    this.claimService.getMyReviews().subscribe({
      next: (res) => this.claims.set(res),
      error: (err) => console.error('Failed to load officer claims', err)
    });
  }

  onReview(claim: ClaimDto, resolution: string) {
    const finalStatus = resolution === 'Approved' ? 'Settled' : 'Rejected';
    const remarks = resolution === 'Approved' ? 'Claim verified and settled.' : 'Claim rejected after review.';
    this.claimService.reviewClaim(claim.id, finalStatus, remarks).subscribe({
      next: () => {
        this.loadClaims();
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to update claim status');
      }
    });
  }

  getStatusClass(status: string) {
    switch (status) {
      case 'Settled':
      case 'Approved': return 'bg-green-100 text-green-700';
      case 'Rejected': return 'bg-red-100 text-red-700';
      case 'UnderReview': return 'bg-yellow-100 text-yellow-700';
      case 'Submitted': return 'bg-taupe-100 text-taupe-800';
      default: return 'bg-taupe-100 text-taupe-400';
    }
  }
}








