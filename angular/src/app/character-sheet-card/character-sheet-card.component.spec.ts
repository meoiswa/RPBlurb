import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CharacterSheetCardComponent } from './character-sheet-card.component';

describe('CharacterSheetCardComponent', () => {
  let component: CharacterSheetCardComponent;
  let fixture: ComponentFixture<CharacterSheetCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CharacterSheetCardComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CharacterSheetCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
