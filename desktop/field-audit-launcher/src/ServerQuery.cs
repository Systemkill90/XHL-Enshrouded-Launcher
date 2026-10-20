using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace XHLLauncher {
 internal sealed class ServerQueryInfo {internal string Name;internal int Players;internal int Capacity;}
 internal static class ServerQuery {
  internal static ServerQueryInfo Read(string host,int port,int timeout) {
   using(var socket=new UdpClient()) {
    socket.Client.ReceiveTimeout=timeout;socket.Connect(host,port);
    byte[] header={255,255,255,255,84};byte[] text=Encoding.ASCII.GetBytes("Source Engine Query\0");byte[] request=new byte[header.Length+text.Length];Buffer.BlockCopy(header,0,request,0,header.Length);Buffer.BlockCopy(text,0,request,header.Length,text.Length);
    socket.Send(request,request.Length);var remote=new IPEndPoint(IPAddress.Any,0);byte[] reply=socket.Receive(ref remote);
    if(reply.Length==9&&reply[4]==0x41){byte[] challenge=new byte[request.Length+4];Buffer.BlockCopy(request,0,challenge,0,request.Length);Buffer.BlockCopy(reply,5,challenge,request.Length,4);socket.Send(challenge,challenge.Length);reply=socket.Receive(ref remote);}
    return Decode(reply);
   }
  }
  static string Text(byte[] bytes,ref int offset){int begin=offset;while(offset<bytes.Length&&bytes[offset]!=0)offset++;if(offset==bytes.Length)throw new IOException("Incomplete server info string");string value=Encoding.UTF8.GetString(bytes,begin,offset-begin);offset++;return value;}
  internal static ServerQueryInfo Decode(byte[] bytes){
   if(bytes.Length<6||bytes[0]!=255||bytes[1]!=255||bytes[2]!=255||bytes[3]!=255||bytes[4]!=0x49)throw new IOException("Invalid server info reply");
   int offset=6;string name=Text(bytes,ref offset);Text(bytes,ref offset);string folder=Text(bytes,ref offset),game=Text(bytes,ref offset);
   if(offset+5>bytes.Length)throw new IOException("Incomplete server info counts");
   if(folder.IndexOf("Enshrouded",StringComparison.OrdinalIgnoreCase)<0&&game.IndexOf("Enshrouded",StringComparison.OrdinalIgnoreCase)<0)throw new IOException("Query answered by another game");
   int players=bytes[offset+2],capacity=bytes[offset+3];if(capacity==0||players>capacity)throw new IOException("Invalid player counts");
   return new ServerQueryInfo{Name=name,Players=players,Capacity=capacity};
  }
 }
}
