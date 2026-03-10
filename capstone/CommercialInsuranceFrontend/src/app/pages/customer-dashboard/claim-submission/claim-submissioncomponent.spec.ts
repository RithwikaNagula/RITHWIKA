/**
 * Test Suite for claim-submissioncomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ClaimSubmissionComponent } from './claim-submissioncomponent';
import { ClaimService } from '../../../services/claimservice';
import { PolicyService } from '../../../services/policyservice';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

describe('ClaimSubmissionComponent', () => {
  let component: ClaimSubmissionComponent;
  let fixture: ComponentFixture<ClaimSubmissionComponent>;
  let mockClaimService: jasmine.SpyObj<ClaimService>;
  let mockPolicyService: jasmine.SpyObj<PolicyService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    mockClaimService = jasmine.createSpyObj('ClaimService', ['fileClaim']);
    mockPolicyService = jasmine.createSpyObj('PolicyService', ['getPolicy']);

    // Simulate query param / route param
    mockActivatedRoute = {
      snapshot: { params: { policyId: 'P-123' } }
    };

    await TestBed.configureTestingModule({
      imports: [ClaimSubmissionComponent, FormsModule],
      providers: [
        { provide: ClaimService, useValue: mockClaimService },
        { provide: PolicyService, useValue: mockPolicyService },
        provideRouter([]),
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    mockRouter = TestBed.inject(Router) as any;
    spyOn(mockRouter, 'navigate');

    fixture = TestBed.createComponent(ClaimSubmissionComponent);
    component = fixture.componentInstance;
  });

  it('should initialize component and have proper default states', () => {
    // Assert 
    expect(component).toBeTruthy();
    expect(component.selectedFiles.length).toBe(0);
    expect(component.claimData.claimAmount).toBe(0);
  });

  it('should_LoadPolicy_AndCheckStatus_OnInit', fakeAsync(() => {
    // Arrange
    const dummyPolicy: any = { id: 'P-123', status: 'Active' };
    mockPolicyService.getPolicy.and.returnValue(of(dummyPolicy));

    // Act
    fixture.detectChanges(); // Triggers ngOnInit
    tick();

    // Assert
    expect(mockPolicyService.getPolicy).toHaveBeenCalledWith('P-123');
    expect(component.policy()).toEqual(dummyPolicy);
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  }));

  it('should_RedirectToDashboard_WhenPolicyIsNotActive', fakeAsync(() => {
    // Arrange
    const dummyPolicy: any = { id: 'P-123', status: 'Expired' };
    mockPolicyService.getPolicy.and.returnValue(of(dummyPolicy));

    // Act
    fixture.detectChanges();
    tick();

    // Assert
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
  }));

  it('should_ShowError_WhenNoDocumentsAreProvided', () => {
    // Arrange
    component.policy.set({ id: 'P-123' } as any);
    component.selectedFiles = []; // Empty

    // Act
    component.onSubmit();

    // Assert
    expect(component.submitError()).toBe('Please provide at least 1 supporting document.');
    expect(mockClaimService.fileClaim).not.toHaveBeenCalled();
  });

  it('should_AllowUpTo5Files_DuringFileSelection', () => {
    // Arrange
    const dt = new DataTransfer();
    for (let i = 0; i < 6; i++) {
      dt.items.add(new File([''], `file${i}.png`));
    }

    // Act
    component.handleFiles(dt.files);

    // Assert
    expect(component.selectedFiles.length).toBe(5); // Only 5 allowed by internal rules
  });

  it('should_ProcessFormSuccessfully_AndNavigateDash', fakeAsync(() => {
    // Arrange
    component.policy.set({ id: 'P-123' } as any);
    component.claimData = { description: 'Crash', claimAmount: 5000 };
    component.selectedFiles = [new File([''], 'img.jpg')];
    mockClaimService.fileClaim.and.returnValue(of({} as any));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(mockClaimService.fileClaim).toHaveBeenCalled(); // checks args inside implementation
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
  }));

  it('should_CatchErrorsFromBackend_AndParseProblemDetails', fakeAsync(() => {
    // Arrange
    component.policy.set({ id: 'P-123' } as any);
    component.selectedFiles = [new File([''], 'img.jpg')];
    mockClaimService.fileClaim.and.returnValue(throwError(() => ({ error: { detail: 'Duplicate claim' } })));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(component.submitError()).toBe('Duplicate claim');
    expect(component.loading()).toBeFalse();
  }));
});
