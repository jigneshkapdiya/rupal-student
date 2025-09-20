import { Component, OnInit } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AdministrationService } from '../../_services/administration.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'system-permissions',
  templateUrl: './system-permissions.component.html',
  styleUrls: ['./system-permissions.component.scss']
})
export class SystemPermissionsComponent implements OnInit {
  roleList: any[] = [];
  permissionDetails;
  menuId: any[] = [];
  permissionList: any[] = [];
  roleId: number;
  selectedRole: string;
  addPermission = true;

  constructor(
    public toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private administrationService: AdministrationService,
  ) { }

  ngOnInit(): void {
    this.getRoleList();
  }

  getRoleList() {
    this.spinner.show();
    this.administrationService.getRoleList().pipe(finalize(() => this.spinner.hide())).subscribe(
      (response) => {
        this.roleList = response;
      },
      (error) => {
        this.toastr.error('Error fetching role list');
      }
    );
  }

  onChangeRole(role) {
    if (role) {
      this.spinner.show();
      this.menuId = [];
      this.administrationService.getPermissions(role.id).subscribe(
        (result) => {
          if (result) {
            this.spinner.hide();
            this.permissionDetails = result.data;
            result.data.map((c) => {
              if (c.selected) {
                this.menuId.push(c.id);
                c.permissions.map((pm) => {
                  if (pm.selected) {
                    this.permissionList.push({ ...pm });
                  }
                });
                if (c.subList.length !== 0) {
                  c.subList.map((p) => {
                    if (p.selected) {
                      this.menuId.push(p.id);
                      p.permissions.map((pm) => {
                        if (pm.selected) {
                          this.permissionList.push({ ...pm });
                        }
                      });
                      if (p.subList.length !== 0) {
                        p.subList.map(s => {
                          if (s.selected) {
                            this.menuId.push(s.id);
                            s.permissions.map((pm) => {
                              if (pm.selected) {
                                this.permissionList.push({ ...pm });
                              }
                            });
                          }
                        })
                      }
                    }
                  });
                }
              }
            });
            this.roleId = role.id;
          }
        },
        (error) => {
          this.spinner.hide();
          this.toastr.error(error);
        }
      );
    } else {
      this.permissionDetails = null;
      this.menuId = [];
    }
  }

  onFirstLevelCheckBox(e, id) {
    if (e.target.checked) {
      this.menuId.push(Number(id));
      this.permissionDetails.forEach((x) => {
        if (x.id === id) {
          x.selected = e.target.checked;
          x.subList.forEach((s) => {
            if (s.parentId === id) {
              this.menuId.push(Number(s.id));
              s.selected = e.target.checked;
              s.subList.forEach((ss) => {
                if (ss.parentId === s.id) {
                  this.menuId.push(Number(ss.id));
                  ss.selected = e.target.checked;
                  ss.permissions.forEach((p) => {
                    this.permissionList.push({ ...p });
                    p.selected = e.target.checked;
                  });
                }
              });
              s.permissions.forEach((p) => {
                this.permissionList.push({ ...p });
                p.selected = e.target.checked;
              });
            }
          });
          x.permissions.forEach((p) => {
            this.permissionList.push({ ...p });
            p.selected = e.target.checked;
          });
        }
      });
    } else {
      this.permissionDetails.forEach((x) => {
        if (x.id === id) {
          x.selected = e.target.checked;
          x.subList.forEach((s) => {
            if (s.parentId === id) {
              s.selected = e.target.checked;
              s.subList.forEach((ss) => {
                if (ss.parentId === s.id) {
                  ss.selected = e.target.checked;
                  const index = this.menuId.indexOf(ss.id);
                  if (index > -1) {
                    this.menuId.splice(index, 1);
                  }
                  ss.permissions.forEach((p) => {
                    const ind = this.permissionList.findIndex(i => i.id === p.id);
                    if (ind > -1) {
                      this.permissionList.splice(ind, 1);
                    }
                    p.selected = e.target.checked;
                  });
                }
              });
              const index = this.menuId.indexOf(s.id);
              if (index > -1) {
                this.menuId.splice(index, 1);
              }
              s.permissions.forEach((p) => {
                const ind = this.permissionList.findIndex(i => i.id === p.id);
                if (ind > -1) {
                  this.permissionList.splice(ind, 1);
                }
                p.selected = e.target.checked;
              });
            }
          });
          x.permissions.forEach((p) => {
            const ind = this.permissionList.findIndex(i => i.id === p.id);
            if (ind > -1) {
              this.permissionList.splice(ind, 1);
            }
            p.selected = e.target.checked;
          });
        }
      });
      const index = this.menuId.indexOf(id);
      if (index > -1) {
        this.menuId.splice(index, 1);
      }
    }
  }

