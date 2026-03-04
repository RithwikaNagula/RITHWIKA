import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { LoginComponent } from './logincomponent';
import { AuthService } from '../../services/authservice';
import { Router, provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, FormsModule],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    spyOn(mockRouter, 'navigate');
    fixture.detectChanges();
  });

  it('should create the component', () => {
    // Assert logic
    expect(component).toBeTruthy();
    expect(component.email).toBe('');
    expect(component.password).toBe('');
  });

  it('should_NotCallLogin_WhenFormIsInvalid', () => {
    // Arrange
    component.email = '';
    component.password = '';

    // Act
    component.onSubmit();

    // Assert
    expect(mockAuthService.login).not.toHaveBeenCalled();
  });

  it('should_CallLoginAndNavigateToAdmin_WhenRoleIsAdmin', fakeAsync(() => {
    // Arrange
    component.email = 'admin@test.com';
    component.password = 'password123';
    component.captchaVerified.set(true);
    mockAuthService.login.and.returnValue(of({ token: 'abc', user: { role: 'Admin', id: '1', email: 'admin@test.com', fullName: 'Admin User', createdAt: new Date().toISOString() } }));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(component.loading()).toBeFalse();
    expect(mockAuthService.login).toHaveBeenCalledWith({ email: 'admin@test.com', password: 'password123' });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/admin']);
  }));

  it('should_CallLoginAndNavigateToCustomer_WhenRoleIsCustomer', fakeAsync(() => {
    // Arrange
    component.email = 'cust@test.com';
    component.password = 'password123';
    component.captchaVerified.set(true);
    mockAuthService.login.and.returnValue(of({ token: 'abc', user: { role: 'Customer', id: '1', email: 'cust@test.com', fullName: 'Customer User', createdAt: new Date().toISOString() } }));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
  }));

  it('should_SetError_WhenLoginFails', fakeAsync(() => {
    // Arrange
    component.email = 'wrong@test.com';
    component.password = 'wrongpass';
    component.captchaVerified.set(true);
    mockAuthService.login.and.returnValue(throwError(() => ({ error: { message: 'Invalid credentials' } })));

    // Act
    component.onSubmit();
    tick();

    // Assert
    expect(component.loading()).toBeFalse();
    expect(component.error()).toBe('Invalid credentials');
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  }));
});
