import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditSystemUserModalComponent } from './add-edit-system-user-modal.component';

describe('AddEditSystemUserModalComponent', () => {
  let component: AddEditSystemUserModalComponent;
  let fixture: ComponentFixture<AddEditSystemUserModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditSystemUserModalComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddEditSystemUserModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
