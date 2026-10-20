using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
namespace XHLLauncher {
 internal sealed class PhoneView : IDisposable {
  TcpListener listener; Thread thread; volatile bool active; readonly object gate=new object(); string json="{}";
  readonly string token; readonly string uiFile; internal string Url {get;private set;}
  readonly Func<string, ActionResult> command;
  internal PhoneView(string key, Func<string, ActionResult> handler=null, string htmlFile=null) {token=key;command=handler;uiFile=htmlFile??Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"phone","phone-ui.html");}
  internal static IPAddress LocalAddress() {
   foreach(var n in NetworkInterface.GetAllNetworkInterfaces()) {if(n.OperationalStatus!=OperationalStatus.Up || n.NetworkInterfaceType==NetworkInterfaceType.Loopback)continue; var props=n.GetIPProperties(); if(props.GatewayAddresses.Count==0)continue; foreach(var a in props.UnicastAddresses) {var b=a.Address.GetAddressBytes();if(b.Length==4 && (b[0]==10 || (b[0]==192 && b[1]==168) || (b[0]==172 && b[1]>=16 && b[1]<=31)))return a.Address;}}
   throw new IOException("No private home-network IPv4 address found.");
  }
  internal void Start(IPAddress address,int port) {listener=new TcpListener(address,port);listener.Start(8);active=true;Url="http://"+address+":"+((IPEndPoint)listener.LocalEndpoint).Port+"/"+token+"/";thread=new Thread(Serve){IsBackground=true};thread.Start();}
  internal void Publish(object data){lock(gate)json=new JavaScriptSerializer().Serialize(data);}
  readonly System.Collections.Generic.HashSet<TcpClient> clients=new System.Collections.Generic.HashSet<TcpClient>();
  int workers;
  void Serve(){while(active)try{
   var client=listener.AcceptTcpClient();
   if(Interlocked.Increment(ref workers)>8){Interlocked.Decrement(ref workers);client.Close();continue;}
   lock(clients){if(!active){client.Close();Interlocked.Decrement(ref workers);continue;}clients.Add(client);}
   ThreadPool.QueueUserWorkItem(delegate{try{using(client)Handle(client);}catch{}finally{lock(clients)clients.Remove(client);Interlocked.Decrement(ref workers);}});
  }catch{if(!active)return;}}
  static void Reply(NetworkStream stream,string status,string type,string body){
   byte[] payload=Encoding.UTF8.GetBytes(body);
   string policy="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'; frame-src https://enshroudeddatabase.com https://enshrouded.gamevault.in; object-src 'none'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
   byte[] headers=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+"\r\nContent-Type: "+type+"\r\nContent-Length: "+payload.Length+"\r\nCache-Control: no-store\r\nReferrer-Policy: no-referrer\r\nX-Content-Type-Options: nosniff\r\nX-Frame-Options: DENY\r\nContent-Security-Policy: "+policy+"\r\nConnection: close\r\n\r\n");
   stream.Write(headers,0,headers.Length);stream.Write(payload,0,payload.Length);
  }
  void Handle(TcpClient client){
   client.ReceiveTimeout=1000;client.SendTimeout=2000;
   using(var stream=client.GetStream()){
    var bytes=new MemoryStream();bool complete=false;DateTime deadline=DateTime.UtcNow.AddSeconds(2);
    while(bytes.Length<8192 && DateTime.UtcNow<deadline){int v=stream.ReadByte();if(v<0)break;bytes.WriteByte((byte)v);var a=bytes.GetBuffer();int n=(int)bytes.Length;if(n>=4&&a[n-4]==13&&a[n-3]==10&&a[n-2]==13&&a[n-1]==10){complete=true;break;}}
    if(!complete){Reply(stream,"431 Request Header Fields Too Large","text/plain","Incomplete or oversized request");return;}
    string req=Encoding.ASCII.GetString(bytes.ToArray());string[] lines=req.Split(new[]{"\r\n"},StringSplitOptions.None),line=lines[0].Split(' ');
    if(line.Length!=3 || (line[2]!="HTTP/1.1" && line[2]!="HTTP/1.0")){Reply(stream,"400 Bad Request","text/plain","Bad request");return;}
    string path=line[1],prefix="/"+token+"/";
    if(line[0]=="GET" && (path==prefix||path==prefix+"status")){
     if(path.EndsWith("/status",StringComparison.Ordinal)){string body;lock(gate)body=json;Reply(stream,"200 OK","application/json",body);}
     else Reply(stream,"200 OK","text/html; charset=utf-8",PhonePage());
     return;
    }
    if(line[0]=="POST"&&path.StartsWith(prefix+"command/",StringComparison.Ordinal)){
     bool authorized=false,originAllowed=true;int keyCount=0,originCount=0;
     foreach(string header in lines){int colon=header.IndexOf(':');if(colon<1)continue;string name=header.Substring(0,colon).Trim(),value=header.Substring(colon+1).Trim();
      if(name.Equals("X-XHL-Key",StringComparison.OrdinalIgnoreCase)){keyCount++;authorized=value==token;}
      if(name.Equals("Origin",StringComparison.OrdinalIgnoreCase)){originCount++;originAllowed=value==new Uri(Url).GetLeftPart(UriPartial.Authority);}
     }
     string op=path.Substring((prefix+"command/").Length);
     if(!authorized||!originAllowed||keyCount!=1||originCount>1){Reply(stream,"403 Forbidden","application/json","{\"success\":false,\"message\":\"Request not authorized.\"}");return;}
     if(op!="Start"&&op!="Stop"&&op!="Restart"&&op!="Backup"&&op!="AutoBackupOn"&&op!="AutoBackupOff"&&op!="RecoveryOn"&&op!="RecoveryOff"){Reply(stream,"404 Not Found","text/plain","Not found");return;}
     try{var result=command==null?ActionResult.Fail("Controls are unavailable."):command(op);Reply(stream,result.Success?"202 Accepted":"409 Conflict","application/json",new JavaScriptSerializer().Serialize(new{success=result.Success,message=result.Message}));}
     catch{Reply(stream,"500 Internal Server Error","application/json","{\"success\":false,\"message\":\"The request failed. Check the launcher before retrying.\"}");}
     return;
    }
    Reply(stream,"404 Not Found","text/plain","Not found");
   }
  }
  string PhonePage(){try{if(File.Exists(uiFile)){var f=new FileInfo(uiFile);if(f.Length>0&&f.Length<500000)return File.ReadAllText(uiFile,Encoding.UTF8);}}catch{}return Page;}
  public void Dispose(){active=false;if(listener!=null)listener.Stop();lock(clients){foreach(var client in clients)try{client.Close();}catch{}}if(thread!=null&&Thread.CurrentThread!=thread)thread.Join(1000);}
  const string Page=@"<!doctype html><html lang=""en""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""><meta name=""theme-color"" content=""#121416""><meta name=""apple-mobile-web-app-capable"" content=""yes""><meta name=""apple-mobile-web-app-status-bar-style"" content=""black-translucent""><meta name=""apple-mobile-web-app-title"" content=""ShroudForge""><title>ShroudForge Launcher · Realm control</title><style>
