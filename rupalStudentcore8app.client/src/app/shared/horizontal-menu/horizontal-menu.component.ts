import { Component, OnInit, ChangeDetectorRef, AfterViewInit, OnDestroy } from '@angular/core';
import { HROUTES } from './navigation-routes.config';
import { LayoutService } from '../services/layout.service';
import { ConfigService } from '../services/config.service';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { RouteInfo } from '../vertical-menu/vertical-menu.metadata';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { MenuService } from '../services/menu.service';


@Component({
  selector: 'app-horizontal-menu',
  templateUrl: './horizontal-menu.component.html',
  styleUrls: ['./horizontal-menu.component.scss']
})
export class HorizontalMenuComponent implements OnInit, AfterViewInit, OnDestroy {
  public menuItems: RouteInfo[];
  public config: any = {};
  level: number = 0;
  transparentBGClass = "";
  menuPosition = 'Top';

  layoutSub: Subscription;
  constructor(private layoutService: LayoutService,
    private configService: ConfigService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private spinner: NgxSpinnerService,
    public toastr: ToastrService,
    private menuService: MenuService,) {
    this.config = this.configService.templateConf;
  }

  ngOnInit() {
    this.getMenuList();
  }
  getMenuList() {
    this.spinner.show("menu");
    this.menuService.getMenuList().subscribe(
      (result) => {
        if (result) {
          this.menuItems = result;
          this.spinner.hide("menu");
        }
      },
      (error) => {
        this.spinner.hide("menu");
        this.toastr.error(error);
      }
    );
  }

  ngAfterViewInit() {

    this.layoutSub = this.configService.templateConf$.subscribe((templateConf) => {
      if (templateConf) {
        this.config = templateConf;
      }
      this.loadLayout();
      this.cdr.markForCheck();

    })
  }

  loadLayout() {
    if (this.config.layout.menuPosition && this.config.layout.menuPosition.toString().trim() != "") {
      this.menuPosition = this.config.layout.menuPosition;
    }

    if (this.config.layout.variant === "Transparent") {
      this.transparentBGClass = this.config.layout.sidebar.backgroundColor;
    }
    else {
      this.transparentBGClass = "";
    }
  }

  ngOnDestroy() {
    if (this.layoutSub) {
      this.layoutSub.unsubscribe();
    }
  }

}
