import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CustomerDashboardComponent } from './customer-dashboardcomponent';
import { PolicyService } from '../../services/policyservice';
import { AuthService } from '../../services/authservice';
import { ClaimService } from '../../services/claimservice';
import { of, throwError } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

describe('CustomerDashboardComponent', () => {
  let component: CustomerDashboardComponent;
  let fixture: ComponentFixture<CustomerDashboardComponent>;
  let mockPolicyService: jasmine.SpyObj<PolicyService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockClaimService: jasmine.SpyObj<ClaimService>;

  beforeEach(async () => {
    // Mock the external services
    mockPolicyService = jasmine.createSpyObj('PolicyService', ['getMyPolicies', 'toggleAutoRenew', 'renewPolicy']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['currentUser']);
    mockClaimService = jasmine.createSpyObj('ClaimService', ['getClaimsByPolicy']);

    mockAuthService.currentUser.and.returnValue({ id: '1' } as any);

    await TestBed.configureTestingModule({
      imports: [CustomerDashboardComponent],
      providers: [
        { provide: PolicyService, useValue: mockPolicyService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: ClaimService, useValue: mockClaimService },
        { provide: ActivatedRoute, useValue: { snapshot: { params: {} } } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerDashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create the component and have an initial empty state', () => {
    // Assert 
    expect(component).toBeTruthy();
    expect(component.policies().length).toBe(0);
  });

  it('should_LoadPoliciesAndClaims_OnInitialization', fakeAsync(() => {
    // Arrange: Create stub policies and claims
    const dummyPolicies: any[] = [{ id: '101', status: 'Active' }, { id: '102', status: 'Expired' }];
    const dummyClaims: any[] = [{ id: 'C1', description: 'Accident' }];
    mockPolicyService.getMyPolicies.and.returnValue(of(dummyPolicies));
    mockClaimService.getClaimsByPolicy.and.returnValue(of(dummyClaims));

    // Act
    fixture.detectChanges(); // Triggers ngOnInit -> loadPolicies()
    tick(); // resolve observables

    // Assert
    expect(component.policies().length).toBe(2);
    expect(mockClaimService.getClaimsByPolicy).toHaveBeenCalledTimes(2); // once for each policy
    expect(component.claims().length).toBeGreaterThan(0);
    expect(component.activePolicies().length).toBe(1);
    expect(component.expiredPolicies().length).toBe(1);
  }));

  it('should_ToggleAutoRenew_AndRefreshPolicy', fakeAsync(() => {
    // Arrange
    const initialPolicy: any = { id: '10', autoRenew: false };
    const updatedPolicy: any = { id: '10', autoRenew: true };
    component.policies.set([initialPolicy]);
    mockPolicyService.toggleAutoRenew.and.returnValue(of(updatedPolicy));

    // Act
    component.toggleAutoRenew('10');
    tick();

    // Assert
    expect(mockPolicyService.toggleAutoRenew).toHaveBeenCalledWith('10');
    expect(component.policies()[0].autoRenew).toBe(true);
  }));

  it('should_TriggerRenewal_WhenConfirmedAndSuccessful', fakeAsync(() => {
    // Arrange
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(window, 'alert').and.callFake(() => { });
    mockPolicyService.renewPolicy.and.returnValue(of({} as any));
    mockPolicyService.getMyPolicies.and.returnValue(of([])); // To simulate reload

    // Act
    component.renewPolicy('XYZ', 'POL-123');
    tick();

    // Assert
    expect(window.confirm).toHaveBeenCalled();
    expect(mockPolicyService.renewPolicy).toHaveBeenCalledWith('XYZ');
    expect(window.alert).toHaveBeenCalledWith('Policy renewed successfully! The new policy is pending payment.');
    expect(mockPolicyService.getMyPolicies).toHaveBeenCalled();
  }));
});
