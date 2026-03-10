// Payment form displayed after a policy is approved; submits premium payment and transitions the policy to Active on success.
import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PolicyService, PolicyDto } from '../../../services/policyservice';
import { PaymentService, PremiumBreakdown } from '../../../services/paymentservice';
import { AuthService } from '../../../services/authservice';

@Component({
  selector: 'app-policy-payment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './policy-paymentcomponent.html',
  styleUrl: './policy-paymentcomponent.css'
})
export class PolicyPaymentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private policyService = inject(PolicyService);
  private paymentService = inject(PaymentService);
  private authService = inject(AuthService);

// State and data property: policy
  policy = signal<PolicyDto | null>(null);
// State and data property: loading
  loading = signal(false);
// State and data property: error
  error = signal<string | null>(null);
// State and data property: selectedFrequency
  selectedFrequency = signal<string>('');
// State and data property: selectedMode
  selectedMode = signal<string>('');

// State and data property: today
  today = new Date();
// State and data property: endDate
  endDate = new Date(new Date().setMonth(new Date().getMonth() + 12));

  frequencyOptions: { label: string; value: string; amount: number; installments: number }[] = [];

  paymentModes = [
    { label: 'UPI', value: 'UPI', icon: '📱' },
    { label: 'Net Banking', value: 'NetBanking', icon: '🏦' },
    { label: 'Credit Card', value: 'CreditCard', icon: '💳' },
    { label: 'Debit Card', value: 'DebitCard', icon: '🏧' }
  ];

  breakdown = signal<PremiumBreakdown>({ annual: 0, monthly: 0, quarterly: 0, halfYearly: 0, annually: 0 });

// State and data property: canPay
  canPay = computed(() => !!this.selectedFrequency() && !!this.selectedMode());

  selectedAmount = computed(() => {
    const f = this.selectedFrequency();
    const b = this.breakdown();
    if (f === 'Monthly') return b.monthly;
    if (f === 'Quarterly') return b.quarterly;
    if (f === 'HalfYearly') return b.halfYearly;
    if (f === 'Annually') return b.annually;
    return 0;
  });

  selectedInstallments = computed(() => {
    const f = this.selectedFrequency();
    if (f === 'Monthly') return 12;
    if (f === 'Quarterly') return 4;
    if (f === 'HalfYearly') return 2;
    if (f === 'Annually') return 1;
    return 0;
  });
// Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.policyService.getPolicy(id).subscribe({
        next: (p) => {
          this.policy.set(p);
          if (p.status !== 'Approved') {
            this.error.set(`This policy has status "${p.status}" and cannot be paid for.`);
          }
          const b = this.paymentService.calculatePremium(p.selectedCoverageAmount);
          this.breakdown.set(b);
          this.frequencyOptions = [
            { label: 'Monthly', value: 'Monthly', amount: b.monthly, installments: 12 },
            { label: 'Quarterly', value: 'Quarterly', amount: b.quarterly, installments: 4 },
            { label: 'Half-Yearly', value: 'HalfYearly', amount: b.halfYearly, installments: 2 },
            { label: 'Annually', value: 'Annually', amount: b.annually, installments: 1 }
          ];
        },
        error: () => this.router.navigate(['/'])
      });
    }
  }

// Executes core logic for selectFrequency
  selectFrequency(value: string) {
    this.selectedFrequency.set(value);
  }

// Event listener hook triggered by confirmPayment
  confirmPayment() {
    const p = this.policy();
    if (!p || !this.canPay()) return;

    this.loading.set(true);
    this.error.set(null);

    this.paymentService.initiatePayment({
      policyId: p.id,
      paymentFrequency: this.selectedFrequency(),
      paymentMode: this.selectedMode()
    }).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.router.navigate(['/payment-success', result.id]);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || err.error?.title || 'Payment failed. Please try again.');
      }
    });
  }
}

