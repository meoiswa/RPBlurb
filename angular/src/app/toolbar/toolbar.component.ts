import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component } from '@angular/core';
import { map, Observable } from 'rxjs';

@Component({
  selector: 'rp-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.HandsetPortrait).pipe(
    map(result => result.matches)
  );

  constructor(private breakpointObserver: BreakpointObserver) { }


  clicked(button: string) {
    switch (button) {
      case 'source':
        window.open('https://github.com/meoiswa/RPBlurb', '_blank');
        break;
      case 'coffee':
        window.open('https://ko-fi.com/meoiswa', '_blank');
        break;
      case 'share':
        window.open('https://twitter.com/intent/tweet?text=RPBlurb%20is%20a%20free%20tool%20to%20help%20you%20create%20and%20share%20your%20character%20sheets%20for%20roleplaying%20games.%20It%20is%20open%20source%20and%20free%20to%20use%20at%20https%3A%2F%2Frpblurb.com', '_blank');
        break;
    }
  }


}
