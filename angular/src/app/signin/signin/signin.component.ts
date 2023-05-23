import { Component, inject } from '@angular/core';
import { Auth, signInWithEmailAndPassword, createUserWithEmailAndPassword, signInWithPopup, GoogleAuthProvider, sendPasswordResetEmail } from '@angular/fire/auth';
import { MatDialog } from '@angular/material/dialog';
import { error } from 'firebase-functions/logger';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'rp-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss']
})
export class SigninComponent {

  private auth: Auth = inject(Auth);

  public loading = false;

  emailFormControl = new FormControl('', [Validators.required, Validators.email]);
  passwordFormControl = new FormControl('', [Validators.required, Validators.minLength(6)]);

  constructor(public dialog: MatDialog) { }

  public onSignInClick() {
    if (this.emailFormControl.value && this.passwordFormControl.value) {
      this.loading = true;
      signInWithEmailAndPassword(this.auth, this.emailFormControl.value, this.passwordFormControl.value)
        .catch((error) => {
          if (error.code === 'auth/user-not-found') {
            createUserWithEmailAndPassword(this.auth, this.emailFormControl.value!, this.passwordFormControl.value!)
              .catch((error) => {
                this.loading = false;
                if (error.code === 'auth/invalid-email') {
                  alert('Invalid email');
                } else if (error.code === 'auth/weak-password') {
                  alert('Weak password');
                } else {
                  alert(error.message);
                }
                console.error(error);
              });
          } else if (error.code === 'auth/wrong-password') {
            alert('Wrong password');
          } else if (error.code === 'auth/invalid-email') {
            alert('Invalid email');
          } else if (error.code === 'auth/too-many-requests') {
            alert('Your account has been disabled due to too many failed requests, pelase use the Forgot Password? function to reset your password');
          } else {
            alert(error.message);
          }
          this.loading = false;
          console.error(error);
        });
    }
  }

  public onForgotClick() {

  }

}
