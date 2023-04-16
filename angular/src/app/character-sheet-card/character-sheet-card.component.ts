import { Component, Input } from '@angular/core';
import { doc, docData } from '@angular/fire/firestore';
import { Firestore } from '@angular/fire/firestore';
import { Observable, ReplaySubject, Subject, switchMap } from 'rxjs';
import { CharacterSheet } from '../models/character-sheet';
import { SearchTerm } from '../models/search-term';

@Component({
  selector: 'rp-character-sheet-card',
  templateUrl: './character-sheet-card.component.html',
  styleUrls: ['./character-sheet-card.component.scss']
})
export class CharacterSheetCardComponent {

  protected _searchTerm: SearchTerm | null = null;
  protected _character: CharacterSheet | null = null;

  @Input()
  set input(value: { searchTerm: SearchTerm, character: CharacterSheet } | null) {
    console.log('Set Input', value);
    if (value) {
      this._searchTerm = value.searchTerm;
      this._character = value.character;
    }
  }

  get character() {
    return this._character;
  }

  get searchTerm() {
    return this._searchTerm;
  }

  constructor() {
  }
}
