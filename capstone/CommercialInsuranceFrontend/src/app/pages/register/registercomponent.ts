// Component responsible for handling new user registration, capturing their details, and interacting with the authentication service.
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/authservice';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './registercomponent.html',
  styleUrl: './registercomponent.css'
})
export class RegisterComponent {
  fullName = '';
  email = '';
  password = '';
  loading = signal(false);
  error = signal('');

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit() {
    this.loading.set(true);
    this.error.set('');

    const dto = {
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      role: 'Customer'
    };

    this.authService.register(dto).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/login'], { queryParams: { registered: true } });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Registration failed');
      }
    });
  }
}
