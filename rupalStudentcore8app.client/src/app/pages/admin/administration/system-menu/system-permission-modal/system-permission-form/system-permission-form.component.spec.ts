import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystemPermissionFormComponent } from './system-permission-form.component';

describe('SystemPermissionFormComponent', () => {
  let component: SystemPermissionFormComponent;
  let fixture: ComponentFixture<SystemPermissionFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SystemPermissionFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SystemPermissionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
