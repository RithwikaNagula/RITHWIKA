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
  inquiries = signal<SupportInquiry[]>([]);
  isLoading = signal(true);

  ngOnInit() {
    this.loadInquiries();
  }

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

  markResolved(id: string) {
    this.supportService.resolveInquiry(id).subscribe({
      next: () => {
        this.inquiries.update(list => list.map(i => i.id === id ? { ...i, isResolved: true } : i));
      }
    });
  }
}
