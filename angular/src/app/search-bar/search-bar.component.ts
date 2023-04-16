import { Component, ElementRef, EventEmitter, Output } from '@angular/core';
import { collection, collectionData, Firestore } from '@angular/fire/firestore';
import { FormControl, FormGroup } from '@angular/forms';
import { combineLatest, map, Observable, startWith, Subject, switchMap } from 'rxjs';
import { LiveWorld } from '../models/live-world';
import { SearchTerm } from '../models/search-term';

@Component({
  selector: 'rp-search-bar',
  templateUrl: './search-bar.component.html',
  styleUrls: ['./search-bar.component.scss']
})
export class SearchBarComponent {

  form = new FormControl

  formGroup = new FormGroup({
    world: new FormControl<string | LiveWorld>(''),
    user: new FormControl<string>(''),
  }, {
    validators: (group) => {
      const world = group.get('world');
      const user = group.get('user');
      return world && user && world.value && world.value.name && user.value ? null : { required: true };
    }
  });
  filteredWorlds$: Observable<LiveWorld[]> | undefined;

  // output the selection
  @Output()
  selection = new EventEmitter<SearchTerm>();

  constructor(
    private firestore: Firestore,
    private elementRef: ElementRef) {
  }

  ngOnInit() {
    const itemCollection = collection(this.firestore, 'LiveWorlds').withConverter<LiveWorld>({
      toFirestore: (item) => item,
      fromFirestore: (snapshot, options) => {
        const data = snapshot.data(options);
        return { name: snapshot.ref.id, ...data } as LiveWorld;
      }
    });
    const liveWorlds$ = collectionData(itemCollection);
    this.filteredWorlds$ = combineLatest([
      this.formGroup.controls.world.valueChanges.pipe(startWith('')),
      liveWorlds$
    ]).pipe(
      // filter the list of worlds based on the input
      map(([input, worlds]) => {
        if (typeof input === 'string') {
          return worlds.filter(world => world.name.toLowerCase().includes(input.toLowerCase()));
        } else {
          return worlds;
        }
      })
    );

    // load the selection from localstorage and set it as the formgroup value
    const selection = localStorage.getItem('selection#' + this.elementRef.nativeElement.id);
    if (selection) {
      this.formGroup.setValue(JSON.parse(selection));
      this.submit();
    }
  }

  displayFn(world: LiveWorld): string {
    return world && world.name ? world.name : '';
  }

  submit() {
    if (this.formGroup.valid) {
      const selection = this.formGroup.value as SearchTerm;
      localStorage.setItem('selection#' + this.elementRef.nativeElement.id, JSON.stringify(selection));
      this.selection.emit(selection);
    } else {
      console.warn('Invalid form', this.formGroup);
    }
  }

  clear() {
    this.formGroup.reset();
    localStorage.removeItem('selection#' + this.elementRef.nativeElement.id);
  }
}
