import { DocumentData, QueryDocumentSnapshot, SnapshotOptions } from "@angular/fire/firestore";

export interface CharacterSheet {
  user: string;
  world: string;
  name: string;
  nameStyle: number;
  title: string;
  alignment: string;
  status: string;
  description: string;
  exists: boolean;
}

export const toCharacterSheet = (snapshot: QueryDocumentSnapshot<DocumentData>, options?: SnapshotOptions) => {
  const data = snapshot.data(options);
  return {
    world: data['World'],
    user: data['User'],
    name: data['Name'],
    nameStyle: data['NameStyle'],
    title: data['Title'],
    alignment: data['Alignment'],
    status: data['Status'],
    description: data['Description'],
    exists: true,
  } as CharacterSheet;
}

export const fromCharacterSheet = (item: CharacterSheet) => {
  return {
    World: item.world,
    User: item.user,
    Name: item.name,
    NameStyle: item.nameStyle,
    Title: item.title,
    Alignment: item.alignment,
    Status: item.status,
    Description: item.description,
  }
}
