// Generic error display page shown when navigation reaches an unrecognized route or an API error is caught.
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

// State and data property: statusCode
    statusCode: string | null = null;
// State and data property: errorMessage
    errorMessage: string | null = null;
// State and data property: referenceId
    referenceId: string = Math.random().toString(36).substring(2, 10).toUpperCase();

// Executes core logic for errorTitle
    get errorTitle(): string {
        switch (this.statusCode) {
            case '404': return 'Oops! Nothing Was Found';
            case '403': return 'Access Denied';
            case '401': return 'Authentication Required';
            case '500': return 'Internal Server Error';
            case '503': return 'Service Unavailable';
            case '0': return 'Connection Failed';
// State and data property: default
            default: return 'Something Went Wrong';
        }
    }

// Event listener hook triggered by errorDescription
    get errorDescription(): string {
        if (this.errorMessage) return this.errorMessage;
        switch (this.statusCode) {
            case '404': return 'The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.';
            case '403': return 'You do not have permission to access this resource. Please contact your administrator if you believe this is an error.';
            case '401': return 'Please sign in to access this page. Your session may have expired.';
            case '500': return 'Our servers encountered an unexpected condition. Our team has been notified and is working on a fix.';
            case '503': return 'The service is temporarily offline for maintenance. Please try again in a few minutes.';
            case '0': return 'Unable to reach our servers. Please check your internet connection and try again.';
// State and data property: default
            default: return 'An unexpected error occurred while processing your request. Please try again later.';
        }
    }
// Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            if (params['status']) this.statusCode = params['status'];
            if (params['message']) this.errorMessage = params['message'];
        });
        this.route.data.subscribe(data => {
            if (data['status'] && !this.statusCode) this.statusCode = data['status'];
        });
    }

// Event listener hook triggered by onRetry
    onRetry() {
        window.history.back();
    }
}
