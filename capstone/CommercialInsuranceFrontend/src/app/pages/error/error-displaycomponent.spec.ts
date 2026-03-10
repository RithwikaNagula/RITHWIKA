/**
 * Test Suite for error-displaycomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
// Test suite for checking the rendering and logic of ErrorDisplayComponent
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ErrorDisplayComponent } from './error-displaycomponent';

describe('ErrorDisplayComponent', () => {
  let component: ErrorDisplayComponent;
  let fixture: ComponentFixture<ErrorDisplayComponent>;

  beforeEach(async () => {
    // Configure the testing module for the component
    await TestBed.configureTestingModule({
      imports: [ErrorDisplayComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{path: '**', component: class {}}])]
    })
    .compileComponents();
    
    // Initialize component and trigger change detection
    fixture = TestBed.createComponent(ErrorDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should verify the component is created correctly', () => {
    // Component instance must be truthy
    expect(component).toBeTruthy();
  });
});