  onSecondLevelCheckBox(e, item) {
    if (e.target.checked) {
      this.menuId.push(Number(item.id));
      this.permissionDetails.forEach((x) => {
        if (x.id === item.parentId) {
          x.selected = e.target.checked;
          x.subList.forEach((s) => {
            if (s.id === item.id) {
              s.selected = e.target.checked;
              s.subList.forEach((ss) => {
                if (ss.parentId === item.id) {
                  this.menuId.push(Number(ss.id));
                  ss.selected = e.target.checked;
                  ss.permissions.forEach((p) => {
                    this.permissionList.push({ ...p });
                    p.selected = e.target.checked;
                  });
                }
              });
              s.permissions.forEach((p) => {
                this.permissionList.push({ ...p });
                p.selected = e.target.checked;
              });
            }
          });
          x.permissions.forEach((p) => {
            this.permissionList.push({ ...p });
            p.selected = e.target.checked;
          });
        }
      });
    } else {
      this.permissionDetails.forEach((x) => {
        if (x.id === item.parentId) {
          x.subList.forEach((s) => {
            if (s.id === item.id) {
              s.selected = e.target.checked;
              s.subList.forEach((ss) => {
                if (ss.parentId === item.id) {
                  ss.selected = e.target.checked;
                  ss.permissions.forEach((p) => {
                    const ind = this.permissionList.findIndex(i => i.id === p.id);
                    if (ind > -1) {
                      this.permissionList.splice(ind, 1);
                    }
                    p.selected = e.target.checked;
                  });
                  const index = this.menuId.indexOf(ss.id);
                  if (index > -1) {
                    this.menuId.splice(index, 1);
                  }
                }
              });
              s.permissions.forEach((p) => {
                const ind = this.permissionList.findIndex(i => i.id === p.id);
                if (ind > -1) {
                  this.permissionList.splice(ind, 1);
                }
                p.selected = e.target.checked;
              });
            }
          });
          x.permissions.forEach((p) => {
            const ind = this.permissionList.findIndex(i => i.id === p.id);
            if (ind > -1) {
              this.permissionList.splice(ind, 1);
            }
            p.selected = e.target.checked;
          });
          const parentSelected = !x.subList.every((l) => l.selected === false);
          x.selected = parentSelected;
          if (!parentSelected) {
            // tslint:disable-next-line:no-shadowed-variable
            const index = this.menuId.indexOf(item.parentId);
            if (index > -1) {
              this.menuId.splice(index, 1);
            }
          }
        }
      });
      const index = this.menuId.indexOf(item.id);
      if (index > -1) {
        this.menuId.splice(index, 1);
      }
    }
  }

