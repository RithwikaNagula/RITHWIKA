/**
 * Test Suite for registercomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { RegisterComponent } from './registercomponent';
import { AuthService } from '../../services/authservice';
import { Router, provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['register']);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, FormsModule],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
    expect(component.fullName).toBe('');
    expect(component.email).toBe('');
    expect(component.password).toBe('');
  });

  it('should call register and navigate to login on success', fakeAsync(() => {
    // Arrange
    component.fullName = 'New User';
    component.email = 'new@test.com';
    component.password = 'password123';
    mockAuthService.register.and.returnValue(of({}));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(mockAuthService.register).toHaveBeenCalledWith({
      fullName: 'New User',
      email: 'new@test.com',
      password: 'password123',
      role: 'Customer'
    });
    expect(component.loading()).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { registered: true } });
  }));

  it('should set error message when registration fails', fakeAsync(() => {
    // Arrange
    component.fullName = 'Error User';
    component.email = 'error@test.com';
    component.password = 'password123';
    mockAuthService.register.and.returnValue(throwError(() => ({ error: { message: 'Email already exists' } })));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(component.loading()).toBeFalse();
    expect(component.error()).toBe('Email already exists');
    expect(router.navigate).not.toHaveBeenCalled();
  }));

  it('should use default error message when api error message is missing', fakeAsync(() => {
    // Arrange
    mockAuthService.register.and.returnValue(throwError(() => ({})));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(component.error()).toBe('Registration failed');
  }));
});



