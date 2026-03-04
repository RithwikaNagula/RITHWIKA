import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
// Test suite for checking the rendering and logic of BusinessProfileCreateComponent
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BusinessProfileCreateComponent } from './business-profile-createcomponent';

describe('BusinessProfileCreateComponent', () => {
  let component: BusinessProfileCreateComponent;
  let fixture: ComponentFixture<BusinessProfileCreateComponent>;

  beforeEach(async () => {
    // Configure the testing module for the component
    await TestBed.configureTestingModule({
      imports: [BusinessProfileCreateComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{path: '**', component: class {}}])]
    })
    .compileComponents();
    
    // Initialize component and trigger change detection
    fixture = TestBed.createComponent(BusinessProfileCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should verify the component is created correctly', () => {
    // Component instance must be truthy
    expect(component).toBeTruthy();
  });
});



