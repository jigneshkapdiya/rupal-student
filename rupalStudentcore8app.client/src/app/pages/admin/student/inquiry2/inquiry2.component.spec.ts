import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Inquiry2Component } from './inquiry2.component';

describe('Inquiry2Component', () => {
  let component: Inquiry2Component;
  let fixture: ComponentFixture<Inquiry2Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ Inquiry2Component ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Inquiry2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
