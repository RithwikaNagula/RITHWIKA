// Interface enabling customers to file new insurance claims, verify incident details, and securely upload supporting documents.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClaimService, CreateClaimDto } from '../../../services/claimservice';
import { PolicyService, PolicyDto } from '../../../services/policyservice';

@Component({
  selector: 'app-claim-submission',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './claim-submissioncomponent.html',
  styleUrl: './claim-submissioncomponent.css'
})
export class ClaimSubmissionComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private claimService = inject(ClaimService);
  private policyService = inject(PolicyService);

  policy = signal<PolicyDto | null>(null);
  loading = signal(false);
  submitError = signal<string | null>(null);

  claimData: CreateClaimDto = {
    description: '',
    claimAmount: 0
  };

  selectedFiles: File[] = [];
  isDragging = false;

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
    if (event.dataTransfer?.files) {
      this.handleFiles(event.dataTransfer.files);
    }
  }

  onFileSelect(event: any) {
    if (event.target.files) {
      this.handleFiles(event.target.files);
    }
  }

  handleFiles(files: FileList) {
    for (let i = 0; i < files.length; i++) {
      if (this.selectedFiles.length < 5) {
        this.selectedFiles.push(files[i]);
      }
    }
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  ngOnInit() {
    const policyId = this.route.snapshot.params['policyId'];
    if (policyId) {
      this.policyService.getPolicy(policyId).subscribe({
        next: (p) => {
          if (p.status !== 'Active') {
            this.router.navigate(['/dashboard']);
          }
          this.policy.set(p);
        },
        error: () => this.router.navigate(['/dashboard'])
      });
    }
  }

  onSubmit() {
    const p = this.policy();
    if (!p) return;

    if (this.selectedFiles.length === 0) {
      this.submitError.set('Please provide at least 1 supporting document.');
      return;
    }

    this.loading.set(true);
    this.submitError.set(null);

    const formData = new FormData();
    formData.append('description', this.claimData.description);
    formData.append('claimAmount', this.claimData.claimAmount.toString());

    this.selectedFiles.forEach(file => {
      formData.append('files', file, file.name);
    });

    this.claimService.fileClaim(p.id, formData).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        let errorMsg = 'Failed to submit claim. Please try again.';
        if (err.error) {
          if (err.error.errors) {
            // It's an ASP.NET Core ValidationProblemDetails
            const validationErrors = [];
            for (const key in err.error.errors) {
              if (err.error.errors.hasOwnProperty(key)) {
                validationErrors.push(err.error.errors[key].join(' '));
              }
            }
            errorMsg = validationErrors.join(' | ');
          } else if (err.error.detail) {
            errorMsg = err.error.detail;
          } else if (err.error.message) {
            errorMsg = err.error.message;
          } else if (err.error.title) {
            errorMsg = err.error.title;
          }
        }
        this.submitError.set(errorMsg);
      }
    });
  }
}






