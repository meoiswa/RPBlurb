import { liveWorlds } from './live-worlds';

import * as functions from "firebase-functions";

import * as admin from "firebase-admin";
admin.initializeApp();

const db = admin.firestore();

const validateParams = (world: string, user: string, res: functions.Response) => {
  if (!world || !user) {
    console.log('User and World are required fields', world, user);
    res.status(400).send({ error: 'User and World are required fields' });
    return false;
  }

  return true;
};

const validateWorldExists = async (world: string, res: functions.Response) => {
  const liveWorldsRef = db.collection('LiveWorlds');

  const snapshot = await liveWorldsRef.get();

  if (snapshot.empty) {
    for (const world of liveWorlds) {
      liveWorldsRef.doc(world).set({ enabled: true });
    }
  }

  const liveWorld = await liveWorldsRef.doc(world).get();
  if (!liveWorld.exists) {
    console.log('World does not exist', world);
    res.status(400).send({ error: 'World does not exist' });
    return false;
  }

  return true;
};

export const setCharacter = functions.https.onRequest(async (req, res) => {

  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Credentials", "true");
  res.setHeader("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
  res.setHeader("Access-Control-Allow-Headers", "Access-Control-Allow-Headers, Origin,Accept, X-Requested-With, Content-Type, Access-Control-Request-Method, Access-Control-Request-Headers");

  if (req.method === "OPTIONS") {
    res.status(204).send('');
    return;
  }

  console.log('Invoke setCharacter ', req.body);
  const body = JSON.parse(req.body);
  const world = body.World;
  const user = body.User;
  const name = body.Name;
  const nameStyle = body.NameStyle;
  const title = body.Title;
  const description = body.Description;
  const alignment = body.Alignment;
  const status = body.Status;
  console.log('Parsed values:', world, user, name, nameStyle, title, alignment, status, description);

  
  if (!validateParams(world, user, res)) return;
  if (!(await validateWorldExists(world, res))) return;

  const characterRef = db.collection('rp').doc(world).collection('characters').doc(user);
  await characterRef.set({
    User: user,
    World: world,
    Name: name,
    NameStyle: nameStyle,
    Title: title,
    Description: description,
    Status: status,
    Alignment: alignment
  });

  res.status(200).send({ message: 'Character data set successfully' });
});

export const getCharacter = functions.https.onRequest(async (req, res) => {
  const body = JSON.parse(req.body);
  const world = body.World;
  const user = body.User;
  console.log('Invoke getCharacter ', req.body, world, user);

  if (!validateParams(world, user, res)) return;
  if (!(await validateWorldExists(world, res))) return;

  const characterRef = db.collection('rp').doc(world).collection('characters').doc(user);
  const character = await characterRef.get();
  if (!character.exists) {
    res.status(404).send({ error: 'Character does not exist' });
    return;
  }

  res.status(200).send(character.data());
});
