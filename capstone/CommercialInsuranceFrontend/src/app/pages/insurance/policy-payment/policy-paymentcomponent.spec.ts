import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
// Test suite for checking the rendering and logic of PolicyPaymentComponent
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PolicyPaymentComponent } from './policy-paymentcomponent';

describe('PolicyPaymentComponent', () => {
  let component: PolicyPaymentComponent;
  let fixture: ComponentFixture<PolicyPaymentComponent>;

  beforeEach(async () => {
    // Configure the testing module for the component
    await TestBed.configureTestingModule({
      imports: [PolicyPaymentComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{path: '**', component: class {}}])]
    })
    .compileComponents();
    
    // Initialize component and trigger change detection
    fixture = TestBed.createComponent(PolicyPaymentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should verify the component is created correctly', () => {
    // Component instance must be truthy
    expect(component).toBeTruthy();
  });
});



