/**
 * Test Suite for about-uscomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AboutUs } from './about-uscomponent';

describe('AboutUs', () => {
    let component: AboutUs;
    let fixture: ComponentFixture<AboutUs>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AboutUs],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{path: '**', component: class {}}])]
        }).compileComponents();

        fixture = TestBed.createComponent(AboutUs);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});



