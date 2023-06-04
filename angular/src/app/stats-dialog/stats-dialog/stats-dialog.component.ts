import { Component } from '@angular/core';
import { doc, docSnapshots, Firestore } from '@angular/fire/firestore';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'rp-stats-dialog',
  templateUrl: './stats-dialog.component.html',
  styleUrls: ['./stats-dialog.component.scss']
})
export class StatsDialogComponent {
  data: { key: string, value: number }[] = [];
  total: number = 0;
  largest: number = 0;

  constructor(
    private firestore: Firestore,
    public dialogRef: MatDialogRef<StatsDialogComponent>,
  ) {
    var statsDoc = doc(this.firestore, 'stats', 'stats');
    docSnapshots(statsDoc).subscribe((snapshot) => {
      if (snapshot.exists()) {
        console.debug('Stats exists', snapshot.data());
        var data = snapshot.data();
        this.data = Object.keys(data as any)
          .map((key) => { return { key: key, value: data[key] } })
          .filter((item) => item.value > 0)
          .sort((a, b) => b.value - a.value);
        this.total = this.data[0].value;
        this.data = this.data.slice(1);
        this.largest = this.data.reduce((a, b) => Math.max(a, b.value), 0);
      }
    });
  }

  onNoClick() {
    this.dialogRef.close();
  }
}
