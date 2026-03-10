// Confirmation page shown after a quote is successfully submitted, with a summary and next-step instructions.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PolicyService, PolicyDto } from '../../../services/policyservice';

@Component({
  selector: 'app-quote-success',
  styleUrl: './quote-successcomponent.css',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './quote-successcomponent.html'
})
export class QuoteSuccessComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private policyService = inject(PolicyService);

// State and data property: policy
  policy = signal<PolicyDto | null>(null);
// Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.policyService.getPolicy(id).subscribe(res => this.policy.set(res));
    }
  }
}





