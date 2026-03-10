/**
 * Test Suite for contactcomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Contact } from './contactcomponent';
import { SupportService } from '../../services/supportservice';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { provideRouter } from '@angular/router';

describe('Contact', () => {
    let component: Contact;
    let fixture: ComponentFixture<Contact>;
    let mockSupportService: jasmine.SpyObj<SupportService>;

    beforeEach(async () => {
        mockSupportService = jasmine.createSpyObj('SupportService', ['submitInquiry']);

        await TestBed.configureTestingModule({
            imports: [Contact, ReactiveFormsModule],
            providers: [
                provideRouter([]),
                { provide: SupportService, useValue: mockSupportService }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Contact);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have an invalid form when empty', () => {
        expect(component.contactForm.invalid).toBeTrue();
    });

    it('should call submitInquiry and show success message on success', fakeAsync(() => {
        // Arrange
        component.contactForm.setValue({
            fullName: 'Test User',
            email: 'test@example.com',
            message: 'This is a test message that is long enough.'
        });
        mockSupportService.submitInquiry.and.returnValue(of({} as any));

        // Act
        component.submitInquiry();
        tick();

        // Assert
        expect(mockSupportService.submitInquiry).toHaveBeenCalled();
        expect(component.showSuccess()).toBeTrue();
        expect(component.isSubmitting()).toBeFalse();
    }));
});
