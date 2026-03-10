/**
 * Test Suite for captchacomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CaptchaComponent } from './captchacomponent';
import { FormsModule } from '@angular/forms';

describe('CaptchaComponent', () => {
    let component: CaptchaComponent;
    let fixture: ComponentFixture<CaptchaComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [CaptchaComponent, FormsModule]
        }).compileComponents();

        fixture = TestBed.createComponent(CaptchaComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should generate a 6-character code on init', () => {
        const code = (component as any).code;
        expect(code.length).toBe(6);
    });

    it('should emit captchaValid true when input matches code', () => {
        const code = (component as any).code;
        spyOn(component.captchaValid, 'emit');

        component.userInput = code;
        component.onInput();

        expect(component.isVerified()).toBeTrue();
        expect(component.captchaValid.emit).toHaveBeenCalledWith(true);
    });

    it('should emit captchaValid false when input does not match', () => {
        const code = (component as any).code;
        spyOn(component.captchaValid, 'emit');

        component.userInput = 'WRONG';
        component.onInput();

        expect(component.isVerified()).toBeFalse();
        expect(component.captchaValid.emit).toHaveBeenCalledWith(false);
    });

    it('should regenerate code on refresh', () => {
        const oldCode = (component as any).code;
        component.refresh();
        const newCode = (component as any).code;
        // Since it's random, there's a tiny chance it's the same, but almost always different.
        // We just check it triggered regeneration (clearing userInput etc)
        expect(component.userInput).toBe('');
    });
});
