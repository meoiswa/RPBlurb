import { Component, inject } from '@angular/core';
import { Auth, User, user, signInWithEmailLink, isSignInWithEmailLink } from '@angular/fire/auth';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { Location } from '@angular/common';
import { SearchTerm } from './models/search-term';
import { VerifyDialogComponent } from './verify-dialog/verify-dialog/verify-dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'rp-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'rpblurb-spa';

  private auth: Auth = inject(Auth);
  user$ = user(this.auth);
  userSubscription: Subscription;

  isPendingValidation = false;
  isLoggedIn = false;
  isLoading = true;

  constructor(
    private matIconRegistry: MatIconRegistry,
    private sanitizer: DomSanitizer,
    private location: Location,
    private dialog: MatDialog
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
    this.matIconRegistry.addSvgIcon(
      'google',
      sanitizer.bypassSecurityTrustResourceUrl('assets/google.svg')
    );

    this.userSubscription = this.user$.subscribe((aUser: User | null) => {
      console.log('User$: ', aUser);
      this.isLoading = false;
      this.isLoggedIn = !!aUser;

      if (aUser && window.location.href.includes('/xivauth/verify')) {
        this.isPendingValidation = true;
        var verifyStateJson = localStorage.getItem('verify-character');
        if (verifyStateJson) {
          var verifyState = JSON.parse(verifyStateJson) as SearchTerm;
          if (verifyState) {
            var code = new URL(window.location.href).searchParams.get('code');
            if (code) {
              console.log('Validating Character: Got Authorization Code', verifyState, code);
              var dialogRef = this.dialog.open(VerifyDialogComponent, {
                disableClose: true,
                closeOnNavigation: false,
                data: {
                  user: verifyState.user,
                  world: verifyState.world.name,
                  xivauth: code,
                  uid: aUser.uid
                }
              });
              dialogRef.afterClosed().subscribe((result) => {
                this.isPendingValidation = false;
                this.location.replaceState('/');
              });
            }
          }
        }
      }
    });
  }

  ngOnDestroy() {
    this.userSubscription.unsubscribe();
  }
}
