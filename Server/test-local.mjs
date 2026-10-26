import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { mkdtempSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { randomUUID } from 'node:crypto';
import { setTimeout as delay } from 'node:timers/promises';

const dir=mkdtempSync(resolve('.tools/server/test-'));
const clock=resolve(dir,'clock.txt');writeFileSync(clock,'1800000000');
const port=18787,base='http://127.0.0.1:'+port;
let child;
function launch(){
  child=spawn(process.execPath,['Server/server.mjs','--port='+port,'--data='+resolve(dir,'farm.sqlite'),'--test-clock='+clock],{windowsHide:true,stdio:['ignore','pipe','pipe']});
  child.stderr.on('data',chunk=>process.stderr.write(chunk));
}
async function ready(){
  for(let i=0;i<80;i++){try{const response=await fetch(base+'/health');if(response.ok)return;}catch{}await delay(100);}
  throw new Error('Server did not become ready');
}
async function api(path,body,token){
  const response=await fetch(base+path,{method:body===undefined?'GET':'POST',headers:{'Content-Type':'application/json',...(token?{Authorization:'Bearer '+token}:{})},body:body===undefined?undefined:JSON.stringify(body)});
  return {status:response.status,...await response.json()};
}
async function command(token,kind,args={},requestId=randomUUID()){return api('/api/command',{requestId,kind,...args},token);}
let count=0;
const check=(condition,message)=>{assert.ok(condition,message);count++;};
try{
  launch();await ready();
  check((await api('/api/state')).status===401,'Anonymous state rejected');
  const alice=await api('/api/register',{name:'alice',password:'LocalTest123!'});
  const bob=await api('/api/register',{name:'bob',password:'LocalTest456!'});
  check(alice.success&&bob.success&&alice.farm.coins===120,'Create independent accounts');
  check((await api('/api/login',{name:'alice',password:'wrong-password'})).status===401,'Wrong password rejected');
  check((await api('/api/visit/'+alice.userId,undefined,bob.token)).status===404,'Cross-island visit rejected');
  check((await api('/api/join',{code:alice.islandCode},bob.token)).success,'Join invite island');
  check((await api('/api/island',undefined,bob.token)).members.length===2,'Shared island has two members');
  const requestId=randomUUID();
  const plant=await command(alice.token,'plot',{index:0,tool:0,id:'carrot',farm:{coins:999999},now:9999999999},requestId);
  if(!plant.success)console.error('Plant failed:',plant.status,plant.message);
  check(plant.success&&plant.farm.coins===116&&plant.farm.plots[0].plantedAt===1800000000,'Server owns money and clock');
  const replay=await command(alice.token,'plot',{index:0,tool:0,id:'carrot',farm:{coins:999999},now:9999999999},requestId);
  check(replay.replayed&&replay.farm.coins===116,'Retry cannot charge twice');
  check((await command(alice.token,'sell',{},requestId)).status===409,'Request id conflict rejected');
  const pair=await Promise.all([command(alice.token,'plot',{index:1,tool:0,id:'carrot'}),command(alice.token,'plot',{index:1,tool:0,id:'carrot'})]);
  check(pair.filter(r=>r.success).length===1,'Concurrent planting serialized');
  check((await command(alice.token,'harvest',{index:0,score:3})).success===false,'Premature harvest rejected');
  check((await command(bob.token,'help',{target:alice.userId,index:0,tool:0,id:'corn'})).status===403,'Friend cannot plant on owner land');
  const helped=await command(bob.token,'help',{target:alice.userId,index:0,tool:1});
  check(helped.success&&helped.farm.helped===2,'Water help grants social points');
  check((await command(bob.token,'help',{target:alice.userId,index:0,tool:1})).success===false,'Repeated help gives no reward');
  const visit=await api('/api/visit/'+alice.userId,undefined,bob.token);
  check(visit.farm.plots[0].watered===true,'Owner farm reflects help');
  writeFileSync(clock,'1800000060');
  const harvest=await command(alice.token,'harvest',{index:0,score:999});
  check(harvest.success&&harvest.farm.inventory[0].count===3,'Harvest server standard score ignores forged score');
  check((await command(alice.token,'harvest',{index:0})).success===false,'Duplicate harvest rejected');
  check((await command(alice.token,'order')).success,'Deliver order on server');
  const day=await command(alice.token,'daily');
  check(day.success&&(await command(alice.token,'daily')).success===false,'Daily gift only once');
  check((await command(alice.token,'setCoins',{coins:999999})).status===400,'Unknown mutation denied');
  check((await command(alice.token,'build',{id:'mill'})).success===false,'Building level gate enforced');
  for(let i=2;i<6;i++)await command(alice.token,'plot',{index:i,tool:0,id:'carrot'});
  writeFileSync(clock,'1800000120');
  for(let i=1;i<6;i++)await command(alice.token,'harvest',{index:i});
  const built=await command(alice.token,'build',{id:'mill'});
  check(built.success&&built.farm.development.buildings.some(b=>b.id==='mill'&&b.level===1),'Server building purchase after earned level');
  await command(alice.token,'plot',{index:7,tool:0,id:'wheat'});writeFileSync(clock,'1800000600');
  await command(alice.token,'harvest',{index:7});
  const flour=await command(alice.token,'produce',{id:'flour'});
  check(flour.success&&flour.farm.development.jobs.length===1,'Server production consumes harvested input');
  const job=flour.farm.development.jobs[0].id;
  check((await command(alice.token,'collect',{job})).success===false,'No early production collection');
  writeFileSync(clock,'1800000700');
  check((await command(alice.token,'collect',{job})).success&&(await command(alice.token,'collect',{job})).success===false,'Production completion is single claim');
  check((await command(alice.token,'sellProduct',{id:'flour'})).success,'Sell crafted product');
  check((await command(alice.token,'buyAnimal',{id:'chicken'})).success,'Buy server livestock');
  check((await command(alice.token,'feedAnimal',{id:'chicken'})).success,'Feed server livestock');
  writeFileSync(clock,'1800000800');
  check((await command(alice.token,'collectAnimal',{id:'chicken'})).success,'Collect server livestock');
  const stockBefore=await api('/api/state',undefined,alice.token);
  const stockRequest=randomUUID();
  const selective=await command(alice.token,'sellStock',{id:'eggs',tool:1,job:1},stockRequest);
  check(selective.success&&selective.farm.eggs===stockBefore.farm.eggs-1,'Selective sale deducts exact server quantity');
  const selectiveReplay=await command(alice.token,'sellStock',{id:'eggs',tool:1,job:1},stockRequest);
  check(selectiveReplay.replayed&&selectiveReplay.farm.eggs===selective.farm.eggs,'Selective sale retry is idempotent');
  check(!(await command(alice.token,'sellStock',{id:'eggs',tool:1,job:-1})).success,'Selective sale rejects negative quantity');
  check(!(await command(alice.token,'choiceOrder',{index:1,job:-1})).success,'Choice order rejects stale serial');
  const state=await api('/api/state',undefined,alice.token);
  const old=child;old.kill();await new Promise(resolveExit=>old.once('exit',resolveExit));await delay(150);
  launch();await ready();
  const login=await api('/api/login',{name:'alice',password:'LocalTest123!'});
  check(login.success&&login.farm.coins===state.farm.coins&&login.farm.development.ordersCompleted===1,'SQLite survives server restart');
  check((await api('/api/logout',{},login.token)).success&&(await api('/api/state',undefined,login.token)).status===401,'Logout revokes session');
  writeFileSync(resolve(dir,'result.json'),JSON.stringify({status:'complete',checks:count}));
  console.log('PASS '+count+' local-server checks: auth, ownership, invitations, help, concurrency, retries, server time, economy, persistence.');
}finally{if(child&&!child.killed)child.kill();}
