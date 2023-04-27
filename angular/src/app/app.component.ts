import { Component } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'rp-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'rpblurb-spa';

  constructor(
    private matIconRegistry: MatIconRegistry,
    private sanitizer: DomSanitizer,
  ) {
    this.matIconRegistry.addSvgIcon(
      'patreon',
      sanitizer.bypassSecurityTrustResourceUrl('assets/patreon.svg')
    );
    this.matIconRegistry.addSvgIcon(
      'ko-fi',
      sanitizer.bypassSecurityTrustResourceUrl('assets/ko-fi.svg')
    );
    this.matIconRegistry.addSvgIcon(
      'github',
      sanitizer.bypassSecurityTrustResourceUrl('assets/github.svg')
    );
    this.matIconRegistry.addSvgIcon(
      'twitter',
      sanitizer.bypassSecurityTrustResourceUrl('assets/twitter.svg')
    );
  }
}
