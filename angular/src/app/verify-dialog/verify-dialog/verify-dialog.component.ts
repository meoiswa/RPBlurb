import { HttpClient } from '@angular/common/http';
import { Component, Inject} from '@angular/core';
import { Auth } from '@angular/fire/auth';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'rp-verify-dialog',
  templateUrl: './verify-dialog.component.html',
  styleUrls: ['./verify-dialog.component.scss']
})
export class VerifyDialogComponent {

  private auth: Auth = Inject(Auth);

  private format(data: { user: string, world: string, xivauth: string, uid: string }) {
    return {
      User: data.user,
      World: data.world,
      XivAuth: data.xivauth,
      Uid: data.uid
    }
  }

  constructor(
    public dialogRef: MatDialogRef<VerifyDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { user: string, world: string, xivauth: string, uid: string },
    private httpClient: HttpClient
  ) {
    console.log('VerifyDialogComponent', data);

    if (data && data.user && data.world && data.xivauth && data.uid) {
      this.httpClient.post(
        environment.functions.verifyCharacterFunctionUrl,
        JSON.stringify(this.format(data))
      ).subscribe({
        next: (result) => {
          this.dialogRef.close();
        },
        error: (error) => {
          console.error(error);
          alert(error);
          this.dialogRef.close();
        },
        complete: () => {
          this.dialogRef.close();
        }
      });
    }
  }
}
