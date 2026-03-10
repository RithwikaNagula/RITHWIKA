// Displays available plans for a chosen insurance type; allows the customer to select a plan and proceed to quote generation.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { InsuranceService, PlanDto } from '../../../services/insuranceservice';
import { AuthService } from '../../../services/authservice';
import { BusinessProfileService, BusinessProfileDto } from '../../../services/businessprofileservice';

@Component({
  selector: 'app-plan-selection',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './plan-selectioncomponent.html',
  styleUrl: './plan-selectioncomponent.css'
})
export class PlanSelectionComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private insuranceService = inject(InsuranceService);
  private authService = inject(AuthService);
  private profileService = inject(BusinessProfileService);

  // State and data property: plans
  plans = signal<PlanDto[]>([]);
  // State and data property: loadingQuote
  loadingQuote = signal<string | null>(null);
  // State and data property: selectedPlan
  selectedPlan = signal<PlanDto | null>(null);

  // Filter Signals
  searchText = signal('');
  priceLimit = signal<number>(1000000); // 10L Default Max
  // State and data property: sortBy
  sortBy = signal<'none' | 'premium-asc' | 'premium-desc' | 'coverage-desc'>('none');

  filteredPlans = computed(() => {
    let list = [...this.plans()];

    // 1. Text Search
    if (this.searchText()) {
      const query = this.searchText().toLowerCase().trim();
      list = list.filter(p =>
        p.planName.toLowerCase().includes(query) ||
        p.description.toLowerCase().includes(query)
      );
    }

    // 2. Price Limit
    list = list.filter(p => p.basePremium <= this.priceLimit());

    // 3. Sorting
    if (this.sortBy() === 'premium-asc') {
      list.sort((a, b) => a.basePremium - b.basePremium);
    } else if (this.sortBy() === 'premium-desc') {
      list.sort((a, b) => b.basePremium - a.basePremium);
    } else if (this.sortBy() === 'coverage-desc') {
      list.sort((a, b) => b.maxCoverageAmount - a.maxCoverageAmount);
    }

    return list;
  });
  // Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.route.params.subscribe(params => {
      const typeId = params['typeId'];
      if (typeId) {
        this.loadPlans(typeId);
      }
    });
  }

  // Retrieves and populates required data for loadPlans
  loadPlans(typeId: string) {
    this.insuranceService.getPlansByType(typeId).subscribe({
      next: (res) => this.plans.set(res),
      error: (err) => console.error('Error fetching plans', err)
    });
  }

  // Event listener hook triggered by onGenerateQuote
  onGenerateQuote(plan: PlanDto) {
    // 1. Check Authentication
    if (!this.authService.isAuthenticated()) {
      localStorage.setItem('returnUrl', this.router.url);
      this.router.navigate(['/login']);
      // State and data property: return
      return;
    }

    this.loadingQuote.set(plan.id);

    // 2. Check Business Profile array
    this.profileService.getProfiles().subscribe({
      next: (profiles: BusinessProfileDto[]) => {
        this.loadingQuote.set(null);
        if (profiles && profiles.length > 0) {
          // 3. Move to Quote Summary if at least one profile exists
          this.router.navigate(['/quotes/generate'], { queryParams: { planId: plan.id } });
        } else {
          localStorage.setItem('returnUrl', `/quotes/generate?planId=${plan.id}`);
          this.router.navigate(['/business-profile/create']);
        }
      },
      error: (err: any) => {
        this.loadingQuote.set(null);
        if (err.status === 404) {
          localStorage.setItem('returnUrl', this.router.url);
          this.router.navigate(['/business-profile/create']);
        } else if (err.status === 403) {
          alert("Only Customers can generate quotes. Please sign out and sign in with a Customer account.");
        } else {
          console.error('Failed to verify profiles', err);
          alert("An unexpected error occurred while verifying your profile. Please try again.");
        }
      }
    });
  }

  // Retrieves and populates required data for getBenefits
  getBenefits(description: string): string[] {
    if (!description) return ['Comprehensive coverage', '24/7 Support', 'Fast claims processing'];
    // Split by period or semicolon if exists, otherwise return as single list item
    const splits = description.split(/[.;\n]/).map(s => s.trim()).filter(s => s.length > 0);
    return splits.length > 0 ? splits : [description];
  }

  // Opens detailed modal for a specific plan
  openDetails(plan: PlanDto) {
    this.selectedPlan.set(plan);
    // Prevent background scrolling
    document.body.style.overflow = 'hidden';
  }

  // Closes detailed modal
  closeDetails() {
    this.selectedPlan.set(null);
    document.body.style.overflow = 'auto';
  }
}





