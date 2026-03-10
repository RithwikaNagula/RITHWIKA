/**
 * Test Suite for authservice
 * Layer: Angular Injectable Service
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './authservice';

describe('AuthService', () => {
    let service: AuthService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AuthService]
        });
        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);
        localStorage.clear();
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should login and store token/user', () => {
        const dummyResponse = {
            token: 'test-token',
            user: { id: '1', role: 'Admin', fullName: 'Admin' } as any
        };

        service.login({ email: 'a@b.com', password: 'pw' }).subscribe(res => {
            expect(res).toEqual(dummyResponse as any);
            expect(localStorage.getItem('token')).toBe('test-token');
            expect(service.currentUser()).toEqual(dummyResponse.user);
        });

        const req = httpMock.expectOne('http://localhost:5202/api/Auth/login');
        expect(req.request.method).toBe('POST');
        req.flush(dummyResponse);
    });

    it('should logout and clear storage', () => {
        localStorage.setItem('token', 'abc');
        localStorage.setItem('user', '{}');
        service.currentUser.set({ id: '1' } as any);

        service.logout();

        expect(localStorage.getItem('token')).toBeNull();
        expect(localStorage.getItem('user')).toBeNull();
        expect(service.currentUser()).toBeNull();
    });

    it('should return isAdmin true when role is Admin', () => {
        service.currentUser.set({ role: 'Admin' } as any);
        expect(service.isAdmin()).toBeTrue();
    });
});
