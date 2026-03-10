// Admin user management: deploy new agents/claims officers, view the active personnel directory, and deactivate accounts.
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, RegisterDto } from '../../../services/adminservice';
import { UserDto } from '../../../services/authservice';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-userscomponent.html',
  styleUrl: './admin-userscomponent.css'
})
export class AdminUsersComponent implements OnInit {
// State and data property: agents
  agents = signal<UserDto[]>([]);
// State and data property: officers
  officers = signal<UserDto[]>([]);
// State and data property: currentTab
  currentTab: 'Agents' | 'Officers' = 'Agents';

  newAgent: RegisterDto = { fullName: '', email: '', password: '' };
  newOfficer: RegisterDto = { fullName: '', email: '', password: '' };

  constructor(private adminService: AdminService) { }
// Lifecycle hook: Initialization phase where initial data is loaded from services

  ngOnInit() {
    this.loadData();
  }
// Centralized function responsible for fetching server state and populating local component signals

  loadData() {
    this.adminService.getAgents().subscribe(res => this.agents.set(res));
    this.adminService.getClaimsOfficers().subscribe(res => this.officers.set(res));
  }

// Processes form submission and persists changes for createAgent
  createAgent() {
    this.adminService.createAgent(this.newAgent).subscribe(() => {
      this.newAgent = { fullName: '', email: '', password: '' };
      this.loadData();
      this.currentTab = 'Agents';
    });
  }

// Processes form submission and persists changes for createOfficer
  createOfficer() {
    this.adminService.createClaimsOfficer(this.newOfficer).subscribe(() => {
      this.newOfficer = { fullName: '', email: '', password: '' };
      this.loadData();
      this.currentTab = 'Officers';
    });
  }

// Handles secure deletion or clearance sequence for deleteUser
  deleteUser(id: string, name: string) {
    if (confirm(`Are you sure you want to permanently delete the account for ${name}?`)) {
      this.adminService.deleteUser(id).subscribe({
        next: () => {
          this.loadData();
        },
        error: (err) => {
          alert('Failed to delete user: ' + (err.error?.message || err.message));
        }
      });
    }
  }
}





