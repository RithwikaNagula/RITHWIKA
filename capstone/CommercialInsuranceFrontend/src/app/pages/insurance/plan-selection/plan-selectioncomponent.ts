// Interface for customers to browse, compare, and select specific insurance plans within a chosen insurance type.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { InsuranceService, PlanDto } from '../../../services/insuranceservice';
import { AuthService } from '../../../services/authservice';
import { BusinessProfileService, BusinessProfileDto } from '../../../services/businessprofileservice';

@Component({
  selector: 'app-plan-selection',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './plan-selectioncomponent.html',
  styleUrl: './plan-selectioncomponent.css'
})
export class PlanSelectionComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private insuranceService = inject(InsuranceService);
  private authService = inject(AuthService);
  private profileService = inject(BusinessProfileService);

  plans = signal<PlanDto[]>([]);
  loadingQuote = signal<string | null>(null);

  ngOnInit() {
    this.route.params.subscribe(params => {
      const typeId = params['typeId'];
      if (typeId) {
        this.loadPlans(typeId);
      }
    });
  }

  loadPlans(typeId: string) {
    this.insuranceService.getPlansByType(typeId).subscribe({
      next: (res) => this.plans.set(res),
      error: (err) => console.error('Error fetching plans', err)
    });
  }

  onGenerateQuote(plan: PlanDto) {
    // 1. Check Authentication
    if (!this.authService.isAuthenticated()) {
      localStorage.setItem('returnUrl', this.router.url);
      this.router.navigate(['/login']);
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

  getBenefits(description: string): string[] {
    if (!description) return ['Comprehensive coverage', '24/7 Support', 'Fast claims processing'];
    // Split by period or semicolon if exists, otherwise return as single list item
    const splits = description.split(/[.;\n]/).map(s => s.trim()).filter(s => s.length > 0);
    return splits.length > 0 ? splits : [description];
  }
}






