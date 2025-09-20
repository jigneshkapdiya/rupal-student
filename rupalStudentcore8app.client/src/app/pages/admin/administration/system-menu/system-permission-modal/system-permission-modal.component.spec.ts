import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystemPermissionModalComponent } from './system-permission-modal.component';

describe('SystemPermissionModalComponent', () => {
  let component: SystemPermissionModalComponent;
  let fixture: ComponentFixture<SystemPermissionModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SystemPermissionModalComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SystemPermissionModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
