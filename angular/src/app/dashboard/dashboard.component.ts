import { Component } from '@angular/core';
import { doc, docData, docSnapshots, Firestore, snapToData } from '@angular/fire/firestore';
import { map, Observable, ReplaySubject, Subject, switchMap } from 'rxjs';
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

  constructor(private firestore: Firestore) {
    const switchMapSearchTerm = (term: SearchTerm) => {
      const userDoc = doc(this.firestore, 'rp', term.world.name, 'characters', term.user)
        .withConverter<CharacterSheet>({
          fromFirestore: toCharacterSheet,
          toFirestore: fromCharacterSheet,
        });

      return docSnapshots(userDoc).pipe(
        map((snapshot) => {
          if (snapshot.exists()) {
            console.log('Character sheet exists', snapshot);
            return { searchTerm: term, character: snapshot.data() };
          } else {
            console.log('Character sheet does not exist', snapshot);
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
  }

  public searchOther(term: SearchTerm) {
    console.log('searchOther', term);
    this.otherSearch$.next(term);
  }

  public searchSelf(term: SearchTerm) {
    console.log('searchSelf', term);
    this.selfSearch$.next(term);
  }
}
