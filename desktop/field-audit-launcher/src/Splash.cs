using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XHLLauncher {
 public sealed class LaunchSplash:Form {
  readonly string root;readonly Image art;readonly Label status;readonly Timer animation=new Timer();int phase,step;readonly Stopwatch elapsed=Stopwatch.StartNew();
  public LaunchSplash(string appBase,string serverRoot){root=serverRoot;art=Theme.LoadAsset(appBase,"realm.png");FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.CenterScreen;ClientSize=new Size(760,430);BackColor=Theme.Bg;DoubleBuffered=true;ShowInTaskbar=true;
   Controls.Add(new BrandLockup{Location=new Point(43,77),Size=new Size(300,87)});
   var sub=Theme.Label("ENSHROUDED  /  REALM COMPANION",9,Theme.Cyan,FontStyle.Bold);sub.Location=new Point(46,162);Controls.Add(sub);
   var welcome=Theme.Label("Preparing your control room.",20,Theme.Text,FontStyle.Regular);welcome.Font=new Font("Georgia",20);welcome.Location=new Point(44,226);Controls.Add(welcome);
   status=Theme.Label("Locating your existing installation…",9,Theme.Muted,FontStyle.Regular);status.SetBounds(46,285,650,45);status.AutoSize=false;Controls.Add(status);
   animation.Interval=35;animation.Tick+=delegate{phase=(phase+5)%360;Invalidate(new Rectangle(40,350,680,40));};animation.Start();Shown+=async delegate{await Prepare();};
  }
  async Task Prepare(){try{
   status.Text="01 / Locating the selected server";await Task.Run(delegate{if(!Directory.Exists(root))throw new IOException("Choose your existing server folder in Files & folders.");});step=1;
   status.Text="02 / Reading world configuration";await Task.Run(delegate{Maintenance.Config(root);});step=2;
   status.Text="03 / Verifying the pinned game and mods";var audit=await Task.Run(delegate{return InstallationAudit.Inspect(root);});step=3;status.Text=audit.CanStart?"Ready / Existing installation verified":"Ready / Open System checks to review compatibility";
  }catch(Exception ex){status.Text=ex.Message;step=3;}
   int remaining=Math.Max(0,950-(int)elapsed.ElapsedMilliseconds);if(remaining>0)await Task.Delay(remaining);if(!IsDisposed)Close();
  }
  protected override void OnPaintBackground(PaintEventArgs e){base.OnPaintBackground(e);if(art!=null){Theme.DrawImageCover(e.Graphics,art,ClientRectangle);using(var b=new LinearGradientBrush(ClientRectangle,Color.FromArgb(235,8,15,19),Color.FromArgb(80,8,15,19),0f))e.Graphics.FillRectangle(b,ClientRectangle);}using(var p=new Pen(Theme.Border))e.Graphics.DrawRectangle(p,0,0,Width-1,Height-1);}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=new Pen(Theme.Cyan,2))e.Graphics.DrawArc(p,46,355,19,19,phase,235);using(var b=new SolidBrush(Theme.Border))e.Graphics.FillRectangle(b,83,364,628,2);using(var b=new SolidBrush(Theme.Cyan))e.Graphics.FillRectangle(b,83,364,(int)(628*step/3.0),2);}
  protected override void Dispose(bool disposing){if(disposing){animation.Stop();animation.Dispose();if(art!=null)art.Dispose();}base.Dispose(disposing);}
 }
}
