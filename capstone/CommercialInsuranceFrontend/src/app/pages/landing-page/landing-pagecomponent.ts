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

    insuranceTypes = signal<InsuranceTypeDto[]>([]);

    ngOnInit() {
        this.insuranceService.getAllInsuranceTypes().subscribe({
            next: (types) => this.insuranceTypes.set(types),
            error: (err) => console.error('Failed to load insurance types', err)
        });
    }

    selectType(id: string) {
        this.router.navigate(['/insurance', id, 'plans']);
    }
}




