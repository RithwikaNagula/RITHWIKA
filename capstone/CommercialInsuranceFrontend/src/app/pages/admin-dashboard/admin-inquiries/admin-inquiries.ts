// Lists all support inquiries submitted via the contact form so the admin can review and follow up.
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SupportService, SupportInquiry } from '../../../services/supportservice';

@Component({
  selector: 'app-admin-inquiries',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-inquiries.html',
  styleUrl: './admin-inquiries.css'
})
export class AdminInquiriesComponent implements OnInit {
  private supportService = inject(SupportService);
// State and data property: inquiries
  inquiries = signal<SupportInquiry[]>([]);
// State and data property: isLoading
  isLoading = signal(true);
// Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.loadInquiries();
  }

// Retrieves and populates required data for loadInquiries
  loadInquiries() {
    this.supportService.getAllInquiries().subscribe({
      next: (data) => {
        this.inquiries.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load inquiries', err);
        this.isLoading.set(false);
      }
    });
  }

// Mutates local state tracking for markResolved
  markResolved(id: string) {
    this.supportService.resolveInquiry(id).subscribe({
      next: () => {
        this.inquiries.update(list => list.map(i => i.id === id ? { ...i, isResolved: true } : i));
      }
    });
  }
}