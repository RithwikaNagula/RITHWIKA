// Confirmation view displayed after a customer successfully generates a new insurance policy quote.
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

  policy = signal<PolicyDto | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.policyService.getPolicy(id).subscribe(res => this.policy.set(res));
    }
  }
}






