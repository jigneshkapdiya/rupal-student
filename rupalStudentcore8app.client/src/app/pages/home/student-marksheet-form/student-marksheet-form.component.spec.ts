import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentMarksheetFormComponent } from './student-marksheet-form.component';

describe('StudentMarksheetFormComponent', () => {
  let component: StudentMarksheetFormComponent;
  let fixture: ComponentFixture<StudentMarksheetFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StudentMarksheetFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentMarksheetFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
