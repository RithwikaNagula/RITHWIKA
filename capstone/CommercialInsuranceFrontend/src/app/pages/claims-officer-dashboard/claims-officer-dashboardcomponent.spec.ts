/**
 * Test Suite for claims-officer-dashboardcomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
// Test suite for checking the rendering and logic of ClaimsOfficerDashboardComponent
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClaimsOfficerDashboardComponent } from './claims-officer-dashboardcomponent';

describe('ClaimsOfficerDashboardComponent', () => {
  let component: ClaimsOfficerDashboardComponent;
  let fixture: ComponentFixture<ClaimsOfficerDashboardComponent>;

  beforeEach(async () => {
    // Configure the testing module for the component
    await TestBed.configureTestingModule({
      imports: [ClaimsOfficerDashboardComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{path: '**', component: class {}}])]
    })
    .compileComponents();
    
    // Initialize component and trigger change detection
    fixture = TestBed.createComponent(ClaimsOfficerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should verify the component is created correctly', () => {
    // Component instance must be truthy
    expect(component).toBeTruthy();
  });
});



