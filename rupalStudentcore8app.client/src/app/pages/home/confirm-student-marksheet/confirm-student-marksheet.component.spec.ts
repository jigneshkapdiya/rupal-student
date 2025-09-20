import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmStudentMarksheetComponent } from './confirm-student-marksheet.component';

describe('ConfirmStudentMarksheetComponent', () => {
  let component: ConfirmStudentMarksheetComponent;
  let fixture: ComponentFixture<ConfirmStudentMarksheetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConfirmStudentMarksheetComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfirmStudentMarksheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
