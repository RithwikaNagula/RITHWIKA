// Confirmation page shown after a successful premium payment, with policy ID and payment receipt details.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PaymentService, InvoiceDto } from '../../../services/paymentservice';

@Component({
    selector: 'app-payment-success',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './payment-successcomponent.html',
    styleUrl: './payment-successcomponent.css'
})
export class PaymentSuccessComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private paymentService = inject(PaymentService);

// State and data property: invoice
    invoice = signal<InvoiceDto | null>(null);
// State and data property: loadingInvoice
    loadingInvoice = signal(true);
// State and data property: policyId
    policyId = '';
// Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        const paymentId = this.route.snapshot.params['paymentId'];
        if (paymentId) {
            this.paymentService.getInvoice(paymentId).subscribe({
                next: (inv) => {
                    this.invoice.set(inv);
                    this.loadingInvoice.set(false);
                    // We need policyId for schedule link – get it from the payment
                    this.paymentService.getPaymentById(paymentId).subscribe(p => {
                        this.policyId = p.policyId;
                    });
                },
                error: () => {
                    this.loadingInvoice.set(false);
                    this.router.navigate(['/dashboard']);
                }
            });
        }
    }

// Executes core logic for printInvoice
    printInvoice() {
        window.print();
    }
}

