// Shows a payment schedule table (due dates, instalment amounts) for an active policy fetched by policy ID.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PaymentService, PaymentScheduleDto } from '../../../services/paymentservice';

@Component({
    selector: 'app-payment-schedule',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './payment-schedulecomponent.html',
    styleUrl: './payment-schedulecomponent.css'
})
export class PaymentScheduleComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private paymentService = inject(PaymentService);

// State and data property: schedule
    schedule = signal<PaymentScheduleDto | null>(null);
// State and data property: loading
    loading = signal(true);

    paidCount = computed(() => {
        const s = this.schedule();
        return s ? s.schedule.filter(i => i.status === 'Paid').length : 0;
    });

    progressPercent = computed(() => {
        const s = this.schedule();
        if (!s || s.totalInstallments === 0) return 0;
        return (this.paidCount() / s.totalInstallments) * 100;
    });
// Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        const policyId = this.route.snapshot.queryParams['policyId'] || this.route.snapshot.params['policyId'];
        if (policyId) {
            this.paymentService.getPaymentSchedule(policyId).subscribe({
                next: (s) => {
                    this.schedule.set(s);
                    this.loading.set(false);
                },
                error: () => {
                    this.loading.set(false);
                    this.router.navigate(['/dashboard']);
                }
            });
        } else {
            this.router.navigate(['/dashboard']);
        }
    }
}

