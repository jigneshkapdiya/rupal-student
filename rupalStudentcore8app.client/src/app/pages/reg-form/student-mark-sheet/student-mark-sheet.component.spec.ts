import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentMarkSheetComponent } from './student-mark-sheet.component';

describe('StudentMarkSheetComponent', () => {
  let component: StudentMarkSheetComponent;
  let fixture: ComponentFixture<StudentMarkSheetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StudentMarkSheetComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentMarkSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
