// Password reset form that reads the token from the URL query string and submits a new password through the AuthService.
import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/authservice';

@Component({
    selector: 'app-reset-password',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    templateUrl: './reset-passwordcomponent.html'
})
export class ResetPasswordComponent implements OnInit {
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

    constructor(
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
    ) { }
// Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            this.email = params['email'] || '';
        });
    }
// Triggered organically on form submission, intercepts click handling to validate and process payload

    onSubmit() {
        if (!this.email || !this.newPassword) return;

        this.loading.set(true);
        this.error.set('');

        this.authService.resetPassword({ email: this.email, newPassword: this.newPassword }).subscribe({
            next: () => {
                this.loading.set(false);
                this.success.set('Password has been reset successfully.');
            },
            error: (err) => {
                this.loading.set(false);
                const msg = err.error?.message || err.error?.detail || err.error?.title || 'Error resetting password';
                this.error.set(msg);
            }
        });
    }
}