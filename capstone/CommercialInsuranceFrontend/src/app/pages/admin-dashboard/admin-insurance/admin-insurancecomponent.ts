// Admin insurance type management: list all categories, add new types, update descriptions, and delete unused types.
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
  // State and data property: types
  types = signal<InsuranceTypeDto[]>([]);
  // State and data property: plans
  plans = signal<PlanDto[]>([]);

  newType = { typeName: '', description: '' };
  newPlan = { planName: '', description: '', minCoverageAmount: 0, maxCoverageAmount: 0, basePremium: 0, durationInMonths: 12, insuranceTypeId: null as any };
  selectedFile: File | null = null;

  constructor(private insuranceService: InsuranceService) { }
  // Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.loadData();
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }
  // Centralized function responsible for fetching server state and populating local component signals

  loadData() {
    this.insuranceService.getAllInsuranceTypes().subscribe(res => this.types.set(res));
    this.insuranceService.getAllPlans().subscribe(res => this.plans.set(res));
  }

  // Processes form submission and persists changes for createType
  createType() {
    this.insuranceService.createInsuranceType(this.newType).subscribe(() => {
      this.newType = { typeName: '', description: '' };
      this.loadData();
    });
  }

  // Processes form submission and persists changes for createPlan
  createPlan() {
    if (!this.newPlan.insuranceTypeId) return;

    const formData = new FormData();
    formData.append('planName', this.newPlan.planName);
    formData.append('description', this.newPlan.description);
    formData.append('minCoverageAmount', this.newPlan.minCoverageAmount.toString());
    formData.append('maxCoverageAmount', this.newPlan.maxCoverageAmount.toString());
    formData.append('basePremium', this.newPlan.basePremium.toString());
    formData.append('durationInMonths', this.newPlan.durationInMonths.toString());
    formData.append('insuranceTypeId', this.newPlan.insuranceTypeId);

    if (this.selectedFile) {
      formData.append('image', this.selectedFile);
    }

    this.insuranceService.createPlan(formData).subscribe(() => {
      this.newPlan = { planName: '', description: '', minCoverageAmount: 0, maxCoverageAmount: 0, basePremium: 0, durationInMonths: 12, insuranceTypeId: null as any };
      this.selectedFile = null;
      this.loadData();
    });
  }

  // Handles secure deletion or clearance sequence for deleteType
  deleteType(id: string, name: string) {
    if (confirm(`Are you sure you want to permanently delete the Insurance Type: ${name}?`)) {
      this.insuranceService.deleteInsuranceType(id).subscribe({
        next: () => this.loadData(),
        error: (err) => alert(err.error?.message || 'Failed to delete insurance type')
      });
    }
  }

  // Handles secure deletion or clearance sequence for deletePlan
  deletePlan(id: string, name: string) {
    if (confirm(`Are you sure you want to permanently delete the Plan: ${name}?`)) {
      this.insuranceService.deletePlan(id).subscribe({
        next: () => this.loadData(),
        error: (err) => alert(err.error?.message || 'Failed to delete plan')
      });
    }
  }
}





