// Dashboard component for administrators to view, manage, and edit user accounts across the system.
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
  agents = signal<UserDto[]>([]);
  officers = signal<UserDto[]>([]);
  currentTab: 'Agents' | 'Officers' = 'Agents';

  newAgent: RegisterDto = { fullName: '', email: '', password: '' };
  newOfficer: RegisterDto = { fullName: '', email: '', password: '' };

  constructor(private adminService: AdminService) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.adminService.getAgents().subscribe(res => this.agents.set(res));
    this.adminService.getClaimsOfficers().subscribe(res => this.officers.set(res));
  }

  createAgent() {
    this.adminService.createAgent(this.newAgent).subscribe(() => {
      this.newAgent = { fullName: '', email: '', password: '' };
      this.loadData();
      this.currentTab = 'Agents';
    });
  }

  createOfficer() {
    this.adminService.createClaimsOfficer(this.newOfficer).subscribe(() => {
      this.newOfficer = { fullName: '', email: '', password: '' };
      this.loadData();
      this.currentTab = 'Officers';
    });
  }

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






