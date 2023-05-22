import { Component, inject } from '@angular/core';
import { Auth, User, user, signInWithEmailLink, isSignInWithEmailLink } from '@angular/fire/auth';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { Location } from '@angular/common';

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

  isPendingEmailLogin = false;
  isLoggedIn = false;
  isLoading = true;

  constructor(
    private matIconRegistry: MatIconRegistry,
    private sanitizer: DomSanitizer,
    private location: Location,
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
    });

    if (isSignInWithEmailLink(this.auth, window.location.href)) {
      this.isPendingEmailLogin = true;
      signInWithEmailLink(this.auth, localStorage.getItem('emailForSignIn') || '', window.location.href).then((result) => {
        console.log('Logged in with email link: ', result);
      }).catch((err) => {
        console.error('Error logging in with email link: ', err);
        this.isPendingEmailLogin = false;
      }).finally(() => {
        window.localStorage.removeItem('emailForSignIn');
        this.location.replaceState('/');
      });
    }
  }

  ngOnDestroy() {
    this.userSubscription.unsubscribe();
  }
}
