/**
 * Test Suite for plan-selectioncomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { PlanSelectionComponent } from './plan-selectioncomponent';
import { InsuranceService } from '../../../services/insuranceservice';
import { AuthService } from '../../../services/authservice';
import { BusinessProfileService } from '../../../services/businessprofileservice';
import { Router, ActivatedRoute, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('PlanSelectionComponent', () => {
  let component: PlanSelectionComponent;
  let fixture: ComponentFixture<PlanSelectionComponent>;
  let mockInsuranceService: jasmine.SpyObj<InsuranceService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockProfileService: jasmine.SpyObj<BusinessProfileService>;
  let router: Router;

  beforeEach(async () => {
    mockInsuranceService = jasmine.createSpyObj('InsuranceService', ['getPlansByType']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    mockProfileService = jasmine.createSpyObj('BusinessProfileService', ['getProfiles']);

    // Default mock setups
    mockInsuranceService.getPlansByType.and.returnValue(of([]));
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockProfileService.getProfiles.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [PlanSelectionComponent, FormsModule],
      providers: [
        provideRouter([]),
        { provide: InsuranceService, useValue: mockInsuranceService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: BusinessProfileService, useValue: mockProfileService },
        {
          provide: ActivatedRoute,
          useValue: { params: of({ typeId: 'T1' }), url: of([]) }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PlanSelectionComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load plans on init', () => {
    const dummyPlans = [{ id: 'P1', planName: 'Plan 1', basePremium: 100 }] as any;
    mockInsuranceService.getPlansByType.and.returnValue(of(dummyPlans));

    component.ngOnInit();

    expect(mockInsuranceService.getPlansByType).toHaveBeenCalledWith('T1');
    expect(component.plans()).toEqual(dummyPlans);
  });

  it('should filter plans by search text', () => {
    const dummyPlans = [
      { id: 'P1', planName: 'Gold Plan', basePremium: 100, description: 'Best' },
      { id: 'P2', planName: 'Silver Plan', basePremium: 50, description: 'Good' }
    ] as any;
    component.plans.set(dummyPlans);

    component.searchText.set('gold');
    fixture.detectChanges();

    expect(component.filteredPlans().length).toBe(1);
    expect(component.filteredPlans()[0].planName).toBe('Gold Plan');
  });

  it('should redirect to login if not authenticated on generate quote', () => {
    const plan = { id: 'P1' } as any;
    mockAuthService.isAuthenticated.and.returnValue(false);

    component.onGenerateQuote(plan);

    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should navigate to business profile if no profile exists', fakeAsync(() => {
    const plan = { id: 'P1' } as any;
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockProfileService.getProfiles.and.returnValue(of([]));

    component.onGenerateQuote(plan);
    tick();

    expect(router.navigate).toHaveBeenCalledWith(['/business-profile/create']);
  }));

  it('should navigate to quote generate if profile exists', fakeAsync(() => {
    const plan = { id: 'P1' } as any;
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockProfileService.getProfiles.and.returnValue(of([{ id: 'PROF1' } as any]));

    component.onGenerateQuote(plan);
    tick();

    expect(router.navigate).toHaveBeenCalledWith(['/quotes/generate'], { queryParams: { planId: 'P1' } });
  }));
});
