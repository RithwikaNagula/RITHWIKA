// Component dedicated to user authentication, handling login form submission and securing session tokens.
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/authservice';
import { CaptchaComponent } from '../../components/captcha/captchacomponent';

@Component({
  selector: 'app-login',
  styleUrl: './logincomponent.css',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CaptchaComponent],
  templateUrl: './logincomponent.html'
})
export class LoginComponent {
  email = '';
  password = '';
  loading = signal(false);
  error = signal('');
  captchaVerified = signal(false);

  constructor(private authService: AuthService, private router: Router) { }

  onCaptchaValid(valid: boolean): void {
    this.captchaVerified.set(valid);
  }

  onSubmit() {
    if (!this.email || !this.password) return;

    if (!this.captchaVerified()) {
      this.error.set('Please complete the security verification before signing in.');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.user.role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (res.user.role === 'Agent') {
          this.router.navigate(['/agent/dashboard']);
        } else if (res.user.role === 'Customer') {
          this.router.navigate(['/dashboard']);
        } else if (res.user.role === 'ClaimsOfficer') {
          this.router.navigate(['/claims-officer/dashboard']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Invalid credentials');
      }
    });
  }
}