:root{color-scheme:dark}*{box-sizing:border-box}body{background:#121416;color:#e5dfd1;font:15px system-ui;margin:0}main{max-width:850px;margin:auto;padding:28px 18px 48px}header{display:flex;justify-content:space-between;align-items:center;margin:0 0 24px}h1{font:32px Georgia;margin:6px 0}h2{font-size:16px;margin:0 0 18px}small,.muted{color:#a5a8a5;font-size:12px;line-height:1.6}.eyebrow{color:#c2a66d;letter-spacing:.16em;font-size:10px;text-transform:uppercase}.sigil{color:#c2a66d;font:36px Georgia;border:1px solid #65573d;padding:8px 15px}.panel{background:#1c1f20;border:1px solid #353936;border-radius:12px;padding:20px;margin:14px 0}.hero{border-top:2px solid #a58b57}.state{font:24px Georgia;margin:8px 0}.dot{display:inline-block;width:8px;height:8px;border-radius:50%;background:#909590;margin-right:9px}.online .dot{background:#91b99c;box-shadow:0 0 12px #91b99c55}.metrics{display:grid;grid-template-columns:repeat(4,1fr);gap:10px;margin:18px 0}.metric{background:#222627;border-radius:8px;padding:14px 12px}.metric strong{display:block;font-size:19px;font-weight:500;margin-top:8px}.actions{display:grid;grid-template-columns:repeat(3,1fr);gap:10px}button{min-height:46px;border:1px solid #55584f;background:#292e2c;color:#e5dfd1;padding:11px 15px;border-radius:7px;font:inherit;cursor:pointer}button.primary{border-color:#a58b57;background:#393528}button.danger{border-color:#795953}button:disabled{opacity:.4;cursor:default}button:focus-visible{outline:2px solid #d6b777;outline-offset:3px}.row{display:flex;align-items:center;justify-content:space-between;gap:14px;padding:14px 0;border-bottom:1px solid #333735}.row:last-child{border:0}.row strong{overflow-wrap:anywhere;min-width:0;display:block;font-weight:500;margin-bottom:4px}.switch{flex-shrink:0;min-width:66px}.switch[aria-pressed=true]{color:#a9d0ae;border-color:#6d9273;background:#27362b}.result{white-space:pre-wrap;overflow-wrap:anywhere;line-height:1.5;margin:14px 0 0;color:#c9c2b2}.working{border-left:3px solid #c2a66d;padding-left:12px}.empty{padding:14px 0;color:#a5a8a5}.badge{font-size:11px;border:1px solid #454940;border-radius:20px;padding:5px 9px;color:#c2a66d}.columns{display:grid;grid-template-columns:1fr 1fr;gap:14px}.columns .panel{margin:0}footer{text-align:center;margin:24px 0;color:#8b908a;font-size:11px}@media(max-width:600px){.metrics{grid-template-columns:1fr 1fr}.columns{grid-template-columns:1fr}.actions{gap:7px}main{padding:22px 14px}.panel{padding:17px}}

[hidden]{display:none!important}
/* Touch-first phone shell and map surface */
main{max-width:1000px;padding-bottom:calc(52px + env(safe-area-inset-bottom,0px))}
header{margin-bottom:14px}header h1{letter-spacing:-.04em}
nav.tabs{display:grid;grid-template-columns:1fr 1fr;position:sticky;top:0;z-index:10;background:#121416e8;backdrop-filter:blur(14px);gap:8px;padding:9px 0 12px}
.tabs button{border:1px solid #444944;border-radius:10px;font-weight:600;min-height:48px}
.tabs button[aria-selected=true]{color:#f4ddb1;border-color:#b3965d;background:#302c23}
.view[hidden]{display:none!important}
.mapHeading{display:flex;justify-content:space-between;align-items:center;gap:10px;flex-wrap:wrap}
.mapHeading h2{font:26px Georgia;margin:5px 0 0}
.mapWindow{border:1px solid #494d48;background:radial-gradient(circle at center,#263b35,#111819);border-radius:12px;position:relative;overflow:hidden;min-height:56vh}
.mapWindow iframe{display:block;background:#111819;border:0;width:100%;height:min(70vh,850px);min-height:420px}
.mapWindow [hidden]{display:none!important}
.mapTrouble{position:absolute;z-index:2;bottom:12px;left:12px;right:12px;padding:14px;border:1px solid #897651;background:rgba(18,23,24,.97);box-shadow:0 8px 30px #0009;border-radius:10px;color:#e5dfd1}
.mapTrouble strong{display:block;font:19px Georgia;margin-bottom:6px}
.mapTrouble p{color:#ccd0c9;font-size:13px;line-height:1.45;margin:0 0 10px}
.mapTroubleActions{display:flex;gap:8px;flex-wrap:wrap}
.mapTroubleActions button,.mapTroubleActions a{display:inline-flex;align-items:center;justify-content:center;min-height:44px;padding:10px 12px;border-radius:7px;font:13px system-ui;text-decoration:none}
.mapTroubleActions a{background:#b4975c;color:#111819;font-weight:700}
.mapTroubleActions button{border:1px solid #5b625d;background:#2e3432;color:#ece5d6}
.mapStart{display:grid;place-content:center;justify-items:center;text-align:center;gap:13px;padding:35px 20px;min-height:56vh}
.mapStart strong{font:27px Georgia;color:#eadbc4}.mapStart p{max-width:35ch;margin:0;color:#afbbb3;line-height:1.55}
.mapStart button{min-width:165px}
.mapControls{display:flex;gap:10px;flex-wrap:wrap;margin-top:13px}.mapControls a{color:#e6c589;display:inline-flex;align-items:center;justify-content:center;min-height:44px;min-width:170px;border:1px solid #75694f;border-radius:8px;padding:10px;text-decoration:none}
.mapNote{font-size:12px;color:#9daaa4;line-height:1.7;margin:12px 0}
.playerRow{display:block;padding:12px 0;border-bottom:1px solid #333735}
.playerRow:last-child{border:0}.playerTop{display:flex;justify-content:space-between;gap:10px;align-items:center}.playerBadge{font-size:11px;padding:4px 8px;border-radius:30px;border:1px solid #667b68;color:#b5d5bb}.playerBad{color:#f5ac8f;border-color:#855b50}
.playerDetails{display:flex;flex-wrap:wrap;gap:7px;margin-top:8px}.playerDetails span{background:#262d2c;border-radius:5px;color:#c9d3cd;font-size:12px;padding:5px 7px}
.activityText{font-size:13px;color:#c6cfc7;line-height:1.4}
@media(max-width:600px){.mapWindow{margin:0 -5px}.mapWindow iframe{height:clamp(220px,calc(100dvh - 225px),850px);min-height:0;max-height:850px}.mapWindow [hidden]{display:none!important}
.mapTrouble{position:absolute;z-index:2;bottom:12px;left:12px;right:12px;padding:14px;border:1px solid #897651;background:rgba(18,23,24,.97);box-shadow:0 8px 30px #0009;border-radius:10px;color:#e5dfd1}
.mapTrouble strong{display:block;font:19px Georgia;margin-bottom:6px}
.mapTrouble p{color:#ccd0c9;font-size:13px;line-height:1.45;margin:0 0 10px}
.mapTroubleActions{display:flex;gap:8px;flex-wrap:wrap}
.mapTroubleActions button,.mapTroubleActions a{display:inline-flex;align-items:center;justify-content:center;min-height:44px;padding:10px 12px;border-radius:7px;font:13px system-ui;text-decoration:none}
.mapTroubleActions a{background:#b4975c;color:#111819;font-weight:700}
.mapTroubleActions button{border:1px solid #5b625d;background:#2e3432;color:#ece5d6}
.mapStart{min-height:55vh}.mapControls a{flex:1}header h1{font-size:28px}main{padding-top:18px}}
</style></head><body><main><header><div><div class=""eyebrow"">ShroudForge Launcher / Enshrouded</div><h1 id=""name"">Your realm</h1><small>Private network · Server controls</small></div><span class=""sigil"" aria-hidden=""true"">◆</span></header>
<nav class=""tabs"" role=""tablist"" aria-label=""Phone companion sections""><button id=""overviewTab"" role=""tab"" aria-selected=""true"" aria-controls=""dashboardView"" onclick=""showTab('overview')"">Overview</button><button id=""mapTab"" role=""tab"" aria-selected=""false"" aria-controls=""mapView"" onclick=""showTab('map')"">Map</button></nav>
<div id=""dashboardView"" class=""view"" role=""tabpanel"" aria-labelledby=""overviewTab"">
<section class=""panel hero""><span class=""eyebrow"">Realm status</span><div id=""state"" class=""state""><span class=""dot""></span><span id=""stateText"">Connecting…</span></div><small id=""updated"">Waiting for the host</small><div class=""metrics""><div class=""metric""><small>UPTIME</small><strong id=""uptime"">—</strong></div><div class=""metric""><small>PLAYERS</small><strong id=""count"">—</strong></div><div class=""metric""><small>HOST CPU SHARE</small><strong id=""cpu"">—</strong></div><div class=""metric""><small>MEMORY</small><strong id=""memory"">—</strong></div></div><div class=""actions""><button id=""start"" class=""primary"" disabled onclick=""run('Start')"">Start</button><button id=""restart"" disabled onclick=""run('Restart')"">Restart</button><button id=""stop"" class=""danger"" disabled onclick=""run('Stop')"">Stop</button></div><p id=""result"" class=""result"" role=""status"" aria-live=""polite""></p><p class=""muted"">Restart only restarts the server. It never wipes the world. Stop and Restart disconnect players.</p></section>
<div class=""columns""><section class=""panel""><h2>Players online <span id=""playerBadge"" class=""badge"">0</span></h2><div id=""roster"" class=""empty"">Waiting for player information</div><small>Session times are observed by the launcher.</small></section><section class=""panel""><h2>World protection</h2><div class=""eyebrow"">Last recorded completed copy</div><p id=""lastBackup"">Checking…</p><button id=""backup"" class=""primary"" disabled onclick=""run('Backup')"">Back up now</button><p id=""backupResult"" class=""result"" role=""status""></p><p class=""muted"">Copies and verifies your saves. Existing backups are kept. Live copies may span game save cycles.</p></section></div>
<section class=""panel""><h2>World information</h2><div class=""row""><span>Save-folder files (includes backups)</span><strong id=""worldFiles"">Checking</strong></div><div class=""row""><span>Save folder size</span><strong id=""worldSize"">—</strong></div><div class=""row""><span>Last file write</span><strong id=""worldSaved"">—</strong></div><div class=""row""><span>Bases observed</span><strong id=""bases"">—</strong></div></section><section class=""panel""><h2>Known player records</h2><div id=""knownPlayers"" class=""empty""></div><p class=""muted"">Names come from server logs and session history. These records do not prove a separate player-save file exists. The host cannot list character saves stored on other players’ PCs.</p></section><section class=""panel""><h2>Recent server activity</h2><div id=""activity"" class=""empty"">Waiting for recorded events</div><p class=""muted"">Events are from the launcher and server logs; exact location is not available.</p></section>
<section class=""panel""><h2>Automation</h2><div class=""row""><div><strong>Automatic backups</strong><small id=""backupSchedule"">While this launcher is open</small></div><button id=""autoBackup"" class=""switch"" aria-pressed=""false"" disabled onclick=""run(current.autoBackup?'AutoBackupOff':'AutoBackupOn')"">Off</button></div><div class=""row""><div><strong>Crash recovery</strong><small>Up to two attempts per hour after an observed exit. Manual Stop pauses recovery.</small></div><button id=""recovery"" class=""switch"" aria-pressed=""false"" disabled onclick=""run(current.recovery?'RecoveryOff':'RecoveryOn')"">Off</button></div></section><footer>Keep the host launcher open · Keep this link private</footer></div>
<div id=""mapView"" class=""view"" role=""tabpanel"" aria-labelledby=""mapTab"" hidden>
<section class=""panel""><div class=""mapHeading""><div><div class=""eyebrow"">ShroudForge / Cartography</div><h2>Explore Embervale</h2></div><span class=""badge"">1.0 terrain</span></div>
<p class=""mapNote"">Real community map data, not a simulated landscape. Use the map's own 3D view and filters for roads, landmarks, chests, quests and resources. Pinch to zoom; drag to explore.</p>
<div class=""mapWindow""><div id=""mapStart"" class=""mapStart""><span class=""sigil"" aria-hidden=""true"">◇</span><strong>Explore Embervale</strong><p>Load the interactive 3D world map only when you need it. A connection to the map provider is required.</p><button class=""primary"" onclick=""loadMap()"">Load interactive map</button></div><iframe id=""gameMap"" title=""External interactive Embervale 3D map by GameVault"" referrerpolicy=""no-referrer"" allow=""fullscreen; gyroscope; accelerometer"" allowfullscreen hidden></iframe><div id=""mapTrouble"" class=""mapTrouble"" role=""status"" hidden><strong>Map still showing white?</strong><p>This map belongs to another website, which may prevent embedding or fail in iPhone Safari. ShroudForge cannot inspect or bypass its frame restrictions. You can open the full interactive map without affecting the server.</p><div class=""mapTroubleActions""><a href=""https://enshrouded.gamevault.in/map"" target=""_blank"" rel=""noopener noreferrer"" referrerpolicy=""no-referrer"">Open full 3D map ↗</a><button type=""button"" onclick=""loadMap(true)"">Try again</button><button id=""mapHelpMode"" type=""button"" onclick=""toggleMapMode()"">Try Protected mode</button><button type=""button"" onclick=""dismissMapHelp()"">Map works / Hide</button></div></div></div>
<div class=""mapControls""><button id=""reloadMap"" onclick=""loadMap(true)"">Reload map</button><button id=""mapMode"" type=""button"" aria-pressed=""true"" onclick=""toggleMapMode()"">Use Protected mode</button><button id=""mapTroubleshoot"" type=""button"" onclick=""showMapTrouble()"">Map blank?</button><a href=""https://enshrouded.gamevault.in/map"" target=""_blank"" rel=""noopener noreferrer"" referrerpolicy=""no-referrer"">Open full map ↗</a></div>
<p class=""mapNote"" id=""mapHelp"" role=""status"">Compatibility mode is the default because it rendered successfully in iPhone Safari. This loads an external map only after you tap Load. The provider stays on its own website and receives no ShroudForge key, private URL, save or player data. For stricter iframe controls, select Protected mode.</p>
<p class=""mapNote"">3D terrain and map markers provided by GameVault's Companion for Enshrouded. Not affiliated with Keen Games. Map loading is independent of your server controls.</p>
</section></div>
</main><script>
let current={},sending=false,notice='',mapLoaded=false,mapAssistTimer=null,mapCompatibility=true;const el=id=>document.getElementById(id);
function showTab(which){
 const map=which==='map';el('dashboardView').hidden=map;el('mapView').hidden=!map;
 el('overviewTab').setAttribute('aria-selected',String(!map));
 el('mapTab').setAttribute('aria-selected',String(map));
 if(location.hash!==(map?'#map':'#overview'))history.replaceState(null,'',map?'#map':'#overview');
}
function dismissMapHelp(){el('mapTrouble').hidden=true;}
function showMapTrouble(){
 if(!mapLoaded||el('gameMap').hidden)return;
 el('mapTrouble').hidden=false;
 text('mapHelp','If the map is blank or white, open the full interactive map. A browser iframe load event does not prove its 3D content actually rendered.');
}
function syncMapMode(){
 el('mapMode').textContent=mapCompatibility?'Use Protected mode':'Use Compatibility mode';
 el('mapMode').setAttribute('aria-pressed',String(mapCompatibility));
 el('mapHelpMode').textContent=mapCompatibility?'Try Protected mode':'Try Compatibility';
}
function toggleMapMode(){
 mapCompatibility=!mapCompatibility;
 syncMapMode();
 if(mapLoaded)loadMap(true);
}
function loadMap(reload){
 const frame=el('gameMap');
 if(mapAssistTimer!==null)clearTimeout(mapAssistTimer);
 mapAssistTimer=null;
 syncMapMode();
 el('mapTrouble').hidden=true;
 el('mapStart').hidden=true;
 frame.hidden=false;
 if(mapCompatibility)frame.removeAttribute('sandbox');
 else frame.setAttribute('sandbox','allow-scripts allow-same-origin allow-forms allow-popups');
 if(!mapLoaded||reload){
  mapLoaded=true;frame.src='https://enshrouded.gamevault.in/map';
  text('mapHelp',mapCompatibility?'Compatibility mode is active: this external map uses normal cross-origin iframe rules and no-referrer. Select Map blank? only if you need help.':'Protected mode is active: stricter iframe permissions might produce white terrain. Switch back to Compatibility if needed.');
 }
 // Cross-origin iframe failure is not reliably observable in Safari.
 // Never show a misleading success indicator just because the iframe fires ""load"".
 if(!mapCompatibility)mapAssistTimer=setTimeout(showMapTrouble,9000);
}
function text(id,value){el(id).textContent=value;}function lock(){['start','restart','stop','backup','autoBackup','recovery'].forEach(id=>el(id).disabled=true);}
async function refresh(){try{let r=await fetch('status',{cache:'no-store'});if(!r.ok)throw Error();let s=await r.json();if(s.observedUtc&&Date.now()-Date.parse(s.observedUtc)>15000)throw Error('Stale host data');current=s;text('name',s.name);text('stateText',s.busy?'Operation in progress':s.readiness||(s.online?'The realm is online':'The realm is resting'));el('state').className='state'+(s.queryConfirmed?' online':'');text('updated','Updated '+s.updated);text('uptime',s.uptime);text('count',s.players);text('cpu',s.cpu==='—'?'—':s.cpu+'%');text('memory',s.memory);text('playerBadge',s.players);const list=el('roster');list.replaceChildren();(s.roster||[]).forEach(p=>{
 let row=document.createElement('div');row.className='playerRow';let top=document.createElement('div');top.className='playerTop';
 let name=document.createElement('strong');name.textContent=p.name;let health=document.createElement('span');health.className='playerBadge'+(p.health<55?' playerBad':'');health.textContent=Number.isFinite(p.health)?p.health+' / 100':'Health unknown';top.append(name,health);
 let session=document.createElement('small');session.textContent=p.session;
 let details=document.createElement('div');details.className='playerDetails';[p.connection||'Status unknown','Ping '+(p.ping||'—'),'Jitter '+(p.jitter||'—'),'Dropped '+(p.droppedPackets??'—')].forEach(v=>{let cell=document.createElement('span');cell.textContent=v;details.append(cell);});
 row.append(top,session,details);if(p.lastIssue){let issue=document.createElement('small');issue.textContent='Last issue: '+p.lastIssue;row.append(issue);}list.append(row);
});if(!list.children.length)list.textContent=s.online?'No named players currently observed.':'Server offline';['worldFiles','worldSize','worldSaved','bases'].forEach(id=>text(id,s[id]||'Not observed'));const known=el('knownPlayers');known.replaceChildren();(s.knownPlayers||[]).forEach(p=>{let row=document.createElement('div');row.className='row';let name=document.createElement('strong');name.textContent=p.name;let evidence=document.createElement('small');evidence.textContent=p.evidence;row.append(name,evidence);known.append(row);});if(!known.children.length)known.textContent='No identifiable player records in the available history.';const events=el('activity');events.replaceChildren();(s.activity||[]).forEach(e=>{let row=document.createElement('div');row.className='row';let message=document.createElement('span');message.className='activityText';message.textContent=e.message;let when=document.createElement('small');when.textContent=e.time;row.append(message,when);events.append(row);});if(!events.children.length)events.textContent='No recent activity available.';
text('lastBackup',(s.backupCount>=0?s.backupCount+' completion markers · ':'')+s.lastBackup);text('backupSchedule','Every '+s.backupMinutes+' minutes'+(s.backupPaused?' · Paused for review':s.controls&&s.autoBackup?' · Next '+s.nextBackup:' · Currently off'));text('backupResult',s.backupStatus||'');text('result',s.operation||notice||'Ready for your next action');el('result').className='result'+(s.busy?' working':'');['autoBackup','recovery'].forEach(id=>{el(id).setAttribute('aria-pressed',s[id]?'true':'false');text(id,s.controls?(id==='recovery'&&s.recoveryPaused?'Paused':s[id]?'On':'Off'):'Paused');});const busy=s.busy||!s.controls||sending;el('start').disabled=busy||s.online;el('stop').disabled=el('restart').disabled=busy||!s.online;['backup','autoBackup','recovery'].forEach(id=>el(id).disabled=busy);}catch(e){lock();text('stateText','Connection lost');text('updated','Check the host launcher and Wi-Fi.');}}
async function run(op){if(sending)return;if(['Start','Stop','Restart'].includes(op)&&!confirm(op+' the server?'+(op==='Start'?'':' Connected players will be disconnected.')))return;sending=true;lock();notice='Sending request…';text('result',notice);try{let r=await fetch('command/'+op,{method:'POST',headers:{'X-XHL-Key':location.pathname.split('/')[1]}});let v=await r.json();notice=v.message;text('result',notice);}catch(e){notice='Request result unknown. Check status before retrying.';text('result',notice);}finally{sending=false;}await refresh();}syncMapMode();showTab(location.hash==='#map'?'map':'overview');refresh();setInterval(refresh,1000);
</script></body></html>";
 }
}
