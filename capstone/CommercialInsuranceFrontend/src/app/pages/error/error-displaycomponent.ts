// Fallback component designed to catch and display global system errors and connection failures gracefully to the user.
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
    selector: 'app-error-display',
    styleUrl: './error-displaycomponent.css',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './error-displaycomponent.html'
})
export class ErrorDisplayComponent {
    private route = inject(ActivatedRoute);

    statusCode: string | null = null;
    errorMessage: string | null = null;
    referenceId: string = Math.random().toString(36).substring(2, 10).toUpperCase();

    get errorTitle(): string {
        switch (this.statusCode) {
            case '404': return 'Oops! Nothing Was Found';
            case '403': return 'Access Denied';
            case '401': return 'Authentication Required';
            case '500': return 'Internal Server Error';
            case '503': return 'Service Unavailable';
            case '0': return 'Connection Failed';
            default: return 'Something Went Wrong';
        }
    }

    get errorDescription(): string {
        if (this.errorMessage) return this.errorMessage;
        switch (this.statusCode) {
            case '404': return 'The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.';
            case '403': return 'You do not have permission to access this resource. Please contact your administrator if you believe this is an error.';
            case '401': return 'Please sign in to access this page. Your session may have expired.';
            case '500': return 'Our servers encountered an unexpected condition. Our team has been notified and is working on a fix.';
            case '503': return 'The service is temporarily offline for maintenance. Please try again in a few minutes.';
            case '0': return 'Unable to reach our servers. Please check your internet connection and try again.';
            default: return 'An unexpected error occurred while processing your request. Please try again later.';
        }
    }

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            if (params['status']) this.statusCode = params['status'];
            if (params['message']) this.errorMessage = params['message'];
        });
        this.route.data.subscribe(data => {
            if (data['status'] && !this.statusCode) this.statusCode = data['status'];
        });
    }

    onRetry() {
        window.history.back();
    }
}

