/**
 * Test Suite for payment-schedulecomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
// Test suite for checking the rendering and logic of PaymentScheduleComponent
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentScheduleComponent } from './payment-schedulecomponent';

describe('PaymentScheduleComponent', () => {
  let component: PaymentScheduleComponent;
  let fixture: ComponentFixture<PaymentScheduleComponent>;

  beforeEach(async () => {
    // Configure the testing module for the component
    await TestBed.configureTestingModule({
      imports: [PaymentScheduleComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: '**', component: PaymentScheduleComponent }])]
    })
      .compileComponents();

    // Initialize component and trigger change detection
    fixture = TestBed.createComponent(PaymentScheduleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should verify the component is created correctly', () => {
    // Component instance must be truthy
    expect(component).toBeTruthy();
  });
});


