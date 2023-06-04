import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, inject } from '@angular/core';
import { Auth, user } from '@angular/fire/auth';
import { MatDialog } from '@angular/material/dialog';
import { map, Observable, Subscription } from 'rxjs';
import { StatsDialogComponent } from '../stats-dialog/stats-dialog/stats-dialog.component';

@Component({
  selector: 'rp-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent {

  private auth: Auth = inject(Auth);
  user$ = user(this.auth);

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.HandsetPortrait).pipe(
    map(result => result.matches)
  );

  constructor(
    private breakpointObserver: BreakpointObserver,
    public dialog: MatDialog
  ) {
  }


  clicked(button: string) {
    switch (button) {
      case 'github':
        window.open('https://github.com/meoiswa/RPBlurb', '_blank');
        break;
      case 'ko-fi':
        window.open('https://ko-fi.com/meoiswa', '_blank');
        break;
      case 'patreon':
        window.open('https://patreon.com/meoiswa', '_blank');
        break;
      case 'tweet':
        const text = `RPBlurb is a free tool to help you create and share your Roleplaying Character sheets in Final Fantasy XIV. Visit https://rpblurb.meoiswa.cat to create your own!`;
        window.open('https://twitter.com/intent/tweet?text=' + encodeURI(text), '_blank');
        break;
    }
  }

  public logout() {
    this.auth.signOut();
  }

  public showStats() {
    const dialogRef = this.dialog.open(StatsDialogComponent);
  }
}
