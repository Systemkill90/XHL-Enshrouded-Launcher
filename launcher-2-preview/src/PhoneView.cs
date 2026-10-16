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
  readonly string token; internal string Url {get;private set;}
  readonly Func<string, ActionResult> command;
  internal PhoneView(string key, Func<string, ActionResult> handler=null) {token=key;command=handler;}
  internal static IPAddress LocalAddress() {
   foreach(var n in NetworkInterface.GetAllNetworkInterfaces()) {if(n.OperationalStatus!=OperationalStatus.Up || n.NetworkInterfaceType==NetworkInterfaceType.Loopback)continue; var props=n.GetIPProperties(); if(props.GatewayAddresses.Count==0)continue; foreach(var a in props.UnicastAddresses) {var b=a.Address.GetAddressBytes();if(b.Length==4 && (b[0]==10 || (b[0]==192 && b[1]==168) || (b[0]==172 && b[1]>=16 && b[1]<=31)))return a.Address;}}
   throw new IOException("No private home-network IPv4 address found.");
  }
  internal void Start(IPAddress address,int port) {listener=new TcpListener(address,port);listener.Start(8);active=true;Url="http://"+address+":"+((IPEndPoint)listener.LocalEndpoint).Port+"/"+token+"/";thread=new Thread(Serve){IsBackground=true};thread.Start();}
  internal void Publish(object data){lock(gate)json=new JavaScriptSerializer().Serialize(data);}
  void Serve(){while(active)try{using(var c=listener.AcceptTcpClient()){c.ReceiveTimeout=2000;c.SendTimeout=2000;using(var stream=c.GetStream()){var bytes=new MemoryStream();int v;while(bytes.Length<8192 && (v=stream.ReadByte())>=0){bytes.WriteByte((byte)v);var a=bytes.GetBuffer();int n=(int)bytes.Length;if(n>=4 && a[n-4]==13 && a[n-3]==10 && a[n-2]==13 && a[n-1]==10)break;}
   string req=Encoding.ASCII.GetString(bytes.ToArray());string[] line=req.Split('\n')[0].Trim().Split(' ');string status="404 Not Found",type="text/plain",body="Not found";
   if(line.Length==3 && line[0]=="GET" && (line[1]=="/"+token+"/" || line[1]=="/"+token+"/status")) {status="200 OK";if(line[1].EndsWith("/status")){type="application/json";lock(gate)body=json;}else{type="text/html; charset=utf-8";body=Page;}}
   if(line.Length==3 && line[0]=="POST" && line[1].StartsWith("/"+token+"/command/")) {
    string op=line[1].Substring(("/"+token+"/command/").Length);
    bool authorized=false,originAllowed=true;
    foreach(string header in req.Split('\n')) {int colon=header.IndexOf(':');if(colon<0)continue;string name=header.Substring(0,colon).Trim(),value=header.Substring(colon+1).Trim();if(name.Equals("X-XHL-Key",StringComparison.OrdinalIgnoreCase))authorized=value==token;if(name.Equals("Origin",StringComparison.OrdinalIgnoreCase))originAllowed=value==new Uri(Url).GetLeftPart(UriPartial.Authority);}
    if(authorized && originAllowed && (op=="Start" || op=="Stop" || op=="Restart")) {var result=command==null?ActionResult.Fail("Controls are unavailable."):command(op);status=result.Success?"202 Accepted":"409 Conflict";type="application/json";body=new JavaScriptSerializer().Serialize(new {success=result.Success,message=result.Message});}
   }
   byte[] payload=Encoding.UTF8.GetBytes(body);byte[] headers=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+"\r\nContent-Type: "+type+"\r\nContent-Length: "+payload.Length+"\r\nCache-Control: no-store\r\nReferrer-Policy: no-referrer\r\nX-Content-Type-Options: nosniff\r\nX-Frame-Options: DENY\r\nConnection: close\r\n\r\n");stream.Write(headers,0,headers.Length);stream.Write(payload,0,payload.Length);}}}catch{if(!active)return;}}
  public void Dispose(){active=false;if(listener!=null)listener.Stop();}
  const string Page=@"<!doctype html><html><head><meta name='viewport' content='width=device-width,initial-scale=1'><title>XHL Server</title><style>body{background:#121416;color:#e5dfd1;font:16px system-ui;margin:24px;max-width:900px}h1{font-family:Georgia;color:#c2a66d}section{background:#202224;border:1px solid #41403a;border-radius:8px;padding:18px;margin:16px 0}button{background:#34332d;color:#e5dfd1;border:1px solid #c2a66d;padding:16px 22px;margin:6px;border-radius:6px;font:inherit}button:disabled{opacity:.4}small{color:#aaa}#state{font-size:24px}</style></head><body><h1 id='name'>XHL Server</h1><small>Private network · Server controls</small><section><div id='state'>Connecting…</div><p id='stats'></p><small id='updated'></small></section><section><button id='start' disabled onclick=""run('Start')"">Start</button><button id='stop' disabled onclick=""run('Stop')"">Stop</button><button id='restart' disabled onclick=""run('Restart')"">Restart</button><p id='result'></p><small>Stop and Restart disconnect players. Saves are preserved.</small></section><script>async function refresh(){try{let r=await fetch('status',{cache:'no-store'});if(!r.ok)throw Error();let s=await r.json();document.getElementById('name').textContent=s.name;document.getElementById('state').textContent=s.online?'Server online':'Server offline';document.getElementById('stats').textContent=s.players+' players · CPU '+s.cpu+'% · RAM '+s.memory;document.getElementById('updated').textContent='Updated '+s.updated;document.getElementById('start').disabled=s.busy||!s.controls||s.online;document.getElementById('stop').disabled=document.getElementById('restart').disabled=s.busy||!s.controls||!s.online;document.getElementById('result').textContent=s.operation;}catch(e){document.querySelectorAll('button').forEach(b=>b.disabled=true);document.getElementById('state').textContent='Connection lost — check the host launcher';}}async function run(op){if(!confirm(op+' the server?'+(op==='Start'?'':' Connected players will be disconnected.')))return;document.querySelectorAll('button').forEach(b=>b.disabled=true);try{let r=await fetch('command/'+op,{method:'POST',headers:{'X-XHL-Key':location.pathname.split('/')[1]}});let v=await r.json();document.getElementById('result').textContent=v.message;}catch(e){document.getElementById('result').textContent='Request result unknown. Check status before retrying.';}await refresh();}refresh();setInterval(refresh,2000);</script></body></html>";
 }
}
