using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using XHLLauncher;
static class ProductionChecks {
 static int count;
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("PASS "+message);count++;}
 static byte[] Info(){using(var stream=new MemoryStream()){stream.Write(new byte[]{255,255,255,255,73,17},0,6);foreach(string s in new[]{"Fixture realm","","Enshrouded","Enshrouded"}){byte[] b=Encoding.UTF8.GetBytes(s+"\0");stream.Write(b,0,b.Length);}byte[] tail={0,0,0,16,0,100,119,1,0};stream.Write(tail,0,tail.Length);return stream.ToArray();}}
 static void Settings(string root){
  var settings=new LauncherSettings{ServerRoot=root,ServerName="Original",PhoneViewKey="preserved-private-fixture-key",AutoRestartOnCrash=false};
  Storage.SaveSettings(root,settings);settings.ServerName="Updated";Storage.SaveSettings(root,settings);
  string path=Path.Combine(root,"Manager Data","settings.xml"),before=File.ReadAllText(path);
  Check(Storage.LoadSettings(root).ServerName=="Updated"&&File.Exists(path+".bak"),"Settings saved with last-good backup");
  File.WriteAllText(path,"<truncated");var recovered=Storage.LoadSettings(root);
  Check(recovered.ServerName=="Original"&&recovered.PhoneViewKey==settings.PhoneViewKey&&Storage.LastSettingsNotice!=null,"Truncated settings recover backup, retaining server identity and key");
  Check(File.ReadAllText(path)=="<truncated","Loading recovered settings preserves corrupt evidence");
  Storage.SaveSettings(root,recovered);
  Check(Directory.GetFiles(Path.GetDirectoryName(path),"*.corrupt-*").Length==1&&Storage.LoadSettings(root).ServerName=="Original","Saving recovery quarantines corrupt file without overwriting good backup");
  File.WriteAllText(path,"bad");File.WriteAllText(path+".bak","bad");bool failed=false;try{Storage.LoadSettings(root);}catch(IOException){failed=true;}
  Check(failed&&File.ReadAllText(path)=="bad","Two invalid settings copies block startup rather than silently select defaults");
 }
 static void Activation(string root){
  using(var primary=new RealmInstance(root))using(var second=new RealmInstance(root+Path.DirectorySeparatorChar)){
   Check(primary.IsPrimary&&!second.IsPrimary,"Canonical realm path admits one primary launcher");
   Check(primary.ConsumeActivation()&&!primary.ConsumeActivation(),"Duplicate launch requests one window activation");
   using(var another=new RealmInstance(root+"-other"))Check(another.IsPrimary,"Different realm has independent instance lock");
  }
  using(var next=new RealmInstance(root))Check(next.IsPrimary,"Normal close releases instance lock for reopening");
 }
 static void Query(){
  var info=ServerQuery.Decode(Info());Check(info.Name=="Fixture realm"&&info.Players==0&&info.Capacity==16,"Zero-player game info is a valid readiness response");
  foreach(byte[] invalid in new[]{new byte[0],new byte[]{255,255,255,255,73,17},Encoding.ASCII.GetBytes("HTTP/1.1 200 OK")}){bool failed=false;try{ServerQuery.Decode(invalid);}catch(IOException){failed=true;}Check(failed,"Malformed game-query reply rejected");}
  using(var server=new UdpClient(new IPEndPoint(IPAddress.Loopback,0))){
   Exception error=null;int requests=0;server.Client.ReceiveTimeout=2000;
   var thread=new Thread(delegate(){try{var remote=new IPEndPoint(IPAddress.Any,0);var first=server.Receive(ref remote);requests++;if(first[4]!=84)throw new Exception("Expected INFO");var challenge=new byte[]{255,255,255,255,65,1,2,3,4};server.Send(challenge,challenge.Length,remote);var second=server.Receive(ref remote);requests++;if(second[second.Length-1]!=4)throw new Exception("Challenge not echoed");var response=Info();server.Send(response,response.Length,remote);}catch(Exception ex){error=ex;}});thread.Start();
   var responseInfo=ServerQuery.Read("127.0.0.1",((IPEndPoint)server.Client.LocalEndPoint).Port,1000);thread.Join(3000);
   Check(error==null&&requests==2&&responseInfo.Name=="Fixture realm","Game-query challenge exchange completes on isolated UDP socket");
  }
 }
 static void Phone(){
  int calls=0;
  using(var phone=new PhoneView("production-fixture-private-key",op=>{Interlocked.Increment(ref calls);return ActionResult.Ok(op);},"missing-fixture-page")){
   phone.Start(IPAddress.Loopback,0);phone.Publish(new{name="fixture",online=false});var uri=new Uri(phone.Url);
   using(var stalled=new TcpClient())using(var client=new WebClient()){
    stalled.Connect(uri.Host,uri.Port);stalled.GetStream().WriteByte(71);
    var timer=Stopwatch.StartNew();string status=client.DownloadString(phone.Url+"status");timer.Stop();
    Check(status.Contains("fixture")&&timer.ElapsedMilliseconds<1500,"Slow phone connection does not block status requests");
    Check(client.ResponseHeaders["Content-Security-Policy"].Contains("frame-ancestors 'none'")&&client.ResponseHeaders["Referrer-Policy"]=="no-referrer","Phone responses isolate framing and suppress private-key referrers");
    string html=client.DownloadString(phone.Url);Check(html.Contains("https://enshroudeddatabase.com/map/")&&html.Contains("if(map&&!mapLoaded)loadMap()"),"Packaged fallback includes embedded map and lazy tab loading");
   }
   using(var oversized=new TcpClient(uri.Host,uri.Port)){
    var stream=oversized.GetStream();oversized.ReceiveTimeout=3000;var bytes=Encoding.ASCII.GetBytes(new string('X',8192));stream.Write(bytes,0,bytes.Length);var read=new byte[512];int n=stream.Read(read,0,read.Length);Check(Encoding.ASCII.GetString(read,0,n).Contains("431"),"Oversized phone headers fail before dispatch");
   }
   Check(calls==0,"Read and malformed-request checks dispatch zero operations");
  }
  using(var phone=new PhoneView("fault-fixture",op=>{throw new Exception("private exception details");})){
   phone.Start(IPAddress.Loopback,0);using(var client=new WebClient()){
    client.Headers["X-XHL-Key"]="fault-fixture";bool failed=false;try{client.UploadString(phone.Url+"command/Backup","POST","");}catch(WebException ex){using(var response=ex.Response)failed=((HttpWebResponse)response).StatusCode==HttpStatusCode.InternalServerError;}
    Check(failed&&client.DownloadString(phone.Url+"status")=="{}","Phone handler failure returns error without taking down service");
   }
  }
 }
 static int Main(string[] args){try{string root=Path.Combine(args[0],"production-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);Settings(root);Activation(root);Query();Phone();Console.WriteLine(count+" production regression checks passed.");return 0;}catch(Exception ex){Console.WriteLine("FAIL "+ex);return 1;}}
}
