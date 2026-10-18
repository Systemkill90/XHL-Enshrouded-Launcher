using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace XHLLauncher {
 public sealed partial class ControlRoom {
  Label liveActivity, backupResult, readinessInfo;
  DataGridView backupGrid;
  DashboardTrend cpuTrend, memoryTrend; Label backupOverview; DateTime lastBackupOverview=DateTime.MinValue; int markedBackupCount=-1;
  void ConfigureTray(){
   var tray=new NotifyIcon{Text="ShroudForge Launcher 2.0 • "+settings.ServerName,Icon=Icon,Visible=true};var menu=new ContextMenuStrip();
   Action show=delegate{Show();WindowState=FormWindowState.Normal;Activate();};
   menu.Items.Add("Open ShroudForge Launcher",null,delegate{show();});menu.Items.Add("Exit launcher (server stays running)",null,delegate{Close();});tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate{show();};
   Resize+=delegate{if(WindowState==FormWindowState.Minimized)Hide();};FormClosed+=delegate{tray.Visible=false;tray.Dispose();menu.Dispose();};
  }
  void BuildModernShell() {
   var shell=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=Padding.Empty};
   shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));shell.RowStyles.Add(new RowStyle(SizeType.Percent,100));Controls.Add(shell);
   var side=new Panel{Dock=DockStyle.Fill,BackColor=Theme.Sidebar,Margin=Padding.Empty};shell.Controls.Add(side,0,0);
   side.Controls.Add(new BrandLockup{Location=new Point(19,28),Size=new Size(157,48)});
   var subtitle=TextLabel("REALM COMPANION",7.5f,Theme.Cyan,true);subtitle.Location=new Point(23,78);side.Controls.Add(subtitle);
   var realm=TextLabel("YOUR REALM",7,Theme.Muted2,true);realm.Location=new Point(23,123);side.Controls.Add(realm);
   string[] names={"Overview","Players","World","Mods","Logs","Backups","Automation","Updates","Settings","Installation","Files","Phone"};
   string[] labels={"Dashboard","Players & history","World","Mods","Live console","Backups & restore","Automation","Updates","Server settings","System checks","Files & folders","Phone access"};
   for(int i=0;i<names.Length;i++){
    string page=names[i];var b=Theme.Button(labels[i],Theme.Border);b.SetBounds(10,151+i*37,170,33);b.TextAlign=ContentAlignment.MiddleLeft;b.Padding=new Padding(12,0,0,0);b.FlatAppearance.BorderSize=0;b.Font=Theme.Font(9,FontStyle.Regular);b.Click+=delegate{Navigate(page);};side.Controls.Add(b);navigation.Add(page,b);
   }
   side.Controls.Add(new Label{Dock=DockStyle.Bottom,Height=48,Padding=new Padding(23,8,0,0),Text="SHROUDFORGE LAUNCHER\r\n2.0  /  VELARIS\r\nCreated by SystemKill",ForeColor=Theme.Muted2,Font=Theme.Font(7.5f,FontStyle.Regular)});
   var main=new Panel{Dock=DockStyle.Fill,Margin=Padding.Empty};shell.Controls.Add(main,1,0);
   host=new Panel{Dock=DockStyle.Fill,Padding=new Padding(26,22,26,16)};main.Controls.Add(host);
   var top=new Panel{Dock=DockStyle.Top,Height=72,BackColor=Theme.Sidebar};main.Controls.Add(top);
   serverName=TextLabel(settings.ServerName,21,Theme.Text,false);serverName.Font=new Font("Georgia",21);serverName.SetBounds(27,14,390,34);serverName.AutoSize=false;top.Controls.Add(serverName);
   var strap=TextLabel("ENSHROUDED  /  DEDICATED SERVER",7,Theme.Muted2,true);strap.Location=new Point(29,49);top.Controls.Add(strap);
   var checks=Theme.Button("System checks",Theme.Border);checks.SetBounds(620,20,142,32);checks.Anchor=AnchorStyles.Top|AnchorStyles.Right;checks.Click+=delegate{Navigate("Installation");};top.Controls.Add(checks);top.Resize+=delegate{checks.Left=top.Width-checks.Width-26;};
   footer=new Label{Dock=DockStyle.Bottom,Height=28,Padding=new Padding(26,6,0,0),ForeColor=Theme.Muted2,Font=Theme.Font(7.5f,FontStyle.Regular)};main.Controls.Add(footer);
  }
  Control BuildModernOverview() {
   var page=new Panel{AutoScroll=true};
   var hero=(RoundedPanel)Theme.Card();hero.SetBounds(0,0,1000,150);hero.BackgroundArt=Theme.LoadAsset(appBase,"realm.png");hero.BackgroundOverlayAlpha=75;page.Controls.Add(hero);
   var eyebrow=TextLabel("VELARIS  /  REALM OVERVIEW",8,Theme.Cyan,true);eyebrow.Location=new Point(24,12);hero.Controls.Add(eyebrow);
   state=Body(hero,"Observing your realm",33,24,Theme.Text);state.Left=24;state.Height=40;state.Font=new Font("Georgia",24);
   statusDetail=Body(hero,"Connecting to the existing installation",75,9,Theme.Muted);statusDetail.Left=26;statusDetail.Height=28;
   startServer=ActionButton(hero,"Start server",24,107,136,async delegate{await RunOperation("Start");});startServer.BackColor=Color.FromArgb(49,74,69);
   restartServer=ActionButton(hero,"Restart",172,107,110,async delegate{await RunOperation("Restart");});
   stopServer=ActionButton(hero,"Stop",294,107,92,async delegate{await RunOperation("Stop");});
   startServer.Visible=restartServer.Visible=stopServer.Visible=controlsEnabled;
   operationStatus=TextLabel("Checking the existing installation…",8,Theme.Muted,false);operationStatus.SetBounds(0,158,1000,28);operationStatus.AutoSize=false;page.Controls.Add(operationStatus);
   var stats=new TableLayoutPanel{Location=new Point(0,194),Size=new Size(1000,84),ColumnCount=4,RowCount=1,Margin=Padding.Empty};stats.RowStyles.Add(new RowStyle(SizeType.Percent,100));page.Controls.Add(stats);
   players=new MetricCard("Players",Theme.Teal);cpu=new MetricCard("CPU",Theme.Teal);memory=new MetricCard("Memory",Theme.Cyan);uptime=new MetricCard("Uptime",Theme.Cyan);
   cpu.Chart.Visible=false;memory.Chart.Visible=false;players.Chart.Visible=false;uptime.Chart.Visible=false;
   MetricCard[] cards={players,cpu,memory,uptime};for(int i=0;i<4;i++){stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));cards[i].Dock=DockStyle.Fill;cards[i].Margin=new Padding(i==0?0:6,0,i==3?0:6,0);stats.Controls.Add(cards[i],i,0);}
   stackState=new Label();giftingState=new Label();flightState=new Label();
   var activity=new TableLayoutPanel{Location=new Point(0,294),Size=new Size(1000,176),ColumnCount=2,RowCount=1,Margin=Padding.Empty};activity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));activity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));activity.RowStyles.Add(new RowStyle(SizeType.Percent,100));page.Controls.Add(activity);
   cpuTrend=new DashboardTrend("PROCESSOR LOAD", "%",Theme.Teal,100);memoryTrend=new DashboardTrend("SERVER MEMORY", "GB",Theme.Cyan,0);
   cpuTrend.Dock=memoryTrend.Dock=DockStyle.Fill;cpuTrend.Margin=new Padding(0,0,7,0);memoryTrend.Margin=new Padding(7,0,0,0);activity.Controls.Add(cpuTrend,0,0);activity.Controls.Add(memoryTrend,1,0);
   var world=Card(page,"WORLD & SAVES",0,486,320,138);saveSummary=Body(world,"Reading save metadata",42,9,Theme.Text);saveSummary.Height=52;
   ActionButton(world,"View world",18,100,116,delegate{Navigate("World");});
   var backup=Card(page,"BACKUP COVERAGE",334,486,320,138);backupOverview=Body(backup,"Reading backup history",42,9,Theme.Text);backupOverview.Height=52;
   ActionButton(backup,"Backups",18,100,116,delegate{Navigate("Backups");});
   var health=Card(page,"INSTALLATION & MODS",668,486,332,138);auditSummary=Body(health,"Checking",40,9,Theme.Text);auditSummary.Height=25;auditDetail=Body(health,"",68,8,Theme.Muted);auditDetail.Height=30;
   ActionButton(health,"Review checks",18,100,128,delegate{Navigate("Installation");});ActionButton(health,"Mods",154,100,78,delegate{Navigate("Mods");});
   liveActivity=TextLabel("Waiting for live telemetry",8,Theme.Muted,false);liveActivity.SetBounds(0,638,1000,30);liveActivity.AutoSize=false;page.Controls.Add(liveActivity);
   readinessInfo=TextLabel("READINESS / Checking pinned files and backup markers",8,Theme.Muted,false);readinessInfo.SetBounds(0,674,1000,28);readinessInfo.AutoSize=false;page.Controls.Add(readinessInfo);
   page.Resize+=delegate{int w=Math.Max(720,page.ClientSize.Width-20);hero.Width=stats.Width=activity.Width=operationStatus.Width=liveActivity.Width=readinessInfo.Width=w;int third=(w-28)/3;world.Width=backup.Width=third;backup.Left=third+14;health.Left=2*(third+14);health.Width=w-health.Left;};
   return page;
  }
  void UpdateModernDetails() {
   if(liveActivity!=null){var h=telemetry.GetHost();cpuTrend.UpdateSamples(telemetry.CpuHistory,h.ServerRunning);memoryTrend.UpdateSamples(telemetry.RamHistory,h.ServerRunning);liveActivity.Text=h.ServerRunning?"LIVE  /  PID "+h.Pid+"  •  Trends show actual process samples from this launcher session":"OFFLINE  /  Live trends unavailable. Saved world and backup records remain available.";
    readinessInfo.Text="READINESS / "+SafeSupportReport.Readiness(h,audit,markedBackupCount);
    if((DateTime.Now-lastBackupOverview).TotalSeconds<10)return;lastBackupOverview=DateTime.Now;
    try{var snapshots=Directory.Exists(BackupRoot)?new DirectoryInfo(BackupRoot).GetDirectories().Where(d=>File.Exists(Path.Combine(d.FullName,"VERIFIED.txt"))).OrderByDescending(d=>d.CreationTimeUtc).ToArray():new DirectoryInfo[0];markedBackupCount=snapshots.Length;backupOverview.Text=snapshots.Length+" verified snapshots"+(snapshots.Length>0?"\r\nLatest  "+snapshots[0].CreationTime.ToString("MMM d, h:mm tt"):"\r\nNo verified snapshot found")+"\r\nAutomatic backups  "+(settings.AutoBackup?"Every "+settings.BackupIntervalMinutes+" min":"Off");}catch{backupOverview.Text="Backup history unavailable";}
   }
  }
  Control BuildBackups() {
   var page=Page("Backups & restore","Verified world copies, with your previous world retained on restore.");
   ActionButton(page,"Back up now",0,76,150,async delegate{await BackupNow();RefreshBackupList();});
   ActionButton(page,"Refresh",164,76,100,delegate{RefreshBackupList();});
   ActionButton(page,"Restore selected",278,76,166,async delegate{
    if(!controlsEnabled||operationBusy||updateBusy||lifecycle.StartupPending||backupGrid.SelectedRows.Count!=1)return;
    string snapshot=backupGrid.SelectedRows[0].Tag as string;if(snapshot==null)return;
    if(MessageBox.Show("Restore this verified OFFLINE world snapshot? The server must be stopped. A fresh backup and the current world will be retained.","Restore world",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
    operationBusy=true;UpdateState();var result=await Task.Run(delegate{return Maintenance.Restore(settings.ServerRoot,BackupRoot,snapshot);});operationBusy=false;backupResult.Text=result.Message;RefreshBackupList();UpdateState();
   });
   backupGrid=new DataGridView{Location=new Point(0,132),Size=new Size(900,350),ReadOnly=true,MultiSelect=false};Theme.StyleGrid(backupGrid);foreach(var c in new[]{"Created","Verification","Snapshot"})backupGrid.Columns.Add(c,c);page.Controls.Add(backupGrid);
   backupResult=TextLabel("Select a verified offline snapshot to restore. Live copies are listed but cannot be restored automatically.",9,Theme.Muted,false);backupResult.SetBounds(0,500,900,90);backupResult.AutoSize=false;page.Controls.Add(backupResult);
   page.Resize+=delegate{backupGrid.Width=Math.Max(200,page.ClientSize.Width-20);backupResult.Width=backupGrid.Width;};return page;
  }
  void RefreshBackupList(){if(backupGrid==null)return;backupGrid.Rows.Clear();try{if(!Directory.Exists(BackupRoot))return;foreach(var d in new DirectoryInfo(BackupRoot).GetDirectories().OrderByDescending(x=>x.CreationTimeUtc).Take(100)){bool valid=File.Exists(Path.Combine(d.FullName,"VERIFIED.txt"));int row=backupGrid.Rows.Add(d.CreationTime.ToString("MMM d, h:mm tt"),valid?"Byte-verified":"Incomplete / not verified",d.Name);backupGrid.Rows[row].Tag=d.FullName;}}catch(Exception ex){backupResult.Text=ex.Message;}}
  Control BuildSettings(){ return BuildFullSettings(); }
  Control BuildUpdates(){var page=Page("Safe updates","Download separately. Check all three mods before selecting an update.");var c=Card(page,"A PROTECTED UPDATE PATH",0,78,780,220);Body(c,"1   Select the official SteamCMD application.\r\n2   Download to a separate staging folder.\r\n3   Verify the exact game build and mod compatibility.\r\n4   Stop, copy and verify current saves, then switch.\r\n\r\nUnknown game builds stay blocked. Your current installation remains available.",48,11,Theme.Text);ActionButton(page,"Download update",0,320,180,async delegate{await DownloadUpdate();});ActionButton(page,"Use verified update",195,320,190,async delegate{await ActivateUpdate();});ActionButton(page,"System checks",400,320,155,delegate{Navigate("Installation");});return page;}
 }
 public sealed class DashboardTrend : RoundedPanel {
  readonly string title,unit;readonly Color line;readonly double fixedMaximum;double[] samples=new double[0];bool online;
  public DashboardTrend(string title,string unit,Color line,double maximum){this.title=title;this.unit=unit;this.line=line;fixedMaximum=maximum;DoubleBuffered=true;BackColor=Theme.Panel;}
  public void UpdateSamples(double[] values,bool running){samples=values.Skip(Math.Max(0,values.Length-120)).ToArray();online=running;Invalidate();}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
   using(var font=Theme.Font(8,FontStyle.Bold))using(var brush=new SolidBrush(Theme.Muted))g.DrawString(title,font,brush,18,15);
   string value=online&&samples.Length>0?samples[samples.Length-1].ToString("0.0")+" "+unit:"—";
   using(var font=Theme.Font(23,FontStyle.Regular))using(var brush=new SolidBrush(Theme.Text))g.DrawString(value,font,brush,16,35);
   var area=new RectangleF(18,91,Math.Max(1,Width-36),Math.Max(1,Height-121));
   double max=fixedMaximum>0?fixedMaximum:Math.Max(1,samples.Length==0?1:samples.Max()*1.15);
   using(var pen=new Pen(Theme.Border)){for(int i=0;i<3;i++){float y=area.Top+i*area.Height/2;g.DrawLine(pen,area.Left,y,area.Right,y);}}
   string caption=!online?"Server offline · no live data":samples.Length<2?"Collecting process samples…":"Recent "+samples.Length+" samples  ·  "+(fixedMaximum>0?"0–100%":"0–"+max.ToString("0.0")+" GB");
   if(online&&samples.Length>=2){var points=samples.Select((v,i)=>new PointF(area.Left+i*area.Width/(samples.Length-1),area.Bottom-(float)(Math.Min(max,Math.Max(0,v))/max)*area.Height)).ToArray();var fill=points.Concat(new[]{new PointF(area.Right,area.Bottom),new PointF(area.Left,area.Bottom)}).ToArray();using(var brush=new SolidBrush(Color.FromArgb(24,line)))g.FillPolygon(brush,fill);using(var pen=new Pen(line,2))g.DrawLines(pen,points);using(var brush=new SolidBrush(line)){var last=points[points.Length-1];g.FillEllipse(brush,last.X-3,last.Y-3,6,6);}}
   using(var font=Theme.Font(7.5f,FontStyle.Regular))using(var brush=new SolidBrush(Theme.Muted))g.DrawString(caption,font,brush,18,Height-24);
  }
 }

}





