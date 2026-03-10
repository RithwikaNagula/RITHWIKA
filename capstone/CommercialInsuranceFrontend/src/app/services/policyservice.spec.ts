/**
 * Test Suite for policyservice
 * Layer: Angular Injectable Service
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PolicyService } from './policyservice';

describe('PolicyService', () => {
    let service: PolicyService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [PolicyService]
        });
        service = TestBed.inject(PolicyService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get my policies', () => {
        const dummyPolicies = [{ id: '1', policyNumber: 'POL-1' }] as any;

        service.getMyPolicies().subscribe(policies => {
            expect(policies.length).toBe(1);
            expect(policies).toEqual(dummyPolicies);
        });

        const req = httpMock.expectOne('http://localhost:5202/api/Policy/my-policies');
        expect(req.request.method).toBe('GET');
        req.flush(dummyPolicies);
    });

    it('should calculate premium', () => {
        const dummyCalc = { finalPremium: 1000 } as any;

        service.calculatePremium('P1', 50000, 'B1', 'Monthly').subscribe(res => {
            expect(res.finalPremium).toBe(1000);
        });

        const req = httpMock.expectOne('http://localhost:5202/api/Policy/calculate-premium');
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({
            planId: 'P1',
            selectedCoverageAmount: 50000,
            businessProfileId: 'B1',
            paymentFrequency: 'Monthly'
        });
        req.flush(dummyCalc);
    });

    it('should toggle auto renew', () => {
        const dummyPolicy = { id: '1', autoRenew: true } as any;

        service.toggleAutoRenew('1').subscribe(res => {
            expect(res.autoRenew).toBe(true);
        });

        const req = httpMock.expectOne('http://localhost:5202/api/Policy/1/toggle-autorenew');
        expect(req.request.method).toBe('POST');
        req.flush(dummyPolicy);
    });
});
