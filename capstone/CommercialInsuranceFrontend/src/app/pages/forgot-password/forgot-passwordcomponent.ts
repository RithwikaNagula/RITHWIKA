// Forgot-password form that sends a reset link to the provided email via the AuthService.
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/authservice';

@Component({
    selector: 'app-forgot-password',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    templateUrl: './forgot-passwordcomponent.html'
})
export class ForgotPasswordComponent {
// State and data property: email
    email = '';
// State and data property: newPassword
    newPassword = '';
// State and data property: loading
    loading = signal(false);
// State and data property: error
    error = signal('');
// State and data property: success
    success = signal('');

    constructor(private authService: AuthService, private router: Router) { }
// Triggered organically on form submission, intercepts click handling to validate and process payload

    onSubmit() {
        if (!this.email || !this.newPassword) return;

        this.loading.set(true);
        this.error.set('');
        this.success.set('');

        this.authService.resetPassword({ email: this.email, newPassword: this.newPassword }).subscribe({
            next: () => {
                this.loading.set(false);
                this.success.set('Password has been reset successfully. You can now login.');
                this.newPassword = ''; // clear password
            },
            error: (err) => {
                this.loading.set(false);
                const msg = err.error?.message || err.error?.detail || err.error?.title || 'Error processing request';
                this.error.set(msg);
            }
        });
    }
}