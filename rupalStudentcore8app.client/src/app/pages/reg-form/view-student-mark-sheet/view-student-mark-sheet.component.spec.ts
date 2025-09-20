import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewStudentMarkSheetComponent } from './view-student-mark-sheet.component';

describe('ViewStudentMarkSheetComponent', () => {
  let component: ViewStudentMarkSheetComponent;
  let fixture: ComponentFixture<ViewStudentMarkSheetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewStudentMarkSheetComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewStudentMarkSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
