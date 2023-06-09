import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { initializeApp, provideFirebaseApp } from '@angular/fire/app';
import { getAuth, provideAuth } from '@angular/fire/auth';

import { environment } from '../environments/environment';
import { provideFirestore, getFirestore, connectFirestoreEmulator } from '@angular/fire/firestore';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SearchBarComponent } from './search-bar/search-bar.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { CharacterSheetCardComponent } from './character-sheet-card/character-sheet-card.component';
import { SigninComponent } from './signin/signin/signin.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CharacterSheetFormComponent } from './character-sheet-form/character-sheet-form.component';
import { Location } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { LayoutModule } from '@angular/cdk/layout';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatDialogModule } from '@angular/material/dialog';
import { EmailDialogComponent } from './signin/email-dialog/email-dialog/email-dialog.component';
import { CloseDialogComponent } from './signin/close-dialog/close-dialog/close-dialog.component';
import { StatsDialogComponent } from './stats-dialog/stats-dialog/stats-dialog.component';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    SearchBarComponent,
    CharacterSheetCardComponent,
    CharacterSheetFormComponent,
    ToolbarComponent,
    SigninComponent,
    EmailDialogComponent,
    CloseDialogComponent,
    StatsDialogComponent,
  ],
  imports: [
    BrowserModule,
    provideFirebaseApp(() => initializeApp(environment.firebase)),
    provideAuth(() => getAuth()),
    provideFirestore(() => {
      const firestore = getFirestore();
      if (environment.useEmulators) {
        connectFirestoreEmulator(firestore, 'localhost', 8080);
      }
      return firestore;
    }),
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    LayoutModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatFormFieldModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatMenuModule,
    MatSelectModule,
    MatToolbarModule,
    MatDialogModule,
    ReactiveFormsModule,
  ],
  providers: [
    HttpClient
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
