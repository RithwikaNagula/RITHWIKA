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
    email = '';
    newPassword = '';
    loading = signal(false);
    error = signal('');
    success = signal('');

    constructor(private authService: AuthService, private router: Router) { }

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
