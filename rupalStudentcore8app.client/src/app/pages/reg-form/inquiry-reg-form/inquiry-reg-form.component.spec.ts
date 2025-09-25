import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InquiryRegFormComponent } from './inquiry-reg-form.component';

describe('InquiryRegFormComponent', () => {
  let component: InquiryRegFormComponent;
  let fixture: ComponentFixture<InquiryRegFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InquiryRegFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InquiryRegFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
