import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { Auth } from '@angular/fire/auth';
import { FormControl, FormGroup } from '@angular/forms';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CharacterSheetCardComponent } from '../character-sheet-card/character-sheet-card.component';
import { CharacterSheet, fromCharacterSheet } from '../models/character-sheet';
import { LiveWorld } from '../models/live-world';
import { SearchTerm } from '../models/search-term';

@Component({
  selector: 'rp-character-sheet-form',
  templateUrl: './character-sheet-form.component.html',
  styleUrls: ['./character-sheet-form.component.scss']
})
export class CharacterSheetFormComponent extends CharacterSheetCardComponent {

  private auth: Auth = inject(Auth);

  saving: boolean = false;
  unmodified: boolean = true;

  useFlexColumn: boolean = false;

  owned: boolean = false;

  formGroup = new FormGroup({
    world: new FormControl<string | LiveWorld>(''),
    user: new FormControl<string>(''),
    name: new FormControl<string>(''),
    nameStyle: new FormControl<number>(0),
    title: new FormControl<string>(''),
    alignment: new FormControl<string>(''),
    status: new FormControl<string>(''),
    description: new FormControl<string>(''),
  }, {
    validators: (group) => {
      if (this._character) {
        this._character.name = group.get('name')?.value;
        this._character.nameStyle = group.get('nameStyle')?.value;
        this._character.title = group.get('title')?.value;
        this._character.alignment = group.get('alignment')?.value;
        this._character.status = group.get('status')?.value;
        this._character.description = group.get('description')?.value;
        this.previewCharacter.emit(this._character);
      }
      return null;
    }
  });

  @Output()
  previewCharacter = new EventEmitter<CharacterSheet>();

  override set input(value: { searchTerm: SearchTerm, character: CharacterSheet } | null) {
    console.debug('Set Form Input', value);
    if (value) {
      this._searchTerm = value.searchTerm;

      if (value.character.uids) {
        //TODO: hash UIDs:
        console.log('UIDs:', value.character.uids);
        this.owned = value.character.uids.includes(this.auth.currentUser!.uid);
      } else {
        this.owned = false;
      }

      if (value.character.exists) {
        console.debug('Existing character sheet:', value);
        this._character = value.character;
      } else {
        this._character = {
          world: value.searchTerm.world.name,
          user: value.searchTerm.user,
          name: '',
          nameStyle: 0,
          title: '',
          alignment: '',
          status: '',
          description: '',
          exists: false,
          uids: [],
        }
        console.debug('No character sheet, created:', this._character);
      }
      this.formGroup.patchValue(value.character);
    }
  }


  constructor(
    private breakpointObserver: BreakpointObserver,
    private httpClient: HttpClient) {
    super();

    this.breakpointObserver.observe([
      Breakpoints.XLarge,
    ]).subscribe((result) => {
      this.useFlexColumn = !result.matches;
    });
  }
  // serializes the character sheet and posts it to the server
  public onSubmitClick() {
    if (!this.searchTerm) {
      console.error('No Search Term!');
      return
    }
    console.debug('Environment: ', environment);
    const value = this.formGroup.value as CharacterSheet;
    console.debug('Setting character sheet: ', value);
    if (value && !this.saving) {
      this.saving = true;
      this.httpClient.post(
        environment.functions.setCharacterFunctionUrl,
        JSON.stringify({ uid: this.auth.currentUser!.uid, ...fromCharacterSheet(value) }),
      ).subscribe({
        next: (result) => {
          this.saving = false;
        },
        error: (error) => {
          console.error(error);
          this.saving = false;
        },
        complete: () => {
          this.saving = false;
        }
      });
    }
  }

  public onVerifyClick() {
    var state = JSON.stringify(this.searchTerm);
    localStorage.setItem('verify-character', state);
    console.log('Saved verification state: ', state);

    var host = 'https://edge.xivauth.net';
    var client_id = '_XZHkD-qezoh4_xvRMGk2DJcdqvZhsv0WPhqgDo9uV4';
    var redirect_uri = window.location.origin + '/xivauth/verify';
    var response_type = 'code';
    var scope = 'character:all';

    var url = `${host}/oauth/authorize?client_id=${encodeURIComponent(client_id)}&redirect_uri=${encodeURIComponent(redirect_uri)}&response_type=${encodeURIComponent(response_type)}&scope=${encodeURIComponent(scope)}`;
    console.log('Redirecting to: ', url);
    window.location.href = url;
  }
}
