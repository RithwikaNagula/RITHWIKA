import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Contact } from './contactcomponent';

describe('Contact', () => {
    let component: Contact;
    let fixture: ComponentFixture<Contact>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Contact],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{path: '**', component: class {}}])]
        }).compileComponents();

        fixture = TestBed.createComponent(Contact);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});



