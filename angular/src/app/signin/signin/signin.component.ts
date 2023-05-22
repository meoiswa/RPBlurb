import { Component, inject } from '@angular/core';
import { Auth, sendSignInLinkToEmail, signInWithPopup, GoogleAuthProvider } from '@angular/fire/auth';
import { MatDialog } from '@angular/material/dialog';
import { CloseDialogComponent } from '../close-dialog/close-dialog/close-dialog.component';
import { EmailDialogComponent } from '../email-dialog/email-dialog/email-dialog.component';


@Component({
  selector: 'rp-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss']
})
export class SigninComponent {

  private auth: Auth = inject(Auth);

  public loading = false;

  constructor(public dialog: MatDialog) { }

  public signInEmail() {
    const dialogRef = this.dialog.open(EmailDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.length > 0) {
        this.loading = true;
        console.log(`Dialog result: ${result}`);
        localStorage.setItem('emailForSignIn', result);
        sendSignInLinkToEmail(this.auth, result, {
          url: window.location.href,
          handleCodeInApp: true,
        }).then(() => {
          const closeRef = this.dialog.open(CloseDialogComponent);

          closeRef.afterClosed().subscribe(() => {
            this.loading = false;
          });
        });
      }
    });
  }

  public signInGoogle() {
    this.loading = true;

    signInWithPopup(this.auth, new GoogleAuthProvider()).then((result) => {
      console.log('Logged in with Google: ', result);
    });
  }
}
