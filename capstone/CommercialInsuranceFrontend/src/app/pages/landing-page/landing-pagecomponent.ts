// Public home page: hero section, insurance type listing, features overview, and call-to-action links for registration and plan browsing.
import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InsuranceService, InsuranceTypeDto } from '../../services/insuranceservice';
import { AuthService } from '../../services/authservice';

@Component({
    selector: 'app-landing-page',
    standalone: true,
    imports: [RouterLink, CommonModule],
    templateUrl: './landing-pagecomponent.html',
    styleUrl: './landing-pagecomponent.css'
})
export class LandingPageComponent implements OnInit {
    private insuranceService = inject(InsuranceService);
    private router = inject(Router);
    auth = inject(AuthService);

    // State and data property: insuranceTypes
    insuranceTypes = signal<InsuranceTypeDto[]>([]);
    // Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        this.insuranceService.getAllInsuranceTypes().subscribe({
            next: (types) => {
                this.insuranceTypes.set(types);
                // Initialize observer after elements are rendered
                setTimeout(() => this.initScrollObserver(), 100);
            },
            error: (err) => console.error('Failed to load insurance types', err)
        });
    }

    private initScrollObserver() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                }
            });
        }, { threshold: 0.1 });

        document.querySelectorAll('.reveal-on-scroll').forEach(el => observer.observe(el));
    }

    // Executes core logic for selectType
    selectType(id: string) {
        this.router.navigate(['/insurance', id, 'plans']);
    }

    getTypeImageUrl(typeName: string): string {
        const name = (typeName || '').toLowerCase().trim();
        if (name.includes('auto') || name.includes('vehicle')) return '/assets/images/insurance-types/auto-insurance.png';
        if (name.includes('interruption') || name.includes('business income')) return '/assets/images/insurance-types/business-interruption.png';
        if (name.includes('liability')) return '/assets/images/insurance-types/general-liability.png';
        if (name.includes('workers') || name.includes('compensation')) return '/assets/images/insurance-types/workers-compensation.png';

        // Fallback Premium Image
        return 'https://images.unsplash.com/photo-1450101499163-c8848c66ca85?auto=format&fit=crop&q=80&w=800';
    }
}