  onThirdLevelCheckBox(e, item, secondId: number, firstId: number) {
    if (e.target.checked) {
      this.menuId.push(Number(item.id));
      this.permissionDetails.filter(f => f.id === firstId).forEach((x) => {
        x.subList.filter(f => f.id === secondId).forEach((s) => {
          if (s.id === item.parentId) {
            s.selected = e.target.checked;
            s.subList.forEach((ss) => {
              if (ss.id === item.id) {
                ss.selected = e.target.checked;
                ss.permissions.forEach((p) => {
                  this.permissionList.push({ ...p });
                  p.selected = e.target.checked;
                });
              }
            });
          }
        });
      });
    } else {
      this.permissionDetails.filter(f => f.id === firstId).forEach((x) => {
        x.subList.filter(f => f.id === secondId).forEach((s) => {
          if (s.id === item.parentId) {
            s.subList.forEach(ss => {
              if (ss.id === item.id) {
                ss.selected = e.target.checked;
                ss.permissions.forEach((p) => {
                  const ind = this.permissionList.findIndex(i => i.id === p.id);
                  if (ind > -1) {
                    this.permissionList.splice(ind, 1);
                  }
                  p.selected = e.target.checked;
                });
              }
            })
            const parentSelected = !s.subList.every((l) => l.selected === false);
            s.selected = parentSelected;
            if (!parentSelected) {
              const index = this.menuId.indexOf(s.id);
              if (index > -1) {
                this.menuId.splice(index, 1);
              }
            }
          }
        });
        const parentSelected = !x.subList.every((l) => l.selected === false);
        x.selected = parentSelected;
        if (!parentSelected) {
          const index = this.menuId.indexOf(x.id);
          if (index > -1) {
            this.menuId.splice(index, 1);
          }
        }
      });
      const index = this.menuId.indexOf(item.id);
      if (index > -1) {
        this.menuId.splice(index, 1);
      }
    }
  }

  onChildCheckBoxForClaim(e, item) {
    if (e.target.checked) {
      this.permissionDetails.forEach((x) => {
        if (x.id === item.menuId) {
          x.selected = e.target.checked;
          x.permissions.forEach((p) => {
            if (p.id === item.id) {
              this.permissionList.push({ ...p });
              p.selected = e.target.checked;
            }
          });
        }
        x.subList.forEach((s) => {
          if (s.id === item.menuId) {
            s.selected = e.target.checked;
            s.permissions.forEach((p) => {
              if (p.id === item.id) {
                this.permissionList.push({ ...p });
                p.selected = e.target.checked;
              }
            });
          }
          s.subList.forEach((ss) => {
            if (ss.id === item.menuId) {
              ss.selected = e.target.checked;
              ss.permissions.forEach((p) => {
                if (p.id === item.id) {
                  this.permissionList.push({ ...p });
                  p.selected = e.target.checked;
                }
              });
            }
          });
        });
      });
    } else {
      this.permissionDetails.map((x) => {
        if (x.id === item.menuId) {
          x.permissions.map((p) => {
            if (p.id === item.id) {
              const ind = this.permissionList.findIndex(i => i.id === p.id);
              if (ind > -1) {
                this.permissionList.splice(ind, 1);
              }
              p.selected = e.target.checked;
            }
          });
        }
        x.subList.map((s) => {
          if (s.id === item.menuId) {
            s.permissions.map((p) => {
              if (p.id === item.id) {
                const ind = this.permissionList.findIndex(i => i.id === p.id);
                if (ind > -1) {
                  this.permissionList.splice(ind, 1);
                }
                p.selected = e.target.checked;
              }
            });
          }
          s.subList.map((ss) => {
            if (ss.id === item.menuId) {
              ss.permissions.map((p) => {
                if (p.id === item.id) {
                  const ind = this.permissionList.findIndex(i => i.id === p.id);
                  if (ind > -1) {
                    this.permissionList.splice(ind, 1);
                  }
                  p.selected = e.target.checked;
                }
              });
            }
          });
        });
      });
    }
  }

  onClickSubmit() {
    if (this.menuId.length > 0) {
      this.spinner.show();
      const permissionData = {
        roleId: this.roleId,
        menuId: this.menuId,
        permissions: this.permissionList
      };
      this.administrationService.postPermissions(permissionData).subscribe(
        (result) => {
          if (result) {
            this.spinner.hide();
            this.permissionDetails = null;
            this.menuId = [];
            this.permissionList = [];
            this.selectedRole = null;
            this.toastr.success("Permission assigned successfully");
          }
        },
        (error) => {
          this.spinner.hide();
          this.toastr.error(error, "Failed to assigned Permission");
        }
      );
    }
  }

}
