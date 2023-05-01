import { liveWorlds } from './live-worlds';

import * as functions from "firebase-functions";

import * as admin from "firebase-admin";
admin.initializeApp();

const db = admin.firestore();

const validateParams = (world: string, user: string, res: functions.Response) => {
  if (!world || !user) {
    console.debug('User and World are required fields', world, user);
    res.status(400).send({ error: 'User and World are required fields' });
    return false;
  }

  return true;
};

const validateWorldExists = async (world: string, res: functions.Response) => {
  if (!liveWorlds.includes(world)) {
    console.debug('World does not exist', world);
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

  const trimLength = (str: string, length: number): string => {
    return str.length > length ? str.substring(0, length) : str;
  }

  console.debug('Invoke setCharacter ', req.body);
  const body = JSON.parse(req.body);
  const world = body.World;
  const user = trimLength(body.User, 256);
  const name = trimLength(body.Name, 256);
  const nameStyle = trimLength(body.NameStyle, 256);
  const title = trimLength(body.Title, 256);
  const description = trimLength(body.Description, 512);
  const alignment = trimLength(body.Alignment, 256);
  const status = trimLength(body.Status, 256);
  console.debug('Parsed values:', world, user, name, nameStyle, title, alignment, status, description);


  if (!validateParams(world, user, res)) return;
  if (!(await validateWorldExists(world, res))) return;

  const characterRef = db.collection('rp').doc(world).collection('characters').doc(user);
  const snapshot = await characterRef.get();
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
  if (!snapshot.exists) {
    db.collection('stats').doc('stats').update({ total: admin.firestore.FieldValue.increment(1) });
  }
  res.status(200).send({ message: 'Character data set successfully' });
});

export const getCharacter = functions.https.onRequest(async (req, res) => {
  const body = JSON.parse(req.body);
  const world = body.World;
  const user = body.User;
  console.debug('Invoke getCharacter ', req.body, world, user);

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

export const updateStats = functions.https.onRequest(async (req, res) => {
  console.log('Invoke updateStats');
  const worlds = await db.collection('rp').listDocuments();
  let total = 0;
  const stats = {} as any;
  for (const world of worlds) {
    const characters = await world.collection('characters').listDocuments();
    const count = characters.length;
    console.log('World', world.id, 'has', count, 'characters');
    await world.set({ Characters: count }, { merge: true });
    total += count;
    stats[world.id] = count;
  }
  stats['total'] = total;
  db.collection('stats').doc('stats').set(stats, { merge: true });
  res.status(200).send(stats);
});
