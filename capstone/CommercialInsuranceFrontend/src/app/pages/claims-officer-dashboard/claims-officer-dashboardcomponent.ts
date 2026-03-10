// Claims officer dashboard: pending claim count, resolved-today count, and an audit worklist where officers can approve or reject individual claims.
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

// State and data property: claims
  claims = signal<ClaimDto[]>([]);

// State and data property: pendingClaimsCount
  pendingClaimsCount = computed(() => this.claims().filter(c => c.status === 'Submitted' || c.status === 'UnderReview').length);
// State and data property: resolvedTodayCount
  resolvedTodayCount = computed(() => this.claims().filter(c => c.status === 'Approved' || c.status === 'Rejected' || c.status === 'Settled').length);
// Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.loadClaims();
  }
// Connects to the ClaimService to retrieve claims relevant to the current user or officer

  loadClaims() {
    this.claimService.getMyReviews().subscribe({
      next: (res) => this.claims.set(res),
      error: (err) => console.error('Failed to load officer claims', err)
    });
  }

// Event listener hook triggered by onReview
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

// Retrieves and populates required data for getStatusClass
  getStatusClass(status: string) {
    switch (status) {
      case 'Settled':
      case 'Approved': return 'bg-green-100 text-green-700';
      case 'Rejected': return 'bg-red-100 text-red-700';
      case 'UnderReview': return 'bg-yellow-100 text-yellow-700';
      case 'Submitted': return 'bg-brand-100 text-brand-800';
// State and data property: default
      default: return 'bg-brand-100 text-brand-400';
    }
  }
}







