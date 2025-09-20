import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystemPermissionsComponent } from './system-permissions.component';

describe('SystemPermissionsComponent', () => {
  let component: SystemPermissionsComponent;
  let fixture: ComponentFixture<SystemPermissionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SystemPermissionsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SystemPermissionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
