import { Injectable } from "@angular/core";
import { _ } from "core-js";
import { BehaviorSubject } from "rxjs";

export interface ITemplateConfig {
  layout: {
    variant: string;                   // options: Dark, Light & Transparent
    menuPosition: string;              // options: Side, Top (Note: Use 'Side' for Vertical Menu & 'Top' for Horizontal Menu )
    customizer: {
      hidden: boolean;               // options: true, false
    };
    navbar: {
      type: string;                     // options: Static & Fixed
    }
    sidebar: { //Options for Vertical Side menu
      collapsed: boolean;             // options: true, false
      size: string;                   // Options: 'sidebar-lg', 'sidebar-md', 'sidebar-sm'
      backgroundColor: string;        // Options: 'black', 'pomegranate', 'king-yna', 'ibiza-sunset', 'flickr', 'purple-bliss', 'man-of-steel', 'purple-love'

      /* If you want transparent layout add any of the following mentioned classes to backgroundColor of sidebar option :
        bg-glass-1, bg-glass-2, bg-glass-3, bg-glass-4, bg-hibiscus, bg-purple-pizzaz, bg-blue-lagoon, bg-electric-viloet, bg-protage, bg-tundora
      */
      backgroundImage: boolean;        // Options: true, false | Set true to show background image
      backgroundImageURL: string;
    }
  };
}


@Injectable({
  providedIn: "root"
})
export class ConfigService {
  public templateConf: ITemplateConfig = this.setConfigValue();
  templateConfSubject = new BehaviorSubject<ITemplateConfig>(this.templateConf);
  templateConf$ = this.templateConfSubject.asObservable();

  constructor() {
  }

  // Default configurations for Light layout. Please check *customizer.service.ts* for different colors and bg images options

  setConfigValue() {
    //get from local storage
    // let layout = localStorage.getItem('layout');
    // let menuPosition = localStorage.getItem('menuPosition');
    // let sidebarSize = localStorage.getItem('sidebarSize');
    // let sidebarColor = localStorage.getItem('sidebarColor');
    // let sidebarImage = localStorage.getItem('sidebarImage');
    // let sidebarImageDisplay = localStorage.getItem('sidebarImageDisplay');
    // if (layout && menuPosition && sidebarSize && sidebarColor && sidebarImage && sidebarImageDisplay) {
    //   this.templateConf = {} as ITemplateConfig;
    //   this.templateConf.layout = {} as ITemplateConfig["layout"];
    //   this.templateConf.layout.customizer = {} as ITemplateConfig["layout"]["customizer"];
    //   this.templateConf.layout.navbar = {} as ITemplateConfig["layout"]["navbar"];
    //   this.templateConf.layout.sidebar = {} as ITemplateConfig["layout"]["sidebar"];
    //   this.templateConf.layout.variant = layout;
    //   this.templateConf.layout.menuPosition = menuPosition;
    //   this.templateConf.layout.customizer.hidden = true;

    //   if (this.templateConf == null) {
    //     this.templateConf = {} as ITemplateConfig;
    //     this.templateConf.layout = {} as ITemplateConfig["layout"];
    //     this.templateConf.layout.customizer = {} as ITemplateConfig["layout"]["customizer"];
    //     this.templateConf.layout.navbar = {} as ITemplateConfig["layout"]["navbar"];
    //     this.templateConf.layout.sidebar = {} as ITemplateConfig["layout"]["sidebar"];
    //     this.templateConf.layout.customizer.hidden = true;
    //     this.templateConf.layout.navbar.type = 'Static';
    //     this.templateConf.layout.sidebar.collapsed = false;
    //     this.templateConf.layout.sidebar.size = "sidebar-md";
    //     this.templateConf.layout.sidebar.backgroundColor = "man-of-steel";
    //     this.templateConf.layout.sidebar.backgroundImage = true;
    //     this.templateConf.layout.sidebar.backgroundImageURL = "assets/img/sidebar-bg/01.jpg";
    //   }
    //   return this.templateConf;
    return this.templateConf = {
      layout: {
        variant: "Light",
        menuPosition: "Side",
        customizer: {
          hidden: true
        },
        navbar: {
          type: 'Static'
        },
        sidebar: {
          collapsed: false,
          size: "sidebar-md",
          backgroundColor: "man-of-steel",
          backgroundImage: true,
          backgroundImageURL: "assets/img/sidebar-bg/01.jpg"
        }
      }
    };
  }

  applyTemplateConfigChange(tempConfig: ITemplateConfig) {
    this.templateConf = Object.assign(this.templateConf, tempConfig);
    this.templateConfSubject.next(this.templateConf);
  }
};
// Default configurations for Dark layout. Please check *customizer.service.ts* for different colors and bg images options

// setConfigValue() {
//   return this.templateConf = {
//     layout: {
//       variant: "Dark",
//       menuPosition: "Side",
//       customizer: {
//         hidden: true
//       },
//       navbar: {
//         type: 'Static'
//       },
//       sidebar: {
//         collapsed: false,
//         size: "sidebar-md",
//         backgroundColor: "black",
//         backgroundImage: true,
//         backgroundImageURL: "assets/img/sidebar-bg/01.jpg"
//       }
//     }
//   };
// }

// Default configurations for Transparent layout. Please check *customizer.service.ts* for different colors and bg images options

// setConfigValue() {
//   return this.templateConf = {
//     layout: {
//       variant: "Transparent",
//       menuPosition: "Side",
//       customizer: {
//         hidden: true
//       },
//       navbar: {
//         type: 'Static'
//       },
//       sidebar: {
//         collapsed: false,
//         size: "sidebar-md",
//         backgroundColor: "bg-glass-1",
//         backgroundImage: true,
//         backgroundImageURL: ""
//       }
//     }
//   };
// }



