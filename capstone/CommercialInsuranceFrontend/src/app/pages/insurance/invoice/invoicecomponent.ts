// Renders a printable invoice for a completed policy payment, including plan details, amount, and transaction reference.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PaymentService, InvoiceDto } from '../../../services/paymentservice';

interface LocalInvoiceDto extends InvoiceDto {
// State and data property: id
  id: string;
// State and data property: policyId
  policyId: string;
}

@Component({
  selector: 'app-invoice',
  standalone: true,
  imports: [CommonModule, RouterLink],
  providers: [DatePipe, CurrencyPipe, DecimalPipe],
  templateUrl: './invoicecomponent.html',
  styleUrl: './invoicecomponent.css'
})
export class InvoiceComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private paymentService = inject(PaymentService);

// State and data property: invoice
  invoice = signal<LocalInvoiceDto | null>(null);
// State and data property: loading
  loading = signal(false);
// Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    const id = this.route.snapshot.params['id']; // This is policyId according to routes
    if (id) {
      this.loading.set(true);
      // Fetch payments for policy and get the invoice for the latest one
      this.paymentService.getPaymentsByPolicy(id).subscribe({
        next: (payments) => {
          if (payments && payments.length > 0) {
            const latestPayment = payments[payments.length - 1];
            this.paymentService.getInvoice(latestPayment.id).subscribe({
              next: (inv) => {
                this.invoice.set({ ...inv, id: latestPayment.id, policyId: id });
                this.loading.set(false);
              },
              error: () => this.loading.set(false)
            });
          } else {
            this.loading.set(false);
          }
        },
        error: () => this.loading.set(false)
      });
    }
  }

// Executes core logic for printInvoice
  printInvoice() {
    window.print();
  }
}