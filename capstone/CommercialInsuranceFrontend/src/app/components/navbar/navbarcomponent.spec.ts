/**
 * Test Suite for navbarcomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NavbarComponent } from './navbarcomponent';
import { AuthService } from '../../services/authservice';
import { NotificationService } from '../../services/notificationservice';
import { Router, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { signal } from '@angular/core';

describe('NavbarComponent', () => {
    let component: NavbarComponent;
    let fixture: ComponentFixture<NavbarComponent>;
    let mockAuthService: jasmine.SpyObj<AuthService>;
    let mockNotificationService: jasmine.SpyObj<NotificationService>;
    let router: Router;

    beforeEach(async () => {
        mockAuthService = jasmine.createSpyObj('AuthService', [
            'logout',
            'currentUser',
            'isAdmin',
            'isAgent',
            'isCustomer',
            'isClaimsOfficer',
            'isAuthenticated'
        ]);
        mockNotificationService = jasmine.createSpyObj('NotificationService', ['load', 'markRead', 'markAllRead']);

        // Setup default mock values
        mockAuthService.isAdmin.and.returnValue(false);
        mockAuthService.isAgent.and.returnValue(false);
        mockAuthService.isCustomer.and.returnValue(false);
        mockAuthService.isClaimsOfficer.and.returnValue(false);
        mockAuthService.isAuthenticated.and.returnValue(false);
        mockAuthService.currentUser.and.returnValue(null);
        (mockNotificationService as any).notifications = signal([]);

        await TestBed.configureTestingModule({
            imports: [NavbarComponent],
            providers: [
                provideRouter([]),
                { provide: AuthService, useValue: mockAuthService },
                { provide: NotificationService, useValue: mockNotificationService }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(NavbarComponent);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        spyOn(router, 'navigate');
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call logout and navigate to home', () => {
        component.logout();
        expect(mockAuthService.logout).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should toggle notifications and load them', () => {
        component.showNotifications = false;
        component.toggleNotifications();
        expect(component.showNotifications).toBeTrue();
        expect(mockNotificationService.load).toHaveBeenCalled();

        component.toggleNotifications();
        expect(component.showNotifications).toBeFalse();
    });

    it('should mark notification as read', () => {
        const notif = { id: 'n1', isRead: false } as any;
        component.markRead(notif);
        expect(mockNotificationService.markRead).toHaveBeenCalledWith('n1');
    });

    it('should mark all as read', () => {
        component.markAllRead();
        expect(mockNotificationService.markAllRead).toHaveBeenCalled();
    });
});
