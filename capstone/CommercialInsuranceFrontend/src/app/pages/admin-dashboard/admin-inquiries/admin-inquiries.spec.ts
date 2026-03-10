/**
 * Test Suite for admin-inquiries
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminInquiriesComponent } from './admin-inquiries';
import { SupportService, SupportInquiry } from '../../../services/supportservice';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { By } from '@angular/platform-browser';

describe('AdminInquiriesComponent', () => {
    let component: AdminInquiriesComponent;
    let fixture: ComponentFixture<AdminInquiriesComponent>;
    let supportServiceSpy: jasmine.SpyObj<SupportService>;

    const mockInquiries: SupportInquiry[] = [
        {
            id: 'inq-1',
            fullName: 'John Doe',
            email: 'john@example.com',
            message: 'Need help with policy renewal.',
            createdAt: new Date().toISOString(),
            isResolved: false
        },
        {
            id: 'inq-2',
            fullName: 'Jane Smith',
            email: 'jane@example.com',
            message: 'Query regarding adding new agents.',
            createdAt: new Date().toISOString(),
            isResolved: true
        }
    ];

    beforeEach(async () => {
        // Arrange
        const spy = jasmine.createSpyObj('SupportService', ['getAllInquiries', 'resolveInquiry']);

        await TestBed.configureTestingModule({
            imports: [AdminInquiriesComponent, HttpClientTestingModule],
            providers: [
                { provide: SupportService, useValue: spy }
            ]
        }).compileComponents();

        supportServiceSpy = TestBed.inject(SupportService) as jasmine.SpyObj<SupportService>;
    });

    describe('Component Initialization', () => {
        it('should create the component', () => {
            // Act
            supportServiceSpy.getAllInquiries.and.returnValue(of([]));
            fixture = TestBed.createComponent(AdminInquiriesComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();

            // Assert
            expect(component).toBeTruthy();
        });

        it('should load inquiries on init', () => {
            // Arrange
            supportServiceSpy.getAllInquiries.and.returnValue(of(mockInquiries));

            // Act
            fixture = TestBed.createComponent(AdminInquiriesComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();

            // Assert
            expect(supportServiceSpy.getAllInquiries).toHaveBeenCalled();
            expect(component.inquiries().length).toBe(2);
            expect(component.isLoading()).toBeFalse();
        });

        it('should handle error while loading inquiries', () => {
            // Arrange
            spyOn(console, 'error');
            supportServiceSpy.getAllInquiries.and.returnValue(throwError(() => new Error('Server error')));

            // Act
            fixture = TestBed.createComponent(AdminInquiriesComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();

            // Assert
            expect(supportServiceSpy.getAllInquiries).toHaveBeenCalled();
            expect(component.isLoading()).toBeFalse();
            expect(console.error).toHaveBeenCalledWith('Failed to load inquiries', jasmine.any(Error));
        });
    });

    describe('markResolved', () => {
        beforeEach(() => {
            // Setup component state with mock data
            supportServiceSpy.getAllInquiries.and.returnValue(of(mockInquiries));
            fixture = TestBed.createComponent(AdminInquiriesComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should call resolveInquiry and update list on success', () => {
            // Arrange
            supportServiceSpy.resolveInquiry.and.returnValue(of(void 0));

            // Pre-assert that the first item is not resolved
            expect(component.inquiries()[0].isResolved).toBeFalse();

            // Act
            component.markResolved('inq-1');

            // Assert
            expect(supportServiceSpy.resolveInquiry).toHaveBeenCalledWith('inq-1');
            expect(component.inquiries()[0].isResolved).toBeTrue();
        });
    });
});
