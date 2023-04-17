import { Component, ElementRef, EventEmitter, Output } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { map, Observable, startWith, Subject, switchMap } from 'rxjs';
import { LiveWorld } from '../models/live-world';
import { SearchTerm } from '../models/search-term';
import { liveWorlds } from './live-worlds';

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
    private elementRef: ElementRef) {
  }

  ngOnInit() {
    const worlds = liveWorlds.map(world => ({ name: world, enabled: true } as LiveWorld));
    this.filteredWorlds$ = this.formGroup.controls.world.valueChanges.pipe(
      startWith(''),
      // filter the list of worlds based on the input
      map((input) => {
        if (typeof input === 'string') {
          return worlds.filter(world => world.name.toLowerCase().includes(input.toLowerCase()));
        } else {
          return worlds;
        }
      }),
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
