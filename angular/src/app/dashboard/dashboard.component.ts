import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component } from '@angular/core';
import { doc, docData, docSnapshots, Firestore, snapToData } from '@angular/fire/firestore';
import { map, Observable, ReplaySubject, Subject, switchMap, tap } from 'rxjs';
import { CharacterSheet, fromCharacterSheet, toCharacterSheet } from '../models/character-sheet';
import { SearchTerm } from '../models/search-term';

@Component({
  selector: 'rp-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {

  otherSearch$ = new ReplaySubject<SearchTerm>(1);
  otherCharacter$: Observable<{ searchTerm: SearchTerm, character: CharacterSheet }>;
  selfSearch$ = new ReplaySubject<SearchTerm>(1);
  selfCharacter$: Observable<{ searchTerm: SearchTerm, character: CharacterSheet }>;

  preview: CharacterSheet | null = null;

  widthSize: number = 0;

  constructor(
    private firestore: Firestore,
    private breakpointObserver: BreakpointObserver
  ) {
    const switchMapSearchTerm = (term: SearchTerm) => {
      const userDoc = doc(this.firestore, 'rp', term.world.name, 'characters', term.user)
        .withConverter<CharacterSheet>({
          fromFirestore: toCharacterSheet,
          toFirestore: fromCharacterSheet,
        });

      return docSnapshots(userDoc).pipe(
        map((snapshot) => {
          if (snapshot.exists()) {
            console.debug('Character sheet exists', snapshot);
            return { searchTerm: term, character: snapshot.data() };
          } else {
            console.debug('Character sheet does not exist', snapshot);
            return { searchTerm: term, character: { world: term.world.name, user: term.user, exists: false } as CharacterSheet };
          }
        })
      );
    }

    this.otherCharacter$ = this.otherSearch$.pipe(
      switchMap(switchMapSearchTerm)
    );

    this.selfCharacter$ = this.selfSearch$.pipe(
      switchMap(switchMapSearchTerm)
    );
    this.breakpointObserver.observe([
      Breakpoints.Large,
      Breakpoints.XLarge,
    ]).subscribe((result) => {

      if (result.breakpoints[Breakpoints.Large]) {
        this.widthSize = 1;
      } else if (result.breakpoints[Breakpoints.XLarge]) {
        this.widthSize = 2;
      } else {
        this.widthSize = 0;
      }
    });
  }

  ngAfterViewInit() {

  }

  public searchOther(term: SearchTerm) {
    console.debug('searchOther', term);
    this.otherSearch$.next(term);
  }

  public searchSelf(term: SearchTerm) {
    console.debug('searchSelf', term);
    this.selfSearch$.next(term);
  }

  public nextPreview(preview: CharacterSheet) {
    this.preview = preview;
  }
}
