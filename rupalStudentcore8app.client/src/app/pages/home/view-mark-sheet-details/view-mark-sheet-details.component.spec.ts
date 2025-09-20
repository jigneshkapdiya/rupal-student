import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewMarkSheetDetailsComponent } from './view-mark-sheet-details.component';

describe('ViewMarkSheetDetailsComponent', () => {
  let component: ViewMarkSheetDetailsComponent;
  let fixture: ComponentFixture<ViewMarkSheetDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewMarkSheetDetailsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewMarkSheetDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
