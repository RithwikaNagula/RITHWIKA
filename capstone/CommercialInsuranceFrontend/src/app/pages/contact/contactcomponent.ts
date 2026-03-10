// Contact form page: captures visitor name, email, subject, and message, and submits an inquiry via SupportService.
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SupportService } from '../../services/supportservice';
import { Router } from '@angular/router';

@Component({
    selector: 'app-contact',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './contactcomponent.html',
    styleUrl: './contactcomponent.css',
})
export class Contact {
    private fb = inject(FormBuilder);
    private supportService = inject(SupportService);
    private router = inject(Router);

    // State and data property: isSubmitting
    isSubmitting = signal(false);
    // State and data property: showSuccess
    showSuccess = signal(false);

    contactForm = this.fb.group({
        fullName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        message: ['', [Validators.required, Validators.minLength(10)]]
    });

    // Processes form submission and persists changes for onSubmit
    onSubmit() {
        if (this.contactForm.invalid) {
            this.contactForm.markAllAsTouched();
            // State and data property: return
            return;
        }
        if (this.isSubmitting()) return;

        this.isSubmitting.set(true);
        const dto = {
            fullName: this.contactForm.value.fullName!,
            email: this.contactForm.value.email!,
            message: this.contactForm.value.message!
        };

        this.supportService.submitInquiry(dto).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.showSuccess.set(true);
                this.contactForm.reset();
                setTimeout(() => {
                    this.showSuccess.set(false);
                }, 5000);
            },
            error: (err) => {
                this.isSubmitting.set(false);
                console.error('Submission failed', err);
            }
        });
    }
}
