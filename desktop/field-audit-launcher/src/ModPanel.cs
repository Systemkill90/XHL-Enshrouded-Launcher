using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace XHLLauncher {
 public sealed partial class ControlRoom {
  DataGridView managedMods;TextBox modDetails;Label modNotice;Button addMod,removeMod,configureMod,startupMod,stopForMods;DateTime nextModScan=DateTime.MinValue;
  Control BuildModManager(){
   var page=Page("Mods & configuration","Manage supported startup mods and your local package library.");page.Name="ModsPage";
   var actions=new FlowLayoutPanel{Location=new Point(0,76),Size=new Size(870,82),WrapContents=true,AutoScroll=false};page.Controls.Add(actions);
   addMod=ModButton(actions,"Add mod folder",delegate{ImportMod();});
   configureMod=ModButton(actions,"Configure",delegate{ConfigureSelectedMod();});
   startupMod=ModButton(actions,"Enable at startup",async delegate{await ChangeSelectedStartup(true);});
   removeMod=ModButton(actions,"Remove",async delegate{await RemoveSelectedMod();});removeMod.Width=190;
   ModButton(actions,"Refresh checks",async delegate{await InspectAsync();RefreshManagedMods();});
   ModButton(actions,"Removed packages",delegate{string path=ModManagement.Archive(appBase);Directory.CreateDirectory(path);Process.Start("explorer.exe","\""+path+"\"");});
   stopForMods=ModButton(actions,"Stop server…",async delegate{await RunOperation("Stop");RefreshManagedMods();});
   managedMods=new DataGridView{Location=new Point(0,170),Size=new Size(870,215),ReadOnly=true,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,AccessibleName="Managed mods"};Theme.StyleGrid(managedMods);managedMods.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
   foreach(string title in new[]{"Mod","Version","Type","Startup / installation"})managedMods.Columns.Add(title,title);
   managedMods.Columns[0].FillWeight=135;managedMods.Columns[1].FillWeight=65;managedMods.SelectionChanged+=delegate{ShowSelectedMod();};page.Controls.Add(managedMods);
   modDetails=new TextBox{Location=new Point(0,397),Size=new Size(870,96),Multiline=true,ReadOnly=true,WordWrap=true,ScrollBars=ScrollBars.Vertical,BackColor=Theme.Panel2,ForeColor=Theme.Text,BorderStyle=BorderStyle.FixedSingle,Font=Theme.Font(10,FontStyle.Regular)};page.Controls.Add(modDetails);
   modNotice=TextLabel("Startup changes require a stopped server and a verified backup. Library imports never execute package code.",9,Theme.Muted,false);modNotice.SetBounds(0,505,870,52);modNotice.AutoSize=false;page.Controls.Add(modNotice);
   var evidence=Card(page,"CURRENT RUNTIME / LAST START",0,570,870,300);modStatus=Body(evidence,"Reading installed mod evidence",42,9,Theme.Text);
   page.Resize+=delegate{int width=Math.Max(600,page.ClientSize.Width-20);actions.Width=managedMods.Width=modDetails.Width=modNotice.Width=evidence.Width=width;page.PerformLayout();};
   RefreshManagedMods();return page;
  }
  Button ModButton(FlowLayoutPanel parent,string title,Action action){var button=Theme.Button(title,Theme.Border);button.Size=new Size(145,34);button.Margin=new Padding(0,0,8,6);button.Click+=delegate{try{action();}catch(Exception ex){ModResult(ActionResult.Fail(ex.Message));}};parent.Controls.Add(button);return button;}
  ManagedMod SelectedMod(){return managedMods!=null&&managedMods.SelectedRows.Count>0?managedMods.SelectedRows[0].Tag as ManagedMod:null;}
  bool ModBusy {get{return operationBusy||updateBusy||lifecycle.StartupPending;}}
  void RefreshManagedMods(){
   if(managedMods==null)return;string selected=SelectedMod()==null?null:SelectedMod().Id;managedMods.Rows.Clear();
   try{foreach(var mod in ModManagement.List(settings.ServerRoot,appBase)){string startup=!mod.BuiltIn?"Library only · not loaded":mod.Locked?"Required · check disk below":mod.Enabled?(mod.Id=="flight-stamina"&&!ModManagement.Enabled(settings.ServerRoot,"flight")?"Waiting for Flight":"Configured on next start"):"Disabled next start";int row=managedMods.Rows.Add(mod.Name,mod.Version,mod.Kind,startup);managedMods.Rows[row].Tag=mod;if(mod.Id==selected){managedMods.ClearSelection();managedMods.Rows[row].Selected=true;managedMods.CurrentCell=managedMods.Rows[row].Cells[0];}}}
   catch(Exception ex){modNotice.Text="Mod catalog unavailable: "+ex.Message;}
   nextModScan=DateTime.UtcNow.AddSeconds(5);ShowSelectedMod();
  }
  void ShowSelectedMod(){
   if(modDetails==null)return;var mod=SelectedMod();bool busy=ModBusy,running=telemetry.GetHost().ServerRunning;
   addMod.Enabled=controlsEnabled&&!busy;stopForMods.Enabled=controlsEnabled&&!busy&&running;configureMod.Enabled=mod!=null&&!busy;removeMod.Enabled=mod!=null&&controlsEnabled&&!busy&&!mod.Locked&&(!mod.BuiltIn||(!running&&mod.Enabled));startupMod.Enabled=mod!=null&&mod.BuiltIn&&!mod.Locked&&!mod.Enabled&&controlsEnabled&&!busy&&!running;
   removeMod.Text=mod!=null&&mod.BuiltIn?"Remove from startup":"Remove package";
   modDetails.Text=mod==null?"Select a mod to view its supported actions.":mod.Name+"\r\n"+mod.Support+(mod.BuiltIn&&running?"\r\nStop the server before changing startup options. The running mod is unchanged.":"");
  }
  void ModResult(ActionResult result){if(modNotice!=null){modNotice.Text=result.Message;modNotice.ForeColor=result.Success?Theme.Green:Theme.Amber;}Storage.AppendLauncherEvent(appBase,result.Success?"Info":"Warning",result.Message);}
  async void ImportMod(){
   if(!controlsEnabled||ModBusy)return;
   using(var dialog=new FolderBrowserDialog{Description="Choose a local mod folder containing mod.json. It is copied into the inactive library; no loader or script is installed."}){
    if(dialog.ShowDialog()!=DialogResult.OK)return;string source=dialog.SelectedPath;operationBusy=true;ShowSelectedMod();
    try{var result=await Task.Run(delegate{return ModManagement.Import(source,appBase);});if(!IsDisposed)ModResult(result);}
    catch(Exception ex){if(!IsDisposed)ModResult(ActionResult.Fail(ex.Message));}
    finally{operationBusy=false;if(!IsDisposed){RefreshManagedMods();UpdateState();}}
   }
  }
  async Task ChangeSelectedStartup(bool enabled){var mod=SelectedMod();if(mod==null||!mod.BuiltIn||mod.Locked||!controlsEnabled||ModBusy)return;await ChangeStartup(mod,enabled);}
  async Task ChangeStartup(ManagedMod mod,bool enabled){
   if(telemetry.GetHost().ServerRunning){ModResult(ActionResult.Fail("Stop the server first. No running mod was changed."));return;}
   string text=(enabled?"Enable ":"Remove ")+mod.Name+(enabled?" at the next startup?":" from the next startup?")+"\r\n\r\nA stopped-world backup is verified first. Controller files are kept for rollback."+(mod.Id=="flight"&&!enabled?"\r\nFlight stamina is also disabled because it depends on Flight.":"");
   if(MessageBox.Show(text,"Change startup mod",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
   operationBusy=true;ShowSelectedMod();UpdateState();
   try{var result=await Task.Run(delegate{return ModManagement.SetStartup(settings.ServerRoot,appBase,mod.Id,enabled,BackupRoot);});if(!IsDisposed)ModResult(result);}
   catch(Exception ex){if(!IsDisposed)ModResult(ActionResult.Fail(ex.Message));}
   finally{operationBusy=false;if(!IsDisposed){RefreshManagedMods();UpdateState();}}
  }
  async Task RemoveSelectedMod(){var mod=SelectedMod();if(mod==null||!controlsEnabled||ModBusy)return;if(mod.BuiltIn){if(mod.Locked){ModResult(ActionResult.Fail(mod.Support));return;}await ChangeStartup(mod,false);return;}
   if(MessageBox.Show("Remove "+mod.Name+" from the local library?\r\n\r\nThe package is moved to Removed Mods and can be re-imported. Game files and the running server are unchanged.","Remove package",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
   ModResult(ModManagement.Remove(appBase,mod.Id));RefreshManagedMods();
  }
  void ConfigureSelectedMod(){var mod=SelectedMod();if(mod==null||ModBusy)return;if(!mod.BuiltIn){EditModConfiguration(mod);return;}
   using(var dialog=new Form{Text="Configure "+mod.Name,Size=new Size(630,365),MinimumSize=new Size(630,365),StartPosition=FormStartPosition.CenterParent,BackColor=Theme.Bg,ForeColor=Theme.Text,Font=Theme.Font(10,FontStyle.Regular)}){
    var note=new Label{Text=mod.Support,Location=new Point(22,22),Size=new Size(565,104)};dialog.Controls.Add(note);
    var on=new CheckBox{Text=mod.Locked?"Required by this world":"Enable at next safe startup",Checked=mod.Enabled,Location=new Point(22,142),Size=new Size(565,36),Enabled=controlsEnabled&&!mod.Locked&&!telemetry.GetHost().ServerRunning};dialog.Controls.Add(on);
    var status=new Label{Text=mod.Locked?"This resource patch cannot be removed safely from this world.":telemetry.GetHost().ServerRunning?"Stop the server to change this setting. No live memory changes are made.":"Saving will verify a backup before changing the startup flag.",Location=new Point(22,194),Size=new Size(565,58),ForeColor=Theme.Muted};dialog.Controls.Add(status);
    var save=Theme.Button("Save configuration",Theme.Border);save.SetBounds(22,266,200,36);save.Enabled=on.Enabled;save.Click+=async delegate{bool value=on.Checked;dialog.Close();await ChangeStartup(mod,value);};dialog.Controls.Add(save);
    var close=Theme.Button("Close",Theme.Border);close.SetBounds(237,266,120,36);close.Click+=delegate{dialog.Close();};dialog.Controls.Add(close);dialog.ShowDialog(this);
   }
  }
  void EditModConfiguration(ManagedMod mod){
   string[] files=ModManagement.ConfigFiles(appBase,mod.Id);if(files.Length==0){ModResult(ActionResult.Fail("This package contains no supported text configuration files. Code and mod.json are not edited by this panel."));return;}
   using(var dialog=new Form{Text="Configure "+mod.Name+" · inactive library",Size=new Size(820,610),MinimumSize=new Size(640,480),StartPosition=FormStartPosition.CenterParent,BackColor=Theme.Bg,ForeColor=Theme.Text,Font=Theme.Font(10,FontStyle.Regular)}){
    var top=new Panel{Dock=DockStyle.Top,Height=84};dialog.Controls.Add(top);top.Controls.Add(new Label{Text="Library copy only. These settings do not affect the server until a compatible loader is available.",Location=new Point(16,10),Size=new Size(750,28)});
    var choice=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Location=new Point(16,44),Size=new Size(540,32)};choice.Items.AddRange(files);top.Controls.Add(choice);
    var bottom=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=56,Padding=new Padding(12,6,0,0)};dialog.Controls.Add(bottom);
    var editor=new TextBox{Dock=DockStyle.Fill,Multiline=true,ScrollBars=ScrollBars.Both,WordWrap=false,Font=new Font("Consolas",10),BackColor=Theme.Panel2,ForeColor=Theme.Text,MaxLength=1048576};dialog.Controls.Add(editor);editor.BringToFront();top.BringToFront();bottom.BringToFront();
    string hash="",loaded="";bool loading=false,dirty=false;int previous=-1;
    Action load=delegate{if(choice.SelectedItem==null)return;loading=true;try{loaded=(string)choice.SelectedItem;editor.Text=ModManagement.ReadConfig(appBase,mod.Id,loaded,out hash);previous=choice.SelectedIndex;dirty=false;}finally{loading=false;}};
    editor.TextChanged+=delegate{if(!loading)dirty=true;};
    choice.SelectedIndexChanged+=delegate{if(loading)return;if(dirty&&MessageBox.Show("Discard unsaved edits before switching files?","Unsaved mod configuration",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes){loading=true;choice.SelectedIndex=previous;loading=false;return;}try{load();}catch(Exception ex){ModResult(ActionResult.Fail(ex.Message));}};
    var save=ModButton(bottom,"Save configuration",delegate{var result=ModManagement.SaveConfig(appBase,mod.Id,loaded,editor.Text,hash);ModResult(result);MessageBox.Show(result.Message,"Mod configuration",MessageBoxButtons.OK,result.Success?MessageBoxIcon.Information:MessageBoxIcon.Warning);if(result.Success){hash=InstallationAudit.Hash(Path.Combine(mod.Directory,loaded));dirty=false;}});save.Enabled=controlsEnabled;
    ModButton(bottom,"Reload from disk",delegate{if(!dirty||MessageBox.Show("Discard unsaved edits and reload?","Reload configuration",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)load();});ModButton(bottom,"Close",delegate{dialog.Close();});
    dialog.FormClosing+=delegate(object sender,FormClosingEventArgs args){if(dirty&&MessageBox.Show("Discard unsaved configuration edits?","Unsaved mod configuration",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)args.Cancel=true;};choice.SelectedIndex=0;dialog.ShowDialog(this);
   }
  }
 }
}
