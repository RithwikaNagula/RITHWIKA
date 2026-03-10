/**
 * Test Suite for admin-userscomponent
 * Layer: Angular Component Navigation & DOM
 * Purpose: Validates component instantiation, automated DOM compilation, and verifies correct isolated dependency ingestion.
 */
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AdminUsersComponent } from './admin-userscomponent';
import { AdminService } from '../../../services/adminservice';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('AdminUsersComponent', () => {
  let component: AdminUsersComponent;
  let fixture: ComponentFixture<AdminUsersComponent>;
  let mockAdminService: jasmine.SpyObj<AdminService>;

  beforeEach(async () => {
    mockAdminService = jasmine.createSpyObj('AdminService', [
      'getAgents',
      'getClaimsOfficers',
      'createAgent',
      'createClaimsOfficer',
      'deleteUser'
    ]);

    mockAdminService.getAgents.and.returnValue(of([]));
    mockAdminService.getClaimsOfficers.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [AdminUsersComponent, FormsModule],
      providers: [
        { provide: AdminService, useValue: mockAdminService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminUsersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load agents and officers on init', () => {
    // Arrange
    const dummyAgents = [{ id: 'a1', fullName: 'Agent 1' }] as any;
    const dummyOfficers = [{ id: 'o1', fullName: 'Officer 1' }] as any;
    mockAdminService.getAgents.and.returnValue(of(dummyAgents));
    mockAdminService.getClaimsOfficers.and.returnValue(of(dummyOfficers));

    // Act
    component.ngOnInit();

    // Assert
    expect(mockAdminService.getAgents).toHaveBeenCalled();
    expect(mockAdminService.getClaimsOfficers).toHaveBeenCalled();
    expect(component.agents()).toEqual(dummyAgents);
    expect(component.officers()).toEqual(dummyOfficers);
  });

  it('should create an agent and reload data', () => {
    // Arrange
    component.newAgent = { fullName: 'New Agent', email: 'a@t.com', password: 'pw' };
    mockAdminService.createAgent.and.returnValue(of({} as any));
    spyOn(component, 'loadData');

    // Act
    component.createAgent();

    // Assert
    expect(mockAdminService.createAgent).toHaveBeenCalledWith({ fullName: 'New Agent', email: 'a@t.com', password: 'pw' });
    expect(component.newAgent.fullName).toBe('');
    expect(component.loadData).toHaveBeenCalled();
  });

  it('should delete a user after confirmation', () => {
    // Arrange
    spyOn(window, 'confirm').and.returnValue(true);
    mockAdminService.deleteUser.and.returnValue(of({}));
    spyOn(component, 'loadData');

    // Act
    component.deleteUser('u1', 'User 1');

    // Assert
    expect(window.confirm).toHaveBeenCalled();
    expect(mockAdminService.deleteUser).toHaveBeenCalledWith('u1');
    expect(component.loadData).toHaveBeenCalled();
  });

  it('should show alert when deletion fails', () => {
    // Arrange
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(window, 'alert');
    mockAdminService.deleteUser.and.returnValue(throwError(() => ({ error: { message: 'In use' } })));

    // Act
    component.deleteUser('u1', 'User 1');

    // Assert
    expect(window.alert).toHaveBeenCalledWith('Failed to delete user: In use');
  });
});



