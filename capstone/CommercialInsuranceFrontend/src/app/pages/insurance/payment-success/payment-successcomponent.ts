// Confirmation view displayed after a successful premium payment transaction is completed.
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

    invoice = signal<InvoiceDto | null>(null);
    loadingInvoice = signal(true);
    policyId = '';

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

    printInvoice() {
        window.print();
    }
}


