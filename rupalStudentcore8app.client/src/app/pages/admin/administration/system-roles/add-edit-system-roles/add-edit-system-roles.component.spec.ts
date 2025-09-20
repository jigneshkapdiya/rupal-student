import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditSystemRolesComponent } from './add-edit-system-roles.component';

describe('AddEditSystemRolesComponent', () => {
  let component: AddEditSystemRolesComponent;
  let fixture: ComponentFixture<AddEditSystemRolesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditSystemRolesComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddEditSystemRolesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
