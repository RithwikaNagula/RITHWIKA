// Interface for customers to input necessary business details dynamically and generate an estimated insurance quote.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { InsuranceService, PlanDto } from '../../../services/insuranceservice';
import { PolicyService, CreatePolicyDto, PremiumCalculationDto } from '../../../services/policyservice';
import { AuthService } from '../../../services/authservice';
import { BusinessProfileService, BusinessProfileDto } from '../../../services/businessprofileservice';

@Component({
  selector: 'app-quote-generate',
  styleUrl: './quote-generatecomponent.css',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quote-generatecomponent.html'
})
export class QuoteGenerateComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private insuranceService = inject(InsuranceService);
  private profileService = inject(BusinessProfileService);
  private policyService = inject(PolicyService);
  private authService = inject(AuthService);

  plan = signal<PlanDto | null>(null);
  profiles = signal<BusinessProfileDto[]>([]);
  profile = signal<BusinessProfileDto | null>(null);
  calculation = signal<PremiumCalculationDto | null>(null);
  calcError = signal<string | null>(null);
  loading = signal(false);
  currentFrequency = signal<'Monthly' | 'Yearly'>('Monthly');
  currentCoverage = signal<number>(0);

  selectedFiles: File[] = [];

  ngOnInit() {
    const planId = this.route.snapshot.queryParams['planId'];
    if (!planId) {
      this.router.navigate(['/']);
      return;
    }

    this.loadInitialData(planId);
  }

  loadInitialData(planId: string) {
    this.insuranceService.getAllPlans().subscribe(plans => {
      const p = plans.find(x => x.id === planId);
      if (p) {
        this.plan.set(p);
        this.loadProfiles();
      } else {
        this.router.navigate(['/']);
      }
    });
  }

  loadProfiles() {
    this.profileService.getProfiles().subscribe({
      next: (profs) => {
        this.profiles.set(profs);
        if (profs.length > 0) {
          this.selectProfile(profs[0]);
        }
      },
      error: (err) => console.error('Failed to load profiles', err)
    });
  }

  selectProfile(prof: BusinessProfileDto) {
    this.profile.set(prof);
    if (this.currentCoverage() === 0 && this.plan()) {
      this.currentCoverage.set(this.plan()!.minCoverageAmount || 10000);
    }
    this.calculateRate();
  }

  // Coverage is fixed to plan's min coverage amount (no customer override)

  setFrequency(freq: 'Monthly' | 'Yearly') {
    this.currentFrequency.set(freq);
    this.calculateRate();
  }

  calculateRate() {
    const p = this.plan();
    const prof = this.profile();
    if (!p || !prof) return;

    this.calculation.set(null);
    this.calcError.set(null);

    this.policyService.calculatePremium(p.id, this.currentCoverage(), prof.id, this.currentFrequency()).subscribe({
      next: (calc) => this.calculation.set(calc),
      error: (err) => {
        console.error('Calculation error', err);
        this.calcError.set(err.error?.message || 'Risk assessment failed. This profile might need updates.');
      }
    });
  }

  registerNewBusiness() {
    localStorage.setItem('returnUrl', this.router.url);
    this.router.navigate(['/business-profile/create']);
  }

  onFilesSelected(event: any) {
    const files = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        this.selectedFiles.push(files[i]);
      }
    }
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  confirmQuote() {
    const p = this.plan();
    const prof = this.profile();
    const calc = this.calculation();
    if (!p || !prof || !calc || this.selectedFiles.length === 0) return;

    this.loading.set(true);

    const formData = new FormData();
    formData.append('quoteId', calc.quoteId);
    formData.append('planId', p.id);
    formData.append('customerId', prof.userId);
    formData.append('businessProfileId', prof.id);
    const coverageToSend = this.currentCoverage();
    formData.append('selectedCoverageAmount', coverageToSend.toString());
    formData.append('premiumAmount', calc.finalPremium.toString());
    formData.append('paymentFrequency', this.currentFrequency());

    // Dates
    const startDate = new Date();
    const endDate = new Date();
    endDate.setMonth(startDate.getMonth() + p.durationInMonths);
    formData.append('startDate', startDate.toISOString());
    formData.append('endDate', endDate.toISOString());

    // Documents
    this.selectedFiles.forEach(file => {
      formData.append('documents', file, file.name);
    });

    this.policyService.createPolicy(formData).subscribe({
      next: (policy) => {
        this.loading.set(false);
        this.router.navigate(['/quotes/success', policy.id]);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err.error?.detail || err.error?.message || 'Request failed. Please check your documents.';
        alert(msg);
      }
    });
  }
}






