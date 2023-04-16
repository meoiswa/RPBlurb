import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CharacterSheetFormComponent } from './character-sheet-form.component';

describe('CharacterSheetFormComponent', () => {
  let component: CharacterSheetFormComponent;
  let fixture: ComponentFixture<CharacterSheetFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CharacterSheetFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CharacterSheetFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
