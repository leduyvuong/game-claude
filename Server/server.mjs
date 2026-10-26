import http from 'node:http';
import { DatabaseSync } from 'node:sqlite';
import { spawn } from 'node:child_process';
import { randomBytes, randomUUID, scryptSync, timingSafeEqual, createHash } from 'node:crypto';
import { mkdirSync, readFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const root=resolve(dirname(fileURLToPath(import.meta.url)),'..');
const arg=(key,fallback)=>process.argv.find(a=>a.startsWith('--'+key+'='))?.split('=').slice(1).join('=')??fallback;
const port=Number(arg('port','8787'));
if(!Number.isInteger(port)||port<1024||port>65535)throw new Error('Invalid port');
const dataFile=resolve(arg('data',resolve(root,'Server/.data/harvest.sqlite')));
mkdirSync(dirname(dataFile),{recursive:true});
const database=new DatabaseSync(dataFile);
database.exec(`
PRAGMA journal_mode=WAL;
PRAGMA foreign_keys=ON;
CREATE TABLE IF NOT EXISTS islands (id TEXT PRIMARY KEY, code TEXT UNIQUE NOT NULL);
CREATE TABLE IF NOT EXISTS users (id TEXT PRIMARY KEY, name TEXT UNIQUE NOT NULL, salt TEXT NOT NULL, password TEXT NOT NULL, farm TEXT NOT NULL, island TEXT NOT NULL REFERENCES islands(id), help_day INTEGER NOT NULL DEFAULT 0, help_count INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS sessions (token TEXT PRIMARY KEY, user_id TEXT NOT NULL REFERENCES users(id), expires REAL NOT NULL);
CREATE TABLE IF NOT EXISTS receipts (user_id TEXT NOT NULL REFERENCES users(id), request_id TEXT NOT NULL, fingerprint TEXT NOT NULL, result TEXT NOT NULL, PRIMARY KEY(user_id,request_id));
`);
const stmt=(sql)=>database.prepare(sql);
const now=()=>arg('test-clock','')?Number(readFileSync(resolve(arg('test-clock','')),'utf8')):Date.now()/1000;
const digest=value=>createHash('sha256').update(value).digest('hex');
const canonical=value=>value&&typeof value==='object'&&!Array.isArray(value)?Object.fromEntries(Object.keys(value).sort().map(k=>[k,canonical(value[k])])):Array.isArray(value)?value.map(canonical):value;
class ApiError extends Error {constructor(status,message){super(message);this.status=status;}}
function requireValue(value,status,message){if(!value)throw new ApiError(status,message);}
function worker(farm,command={kind:'state'},elapsed=0){
  return new Promise((resolvePromise,reject)=>{
    const child=spawn(resolve(root,'.tools/server/CoreWorker.exe'),[],{windowsHide:true,stdio:['pipe','pipe','pipe']});
    let output='',error='';const timer=setTimeout(()=>{child.kill();reject(new Error('Core worker timeout'));},10000);
    child.stdout.setEncoding('utf8');child.stderr.setEncoding('utf8');
    child.stdout.on('data',chunk=>output+=chunk);child.stderr.on('data',chunk=>error+=chunk);
    child.on('error',e=>{clearTimeout(timer);reject(e);});
    child.on('close',code=>{clearTimeout(timer);if(code!==0){reject(new Error('Core rejected state: '+error));return;}try{resolvePromise(JSON.parse(output.replace(/^\uFEFF/,'')));}catch(e){reject(e);}});
    child.stdin.on('error',()=>{});
    child.stdin.end(JSON.stringify({farm,...command,now:now(),elapsed}));
  });
}
function transaction(action){database.exec('BEGIN IMMEDIATE');try{const result=action();database.exec('COMMIT');return result;}catch(e){database.exec('ROLLBACK');throw e;}}
function session(userId){
  const token=randomBytes(32).toString('hex');
  stmt('INSERT INTO sessions VALUES (?,?,?)').run(digest(token),userId,now()+7*86400);
  return token;
}
function authenticate(request){
  const raw=request.headers.authorization??'';
  requireValue(raw.startsWith('Bearer ')&&raw.length<200,401,'Cần đăng nhập.');
  const s=stmt('SELECT user_id FROM sessions WHERE token=? AND expires>?').get(digest(raw.slice(7)),now());
  requireValue(s,401,'Phiên đăng nhập đã hết hạn.');
  return stmt('SELECT * FROM users WHERE id=?').get(s.user_id);
}
function envelope(user,result){
  const island=stmt('SELECT code FROM islands WHERE id=?').get(user.island);
  return {...result,userId:user.id,name:user.name,islandCode:island.code};
}
async function body(request){
  let bytes=0,parts=[];
  for await(const chunk of request){bytes+=chunk.length;requireValue(bytes<=65536,413,'Yêu cầu quá lớn.');parts.push(chunk);}
  if(!bytes)return {};
  let value;try{value=JSON.parse(Buffer.concat(parts).toString('utf8'));}catch{throw new ApiError(400,'JSON không hợp lệ.');}
  requireValue(value&&typeof value==='object'&&!Array.isArray(value),400,'Yêu cầu không hợp lệ.');
  return value;
}
const authAttempts=new Map();
function authLimit(request){
  const key=request.socket.remoteAddress,minute=Math.floor(Date.now()/60000);
  const previous=authAttempts.get(key);const entry=previous?.minute===minute?previous:{minute,count:0};
  requireValue(++entry.count<=30,429,'Thử đăng nhập quá nhiều. Chờ một phút.');authAttempts.set(key,entry);
}
async function route(request,payload){
  const host=request.headers.host??'';
  requireValue(host==='127.0.0.1:'+port||host==='localhost:'+port,400,'Host không được phép.');
  const origin=request.headers.origin;
  requireValue(!origin||origin==='http://'+host,403,'Origin không được phép.');
  const path=new URL(request.url,'http://127.0.0.1').pathname,method=request.method;
  if(path==='/health'&&method==='GET')return {success:true,service:'Harvest Isle local server'};
  if((path==='/api/register'||path==='/api/login')&&method==='POST'){
    authLimit(request);
    requireValue(typeof payload.name==='string'&&/^[a-zA-Z0-9_]{3,24}$/.test(payload.name)&&typeof payload.password==='string'&&payload.password.length>=8&&payload.password.length<=128,400,'Tên 3–24 chữ/số/gạch dưới, mật khẩu 8–128 ký tự.');
    const name=payload.name.toLowerCase();let user=stmt('SELECT * FROM users WHERE name=?').get(name);
    if(path==='/api/register'){
      requireValue(!user,409,'Tên đã được sử dụng.');
      const result=await worker(null,{kind:'new'});
      const salt=randomBytes(16).toString('hex'),hash=scryptSync(payload.password,salt,32).toString('hex');
      const id=randomUUID(),island=randomUUID(),code=randomBytes(6).toString('hex').toUpperCase();
      transaction(()=>{stmt('INSERT INTO islands VALUES (?,?)').run(island,code);stmt('INSERT INTO users (id,name,salt,password,farm,island) VALUES (?,?,?,?,?,?)').run(id,name,salt,hash,JSON.stringify(result.farm),island);});
      user=stmt('SELECT * FROM users WHERE id=?').get(id);
    }else{
      const derived=scryptSync(payload.password,user?.salt??'not-a-real-salt',32);
      requireValue(user&&timingSafeEqual(derived,Buffer.from(user.password,'hex')),401,'Sai tên hoặc mật khẩu.');
    }
    return envelope(user,{success:true,message:'Đã đăng nhập máy chủ local.',token:session(user.id),farm:JSON.parse(user.farm),serverTime:now()});
  }
  const user=authenticate(request);
  if(path==='/api/logout'&&method==='POST'){
    stmt('DELETE FROM sessions WHERE token=?').run(digest(request.headers.authorization.slice(7)));return {success:true,message:'Đã đăng xuất.'};
  }
  if(path==='/api/island'&&method==='GET'){
    const members=stmt('SELECT id,name FROM users WHERE island=? ORDER BY name').all(user.island);
    return envelope(user,{success:true,members,serverTime:now()});
  }
  if(path==='/api/join'&&method==='POST'){
    requireValue(typeof payload.code==='string'&&payload.code.length===12,400,'Mã đảo không hợp lệ.');
    const island=stmt('SELECT * FROM islands WHERE code=?').get(payload.code.toUpperCase());
    requireValue(island,404,'Không tìm thấy đảo.');
    if(island.id!==user.island){
      requireValue(stmt('SELECT COUNT(*) AS count FROM users WHERE island=?').get(island.id).count<10,409,'Đảo đã đủ 10 người.');
      stmt('UPDATE users SET island=? WHERE id=?').run(island.id,user.id);user.island=island.id;
    }
    return envelope(user,{success:true,message:'Đã vào đảo chung.',farm:JSON.parse(user.farm),serverTime:now()});
  }
  if(path.startsWith('/api/visit/')&&method==='GET'){
    const target=stmt('SELECT id,name,farm,island FROM users WHERE id=? AND island=?').get(path.slice('/api/visit/'.length),user.island);
    requireValue(target,404,'Nông trại không thuộc đảo của bạn.');
    const result=await worker(JSON.parse(target.farm));stmt('UPDATE users SET farm=? WHERE id=?').run(JSON.stringify(result.farm),target.id);
    return {success:true,name:target.name,userId:target.id,farm:result.farm,serverTime:now()};
  }
  if(path==='/api/state'&&method==='GET'){
    const farm=JSON.parse(user.farm),elapsed=now()-farm.lastSeenUtc;
    const result=await worker(farm,{kind:'state'},elapsed<10?elapsed:0);
    stmt('UPDATE users SET farm=? WHERE id=?').run(JSON.stringify(result.farm),user.id);
    return envelope(user,result);
  }
  if(path==='/api/command'&&method==='POST'){
    requireValue(typeof payload.requestId==='string'&&/^[a-zA-Z0-9-]{16,80}$/.test(payload.requestId),400,'Cần mã yêu cầu duy nhất.');
    const fingerprint=digest(JSON.stringify(canonical(payload)));
    const receipt=stmt('SELECT * FROM receipts WHERE user_id=? AND request_id=?').get(user.id,payload.requestId);
    if(receipt){requireValue(receipt.fingerprint===fingerprint,409,'Mã yêu cầu đã dùng cho thao tác khác.');return envelope(user,{...JSON.parse(receipt.result),farm:JSON.parse(user.farm),serverTime:now(),replayed:true});}
    requireValue(stmt('SELECT COUNT(*) AS count FROM receipts WHERE user_id=?').get(user.id).count<10000,429,'Phiên thử nghiệm đã đạt giới hạn thao tác.');
    const permitted=['plot','harvest','sell','build','produce','collect','sellProduct','sellStock','choiceOrder','order','daily','chapter','nursery','greenhouse','careProduct','buyAnimal','feedAnimal','collectAnimal','help'];
    requireValue(permitted.includes(payload.kind),400,'Lệnh không được phép.');
    requireValue(Number.isInteger(payload.index??0)&&Number.isInteger(payload.tool??0)&&Number.isInteger(payload.job??0),400,'Tham số số nguyên không hợp lệ.');
    requireValue(payload.id===undefined||typeof payload.id==='string'&&payload.id.length<=64,400,'Mã vật phẩm không hợp lệ.');
    const command={kind:payload.kind,id:payload.id??'',index:payload.index??0,tool:payload.tool??0,job:payload.job??0,useCorn:payload.useCorn===true};
    let result,target=null,helperFarm=null,day=Math.floor(now()/86400),helpCount=user.help_day===day?user.help_count:0;
    if(payload.kind==='help'){
      requireValue(payload.target!==user.id&&[1,4].includes(payload.tool),403,'Chỉ được tưới hoặc thoát nước giúp bạn.');
      target=stmt('SELECT id,farm FROM users WHERE id=? AND island=?').get(payload.target??'',user.island);
      requireValue(target,404,'Bạn cùng đảo không tồn tại.');requireValue(helpCount<10,429,'Đã giúp đủ 10 lượt hôm nay.');
      result=await worker(JSON.parse(target.farm),{...command,kind:'plot'});
      helperFarm=JSON.parse(user.farm);if(result.success)helperFarm.helped=Math.min(1000000000,helperFarm.helped+(payload.tool===4?4:2));
    }else{
      const farm=JSON.parse(user.farm),elapsed=now()-farm.lastSeenUtc;
      result=await worker(farm,command,elapsed<10?elapsed:0);
    }
    transaction(()=>{
      if(target){
        stmt('UPDATE users SET farm=? WHERE id=?').run(JSON.stringify(result.farm),target.id);
        stmt('UPDATE users SET farm=?,help_day=?,help_count=? WHERE id=?').run(JSON.stringify(helperFarm),day,helpCount+(result.success?1:0),user.id);
      }else stmt('UPDATE users SET farm=? WHERE id=?').run(JSON.stringify(result.farm),user.id);
      stmt('INSERT INTO receipts VALUES (?,?,?,?)').run(user.id,payload.requestId,fingerprint,JSON.stringify({success:result.success,message:result.message}));
    });
    return envelope(user,{...result,farm:helperFarm??result.farm});
  }
  throw new ApiError(404,'Không tìm thấy đường dẫn.');
}
let queue=Promise.resolve(),queued=0;
const server=http.createServer(async(request,response)=>{
  try{
    requireValue(queued<64,503,'Máy chủ đang bận.');
    const payload=await body(request);queued++;
    const task=queue.then(()=>route(request,payload));queue=task.catch(()=>{});let result;
    try{result=await task;}finally{queued--;}
    response.writeHead(200,{'Content-Type':'application/json; charset=utf-8','Cache-Control':'no-store'});response.end(JSON.stringify(result));
  }catch(error){
    if(!error.status)console.error('Request failed:',error.message);
    response.writeHead(error.status??500,{'Content-Type':'application/json; charset=utf-8','Cache-Control':'no-store'});
    response.end(JSON.stringify({success:false,message:error.status?error.message:'Máy chủ chưa xử lý được yêu cầu.'}));
  }
});
server.requestTimeout=10000;server.headersTimeout=10000;server.keepAliveTimeout=1000;
server.listen(port,'127.0.0.1',()=>console.log('Harvest Isle local server http://127.0.0.1:'+port));
process.on('SIGTERM',()=>server.close(()=>{database.close();process.exit(0);}));
