// Main unified dashboard for administrators, providing high-level metrics, system overviews, and navigation to specific management modules.
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard',
  styleUrl: './admin-dashboardcomponent.css',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-dashboardcomponent.html'
})
export class AdminDashboardComponent { }





