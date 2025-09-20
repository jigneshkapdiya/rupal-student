import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditSystemMenuModalComponent } from './add-edit-system-menu-modal.component';

describe('AddEditSystemMenuModalComponent', () => {
  let component: AddEditSystemMenuModalComponent;
  let fixture: ComponentFixture<AddEditSystemMenuModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditSystemMenuModalComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddEditSystemMenuModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
