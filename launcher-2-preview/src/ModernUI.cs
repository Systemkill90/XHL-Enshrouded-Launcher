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
  Label liveActivity, backupResult;
  DataGridView backupGrid;
  void ConfigureTray(){
   var tray=new NotifyIcon{Text="XHL 2.0 • "+settings.ServerName,Icon=Icon,Visible=true};var menu=new ContextMenuStrip();
   Action show=delegate{Show();WindowState=FormWindowState.Normal;Activate();};
   menu.Items.Add("Open XHL",null,delegate{show();});menu.Items.Add("Exit launcher (server stays running)",null,delegate{Close();});tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate{show();};
   Resize+=delegate{if(WindowState==FormWindowState.Minimized)Hide();};FormClosed+=delegate{tray.Visible=false;tray.Dispose();menu.Dispose();};
  }
  void BuildModernShell() {
   var shell=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=Padding.Empty};
   shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));shell.RowStyles.Add(new RowStyle(SizeType.Percent,100));Controls.Add(shell);
   var side=new Panel{Dock=DockStyle.Fill,BackColor=Theme.Sidebar,Margin=Padding.Empty};shell.Controls.Add(side,0,0);
   side.Controls.Add(new Sigil{Location=new Point(23,23),Size=new Size(32,40)});
   var brand=TextLabel("XHL",25,Theme.Text,true);brand.Location=new Point(66,24);side.Controls.Add(brand);
   var subtitle=TextLabel("REALM COMPANION",7.5f,Theme.Cyan,true);subtitle.Location=new Point(23,78);side.Controls.Add(subtitle);
   var realm=TextLabel("YOUR REALM",7,Theme.Muted2,true);realm.Location=new Point(23,123);side.Controls.Add(realm);
   string[] names={"Overview","Players","World","Mods","Logs","Backups","Automation","Updates","Settings","Installation","Files","Phone"};
   string[] labels={"Dashboard","Players & history","World","Mods","Live console","Backups & restore","Automation","Updates","Server settings","System checks","Files & folders","Phone access"};
   for(int i=0;i<names.Length;i++){
    string page=names[i];var b=Theme.Button(labels[i],Theme.Border);b.SetBounds(10,151+i*37,170,33);b.TextAlign=ContentAlignment.MiddleLeft;b.Padding=new Padding(12,0,0,0);b.FlatAppearance.BorderSize=0;b.Font=Theme.Font(9,FontStyle.Regular);b.Click+=delegate{Navigate(page);};side.Controls.Add(b);navigation.Add(page,b);
   }
   side.Controls.Add(new Label{Dock=DockStyle.Bottom,Height=48,Padding=new Padding(23,8,0,0),Text="XHL 2.0  /  VELARIS\r\nYour world, within reach.",ForeColor=Theme.Muted2,Font=Theme.Font(7.5f,FontStyle.Regular)});
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
   var hero=(RoundedPanel)Theme.Card();hero.SetBounds(0,0,1000,208);hero.BackgroundArt=Theme.LoadAsset(appBase,"realm.png");hero.BackgroundOverlayAlpha=75;page.Controls.Add(hero);
   var eyebrow=TextLabel("VELARIS  /  REALM OVERVIEW",8,Theme.Cyan,true);eyebrow.Location=new Point(24,20);hero.Controls.Add(eyebrow);
   state=Body(hero,"Observing your realm",52,27,Theme.Text);state.Left=24;state.Height=48;state.Font=new Font("Georgia",27);
   statusDetail=Body(hero,"Connecting to the existing installation",104,9,Theme.Muted);statusDetail.Left=26;statusDetail.Height=28;
   startServer=ActionButton(hero,"Start server",24,150,136,async delegate{await RunOperation("Start");});startServer.BackColor=Color.FromArgb(49,74,69);
   restartServer=ActionButton(hero,"Restart",172,150,110,async delegate{await RunOperation("Restart");});
   stopServer=ActionButton(hero,"Stop",294,150,92,async delegate{await RunOperation("Stop");});
   startServer.Visible=restartServer.Visible=stopServer.Visible=controlsEnabled;
   operationStatus=TextLabel("Checking the existing installation…",8,Theme.Muted,false);operationStatus.SetBounds(0,220,1000,36);operationStatus.AutoSize=false;page.Controls.Add(operationStatus);
   var stats=new TableLayoutPanel{Location=new Point(0,264),Size=new Size(1000,96),ColumnCount=4,RowCount=1,Margin=Padding.Empty};stats.RowStyles.Add(new RowStyle(SizeType.Percent,100));page.Controls.Add(stats);
   players=new MetricCard("Players",Theme.Teal);cpu=new MetricCard("CPU",Theme.Teal);memory=new MetricCard("Memory",Theme.Cyan);uptime=new MetricCard("Uptime",Theme.Cyan);
   MetricCard[] cards={players,cpu,memory,uptime};for(int i=0;i<4;i++){stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));cards[i].Dock=DockStyle.Fill;cards[i].Margin=new Padding(i==0?0:6,0,i==3?0:6,0);stats.Controls.Add(cards[i],i,0);}
   var modHead=TextLabel("BALANCED MODS",8,Theme.Muted,true);modHead.Location=new Point(1,384);page.Controls.Add(modHead);
   var mods=new TableLayoutPanel{Location=new Point(0,412),Size=new Size(1000,134),ColumnCount=3,RowCount=1};mods.RowStyles.Add(new RowStyle(SizeType.Percent,100));page.Controls.Add(mods);
   Label[] values=new Label[3];string[] titles={"01   STACKING","02   GIFTING","03   FLIGHT & PITCH HOLD"};for(int i=0;i<3;i++){mods.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3));var c=Card(mods,titles[i],0,0,310,134);c.Dock=DockStyle.Fill;c.Margin=new Padding(i==0?0:6,0,i==2?0:6,0);mods.SetCellPosition(c,new TableLayoutPanelCellPosition(i,0));values[i]=Body(c,"Checking compatibility",46,10,Theme.Text);}
   stackState=values[0];giftingState=values[1];flightState=values[2];
   var world=Card(page,"WORLD RECORD",0,562,490,110);saveSummary=Body(world,"Reading save metadata",43,10,Theme.Text);
   var health=Card(page,"INSTALLATION",506,562,494,110);auditSummary=Body(health,"Checking",40,11,Theme.Text);auditSummary.Height=25;auditDetail=Body(health,"",70,8,Theme.Muted);auditDetail.Height=28;
   liveActivity=TextLabel("Waiting for live telemetry",9,Theme.Muted,false);liveActivity.SetBounds(0,692,1000,46);liveActivity.AutoSize=false;page.Controls.Add(liveActivity);
   page.Resize+=delegate{int w=Math.Max(720,page.ClientSize.Width-20);hero.Width=stats.Width=mods.Width=operationStatus.Width=liveActivity.Width=w;int half=(w-16)/2;world.Width=half;health.Left=half+16;health.Width=w-half-16;};
   return page;
  }
  void UpdateModernDetails() {
   if(liveActivity!=null){var h=telemetry.GetHost();liveActivity.Text=h.ServerRunning?"LIVE  /  Observing PID "+h.Pid+"     •     World saves remain in their configured folder":"Ready when you are. Start uses the existing verified backup and mod routine.";}
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
}

