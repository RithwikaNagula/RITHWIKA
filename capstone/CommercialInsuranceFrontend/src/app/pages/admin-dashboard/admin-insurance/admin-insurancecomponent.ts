// Admin component for managing insurance types and plans, including adding new offerings and configuring plan details.
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InsuranceService, InsuranceTypeDto, PlanDto } from '../../../services/insuranceservice';

@Component({
  selector: 'app-admin-insurance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-insurancecomponent.html',
  styleUrl: './admin-insurancecomponent.css'
})
export class AdminInsuranceComponent implements OnInit {
  types = signal<InsuranceTypeDto[]>([]);
  plans = signal<PlanDto[]>([]);

  newType = { typeName: '', description: '' };
  newPlan = { planName: '', description: '', minCoverageAmount: 0, maxCoverageAmount: 0, basePremium: 0, durationInMonths: 12, insuranceTypeId: null as any };

  constructor(private insuranceService: InsuranceService) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.insuranceService.getAllInsuranceTypes().subscribe(res => this.types.set(res));
    this.insuranceService.getAllPlans().subscribe(res => this.plans.set(res));
  }

  createType() {
    this.insuranceService.createInsuranceType(this.newType).subscribe(() => {
      this.newType = { typeName: '', description: '' };
      this.loadData();
    });
  }

  createPlan() {
    if (!this.newPlan.insuranceTypeId) return;
    this.insuranceService.createPlan(this.newPlan).subscribe(() => {
      this.newPlan = { planName: '', description: '', minCoverageAmount: 0, maxCoverageAmount: 0, basePremium: 0, durationInMonths: 12, insuranceTypeId: null as any };
      this.loadData();
    });
  }

  deleteType(id: string, name: string) {
    if (confirm(`Are you sure you want to permanently delete the Insurance Type: ${name}?`)) {
      this.insuranceService.deleteInsuranceType(id).subscribe({
        next: () => this.loadData(),
        error: (err) => alert(err.error?.message || 'Failed to delete insurance type')
      });
    }
  }

  deletePlan(id: string, name: string) {
    if (confirm(`Are you sure you want to permanently delete the Plan: ${name}?`)) {
      this.insuranceService.deletePlan(id).subscribe({
        next: () => this.loadData(),
        error: (err) => alert(err.error?.message || 'Failed to delete plan')
      });
    }
  }
}






