import * as functions from "firebase-functions";

import * as admin from "firebase-admin";
admin.initializeApp();

const db = admin.firestore();

const validateParams = (world: string, name: string, res: functions.Response) => {
  if (!world || !name) {
    console.log('Name and World are required fields', world, name);
    res.status(400).send({ error: 'Name and World are required fields' });
    return false;
  }

  return true;
};

const validateWorldExists = async (world: string, res: functions.Response) => {
  const liveWorldsRef = db.collection('LiveWorlds');
  const liveWorld = await liveWorldsRef.doc(world).get();
  if (!liveWorld.exists) {
    console.log('World does not exist', world);
    res.status(400).send({ error: 'World does not exist' });
    return false;
  }

  return true;
};

export const setCharacter = functions.https.onRequest(async (req, res) => {
  const body = JSON.parse(req.body);
  const world = body.world;
  const name = body.name;
  const description = body.description;
  const alignment = body.alignment;
  const status = body.status;
  console.log('Invoke setCharacter ', world, name, description, alignment, status);

  if (!validateParams(world, name, res)) return;
  if (!(await validateWorldExists(world, res))) return;

  const characterRef = db.collection('rp').doc(world).collection('characters').doc(name);
  await characterRef.set({
    Name: name,
    World: world,
    Description: description,
    Status: status,
    Alignment: alignment
  });

  res.status(200).send({ message: 'Character data set successfully' });
});

export const getCharacter = functions.https.onRequest(async (req, res) => {
  const body = JSON.parse(req.body);
  const world = body.world;
  const name = body.name;
  console.log('Invoke getCharacter ', req.body, world, name);

  if (!validateParams(world, name, res)) return;
  if (!(await validateWorldExists(world, res))) return;

  const characterRef = db.collection('rp').doc(world).collection('characters').doc(name);
  const character = await characterRef.get();
  if (!character.exists) {
    res.status(404).send({ error: 'Character does not exist' });
    return;
  }

  res.status(200).send(character.data());
});
