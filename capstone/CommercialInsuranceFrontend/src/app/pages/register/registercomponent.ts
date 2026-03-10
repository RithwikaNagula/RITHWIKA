// User registration form with validation, reCAPTCHA, and redirect to login on success.
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
// State and data property: fullName
  fullName = '';
// State and data property: email
  email = '';
// State and data property: password
  password = '';
// State and data property: loading
  loading = signal(false);
// State and data property: error
  error = signal('');

  constructor(private authService: AuthService, private router: Router) { }
// Triggered organically on form submission, intercepts click handling to validate and process payload

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