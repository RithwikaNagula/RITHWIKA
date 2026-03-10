// Admin dashboard shell: renders the sidebar navigation and hosts the child router outlet for all admin sub-pages.
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




