// Form for customers to create or update their business profile (company name, type, industry, address) required before purchasing a policy.
import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BusinessProfileService, CreateBusinessProfileDto } from '../../services/businessprofileservice';

@Component({
    selector: 'app-business-profile-create',
    styleUrl: './business-profile-createcomponent.css',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './business-profile-createcomponent.html'
})
export class BusinessProfileCreateComponent {
    private profileService = inject(BusinessProfileService);
    private router = inject(Router);

// State and data property: loading
    loading = signal(false);
// State and data property: errorMessage
    errorMessage = signal('');

    profile: CreateBusinessProfileDto = {
        businessName: '',
        industry: '',
        annualRevenue: 0,
        employeeCount: 1,
        city: ''
    };
// Triggered organically on form submission, intercepts click handling to validate and process payload

    onSubmit() {
        this.loading.set(true);
        this.errorMessage.set('');

        this.profileService.createProfile(this.profile).subscribe({
            next: () => {
                this.loading.set(false);
                // On success, go back to wherever they were or a dashboard
                const returnUrl = localStorage.getItem('returnUrl') || '/';
                localStorage.removeItem('returnUrl');
                this.router.navigateByUrl(returnUrl);
            },
            error: (err) => {
                this.loading.set(false);
                this.errorMessage.set(err.error?.message || 'Failed to create profile. Please check your data.');
            }
        });
    }
}







