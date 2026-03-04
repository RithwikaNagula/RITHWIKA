import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PaymentService, InvoiceDto } from '../../../services/paymentservice';

interface LocalInvoiceDto extends InvoiceDto {
  id: string;
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

  invoice = signal<LocalInvoiceDto | null>(null);
  loading = signal(false);

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

  printInvoice() {
    window.print();
  }
}
