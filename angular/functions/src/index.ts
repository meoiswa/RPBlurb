import { GoogleAuth } from 'google-auth-library';
import { google } from 'googleapis';

import * as functions from "firebase-functions";
import * as admin from "firebase-admin";

import { liveWorlds } from './live-worlds';

const billing = google.cloudbilling('v1').projects
const PROJECT_ID = process.env.GCLOUD_PROJECT
const PROJECT_NAME = `projects/${PROJECT_ID}`

admin.initializeApp();
const db = admin.firestore();

const validateParams = (world: string, user: string, res: functions.Response) => {
  if (!world || !user) {
    functions.logger.debug('User and World are required fields', world, user);
    res.status(400).send({ error: 'User and World are required fields' });
    return false;
  }

  return true;
};

const validateWorldExists = async (world: string, res: functions.Response) => {
  if (!liveWorlds.includes(world)) {
    functions.logger.debug('World does not exist', world);
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
    if (!str) return str;
    return str.length > length ? str.substring(0, length) : str;
  }

  functions.logger.debug('Invoke setCharacter ', req.body);
  const body = JSON.parse(req.body);
  const world = body.World;
  const user = trimLength(body.User, 256);
  const name = trimLength(body.Name, 256);
  const nameStyle = trimLength(body.NameStyle, 256);
  const title = trimLength(body.Title, 256);
  const description = trimLength(body.Description, 512);
  const alignment = trimLength(body.Alignment, 256);
  const status = trimLength(body.Status, 256);
  functions.logger.debug('Parsed values:', world, user, name, nameStyle, title, alignment, status, description);


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
  functions.logger.debug('Invoke getCharacter ', req.body, world, user);

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
  functions.logger.debug('Invoke updateStats');
  const worlds = await db.collection('rp').listDocuments();
  let total = 0;
  const stats = {} as any;
  for (const world of worlds) {
    const characters = await world.collection('characters').listDocuments();
    const count = characters.length;
    functions.logger.debug('World', world.id, 'has', count, 'characters');
    await world.set({ Characters: count }, { merge: true });
    total += count;
    stats[world.id] = count;
  }
  stats['total'] = total;
  db.collection('stats').doc('stats').set(stats, { merge: true });
  res.status(200).send(stats);
});

const _setAuthCredentials = () => {
  const client = new GoogleAuth({
    scopes: ['https://www.googleapis.com/auth/cloud-billing', 'https://www.googleapis.com/auth/cloud-platform'],
  })

  google.options({ auth: client });
}

const disableBilling = async () => {
  _setAuthCredentials()

  if (PROJECT_NAME) {
      const billingInfo = await billing.getBillingInfo({ name: PROJECT_NAME })
      if (billingInfo.data.billingEnabled) {
          try {
              await billing.updateBillingInfo({
                  name: PROJECT_NAME,
                  requestBody: { billingAccountName: '' },
              })
              functions.logger.info(`✂️ ${PROJECT_NAME} billing account has been removed`)
          } catch (error) {
              functions.logger.error(error)
          }
      } else {
          console.log('👉 looks like you already disabled billing')
      }
  }
}

export const billingMonitor = functions.pubsub.topic('billing').onPublish(async (message) => {
  const pubsubData = JSON.parse(Buffer.from(message.data, 'base64').toString())
  const { costAmount, budgetAmount, currencyCode } = pubsubData

  functions.logger.info(`Project current cost is: ${costAmount}${currencyCode} out of ${budgetAmount}${currencyCode}`)
  if (budgetAmount < costAmount) await disableBilling()

  return null // returns nothing
});
