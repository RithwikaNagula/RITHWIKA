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
    email = '';
    newPassword = '';
    loading = signal(false);
    error = signal('');
    success = signal('');

    constructor(
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            this.email = params['email'] || '';
        });
    }

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
