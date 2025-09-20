import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, HostListener, OnDestroy, OnInit, Output } from '@angular/core';
import { SafeUrl, Title } from '@angular/platform-browser';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService, SignInUser } from '../auth/auth.service';
import { ConfigService } from '../services/config.service';
import { LayoutService } from '../services/layout.service';
import { ProfileService } from './../../pages/admin/_services/profile.service';

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.scss"]
})
export class NavbarComponent implements OnInit, AfterViewInit, OnDestroy {
  selectedLanguageText = "English";
  selectedLanguageFlag = "./assets/image/flags/us.png";
  toggleClass = "ft-maximize";
  placement = "bottom-right";
  menuPosition = 'Side';
  isSmallScreen = false;


  protected innerWidth: any;
  transparentBGClass = "";
  hideSidebar: boolean = true;
  public isCollapsed = true;
  layoutSub: Subscription;
  configSub: Subscription;
  title: any;
  imageProfile: string | SafeUrl;

  @Output()
  toggleHideSidebar = new EventEmitter<Object>();
  user: SignInUser;
  public config: any = {};

  constructor(public translate: TranslateService,
    private layoutService: LayoutService,
    private activatedRoute: ActivatedRoute,
    private pageTitle: Title,
    private authService: AuthService,
    private router: Router,
    public toastr: ToastrService,
    private profileService: ProfileService,
    private configService: ConfigService,
    private cdr: ChangeDetectorRef) {
    this.user = this.authService.getSignInUser();
    const browserLang: string = translate.getBrowserLang();
    translate.use(browserLang.match(/en|es|pt|de/) ? browserLang : "en");
    this.config = this.configService.templateConf;
    this.innerWidth = window.innerWidth;
    this.user = this.authService.getSignInUser();

    this.layoutSub = layoutService.toggleSidebar$.subscribe(
      isShow => {
        this.hideSidebar = !isShow;
      });

    // -- translate language -- //
    const language = this.authService.getLanguage();
    if (language === 'ar') {
      this.selectedLanguageText = "Arabic";
      this.selectedLanguageFlag = "./assets/image/flags/ar.png";
    } else {
      this.selectedLanguageText = "English";
      this.selectedLanguageFlag = "./assets/image/flags/us.png";
    }

    // set title after click
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        const rt = this.getChild(this.activatedRoute);
        rt.data.subscribe((data) => {
          if (this.selectedLanguageText == "Arabic") {
            this.title = data.title_ar;
            this.pageTitle.setTitle(data.title_ar);
          } else {
            this.title = data.title;
            this.pageTitle.setTitle(data.title);
          }
        });
      });
  }

  ngOnInit() {
    if (this.innerWidth < 1200) {
      this.isSmallScreen = true;
    }
    else {
      this.isSmallScreen = false;
    }
    this.getUserById();
  }

  getChild(activatedRoute: ActivatedRoute) {
    if (activatedRoute.firstChild) {
      return this.getChild(activatedRoute.firstChild);
    } else {
      return activatedRoute;
    }
  }

  getUserById() {
    this.profileService.getUserById().subscribe(
      (result) => {
        if (result) {
          this.imageProfile = result.profileImage;
        }
      },
      (error) => {
        this.toastr.error(error);
      }
    );
  }

  ngAfterViewInit() {
    this.configSub = this.configService.templateConf$.subscribe((templateConf) => {
      if (templateConf) {
        this.config = templateConf;
      }
      this.loadLayout();
      this.cdr.markForCheck();

    })
  }

  ngOnDestroy() {
    if (this.layoutSub) {
      this.layoutSub.unsubscribe();
    }
    if (this.configSub) {
      this.configSub.unsubscribe();
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.innerWidth = event.target.innerWidth;
    if (this.innerWidth < 1200) {
      this.isSmallScreen = true;
    }
    else {
      this.isSmallScreen = false;
    }
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

  ChangeLanguage(language: string) {
    if (language === 'ar') {
      this.selectedLanguageText = "Arabic";
      this.selectedLanguageFlag = "./assets/image/flags/ar.png";
    }
    else {
      this.selectedLanguageText = "English";
      this.selectedLanguageFlag = "./assets/image/flags/us.png";
    }
    this.authService.setLanguage(language);
    window.location.reload();
  }

  ToggleClass() {
    if (this.toggleClass === "ft-maximize") {
      this.toggleClass = "ft-minimize";
    } else {
      this.toggleClass = "ft-maximize";
    }
  }

  toggleSidebar() {
    this.layoutService.toggleSidebarSmallScreen(this.hideSidebar);
  }

  logout() {
    this.authService.logout();
  }

  toggleNotificationSidebar() {
    this.layoutService.toggleNotificationSidebar(true);
  }
}
