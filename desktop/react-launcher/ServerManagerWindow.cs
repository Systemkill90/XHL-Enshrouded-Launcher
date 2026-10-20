using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XHLLauncher
{
    internal sealed class ServerManagerWindow : Form
    {
        readonly string app;
        readonly LauncherSettings settings;
        readonly ListBox servers=new ListBox{Dock=DockStyle.Fill,HorizontalScrollbar=true};
        readonly TextBox status=new TextBox{Multiline=true,ReadOnly=true,Dock=DockStyle.Bottom,Height=120,ScrollBars=ScrollBars.Vertical};
        readonly Button scan=new Button{Text="Scan Steam / known folders",AutoSize=true},browse=new Button{Text="Scan a folder…",AutoSize=true},create=new Button{Text="Create new server…",AutoSize=true},use=new Button{Text="Use selected server",AutoSize=true},cancel=new Button{Text="Cancel",AutoSize=true};
        readonly List<string> extra=new List<string>();bool working;
        internal ServerManagerWindow(string app,LauncherSettings settings,bool createNew=false)
        {
            this.app=app;this.settings=settings;Text="ShroudForge — Server installations";ClientSize=new Size(850,510);MinimumSize=new Size(700,450);StartPosition=FormStartPosition.CenterParent;
            var title=new Label{Text="Find an existing installation or create a separate standard server. Scanning never moves, modifies or starts a server.",Dock=DockStyle.Top,Height=48,Padding=new Padding(12)};
            var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=52,Padding=new Padding(10)};actions.Controls.AddRange(new Control[]{scan,browse,create,use,cancel});Controls.Add(servers);Controls.Add(status);Controls.Add(actions);Controls.Add(title);
            scan.Click+=async delegate{await Search();};browse.Click+=async delegate{using(var picker=new FolderBrowserDialog{Description="Choose a folder to search (up to four levels)"})if(picker.ShowDialog(this)==DialogResult.OK){extra.Add(picker.SelectedPath);await Search();}};
            create.Click+=async delegate{await Create();};use.Click+=async delegate{await UseSelected();};cancel.Click+=delegate{Close();};Shown+=async delegate{if(createNew)await Create();else await Search();};FormClosing+=delegate(object sender,FormClosingEventArgs e){if(working){e.Cancel=true;MessageBox.Show(this,"Wait for the current scan or download to finish.","ShroudForge");}};
        }
        void Working(bool value){working=value;scan.Enabled=browse.Enabled=create.Enabled=use.Enabled=cancel.Enabled=!value;}
        async Task Search()
        {
            Working(true);status.Text="Scanning selected and Steam library folders…";
            try{var result=await Task.Run(()=>ServerSetup.Scan(ServerSetup.DefaultRoots(app,settings).Concat(extra)));servers.Items.Clear();foreach(var item in result.Servers.OrderBy(s=>s.Name))servers.Items.Add(item);if(servers.Items.Count>0)servers.SelectedIndex=0;status.Text=result.Servers.Count+" installation(s) found. "+(result.Limited?"Scan limit reached; choose a narrower folder. ":"")+string.Join(Environment.NewLine,result.Errors.ToArray());}
            catch(Exception ex){status.Text="Scan failed: "+ex.Message;}finally{Working(false);}
        }
        async Task Create()
        {
            using(var form=new NewServerWindow(settings)){
                if(form.ShowDialog(this)!=DialogResult.OK)return;
                Working(true);status.Text="Creating a new server…";
                try{
                    Action<string> progress=line=>{if(!IsDisposed&&IsHandleCreated)BeginInvoke(new Action(()=>status.Text=line));};
                    var result=await Task.Run(()=>ServerSetup.Create(app,settings.ServerRoot,form.Target,form.SteamCmd,form.ServerName,form.Password,form.Port,form.Slots,progress));status.Text=result.Message;
                    if(result.Success){extra.Add(form.Target);settings.SteamCmdPath=form.SteamCmd;servers.Items.Add(new FoundServer{Root=form.Target,Name=form.ServerName,Kind="Registered standard"});servers.SelectedIndex=servers.Items.Count-1;}
                }catch(Exception ex){status.Text="Creation failed: "+ex.Message;}finally{Working(false);}
            }
        }
        async Task UseSelected()
        {
            var selected=servers.SelectedItem as FoundServer;if(selected==null)return;
            if(selected.Error!=null){MessageBox.Show(this,selected.Error,"Server folder needs attention");return;}
            if(MessageBox.Show(this,"Reopen the launcher for \""+selected.Name+"\"?\r\n\r\nThe currently selected server will be left alone. If it is running, it will keep running; this launcher will stop monitoring it until you switch back.","Use selected server",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            Working(true);
            try {
                if(!VanillaServer.Managed(selected.Root)&&!File.Exists(Path.Combine(selected.Root,"XHL-Diagnostic-Wrapper.ps1"))){
                    if(MessageBox.Show(this,"Register this as a standard server without mods?\r\n\r\nThis records the current server-file fingerprints for guarded startup. Existing configuration and saves stay unchanged. Modded installations must use their existing verified launch profile.","Register existing standard server",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
                    var registered=await Task.Run(()=>VanillaServer.Register(selected.Root));if(!registered.Success){status.Text=registered.Message;return;}
                }
                var result=ServerSetup.Select(app,settings,selected.Root);status.Text=result.Message;
                if(result.Success){Working(false);DialogResult=DialogResult.OK;Close();}
            }catch(Exception ex){status.Text=ex.Message;}finally{Working(false);}
        }
    }
    internal sealed class NewServerWindow : Form
    {
        readonly TextBox target=new TextBox(),steam=new TextBox(),name=new TextBox{Text="New Enshrouded Server"},password=new TextBox{UseSystemPasswordChar=true};
        readonly NumericUpDown port=new NumericUpDown{Minimum=1024,Maximum=65535,Value=15647},slots=new NumericUpDown{Minimum=1,Maximum=16,Value=16};
        internal string Target{get{return target.Text;}}internal string SteamCmd{get{return steam.Text;}}internal string ServerName{get{return name.Text;}}internal string Password{get{return password.Text;}}internal int Port{get{return (int)port.Value;}}internal int Slots{get{return (int)slots.Value;}}
        internal NewServerWindow(LauncherSettings settings)
        {
            Text="Create a separate server";ClientSize=new Size(650,420);StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;steam.Text=settings.SteamCmdPath;
            Add("New empty server folder",target,20);Add("Official steamcmd.exe",steam,70);Add("Server name",name,120);Add("Administrator join password",password,170);Add("UDP query port",port,220);Add("Player slots",slots,270);
            var folder=new Button{Text="Browse",Bounds=new Rectangle(555,20,75,28)};folder.Click+=delegate{using(var picker=new FolderBrowserDialog{Description="Choose or create a new empty server folder"})if(picker.ShowDialog(this)==DialogResult.OK)target.Text=picker.SelectedPath;};Controls.Add(folder);
            var executable=new Button{Text="Browse",Bounds=new Rectangle(555,70,75,28)};executable.Click+=delegate{using(var picker=new OpenFileDialog{Filter="SteamCMD|steamcmd.exe",CheckFileExists=true})if(picker.ShowDialog(this)==DialogResult.OK)steam.Text=picker.FileName;};Controls.Add(executable);
            Controls.Add(new Label{Text="Downloads a fresh standard server and configures a separate world. Nothing starts automatically. Use a different query port from other running servers. The password grants the new server's administrator role.",Bounds=new Rectangle(20,315,610,48)});
            var ok=new Button{Text="Download and create",Bounds=new Rectangle(335,375,160,30)};ok.Click+=delegate{if(MessageBox.Show(this,"Download the dedicated server into the chosen empty folder?\r\nExisting servers/worlds are not copied or changed. The download may take several minutes.","Create server",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes){DialogResult=DialogResult.OK;Close();}};Controls.Add(ok);
            var cancel=new Button{Text="Cancel",Bounds=new Rectangle(510,375,120,30),DialogResult=DialogResult.Cancel};Controls.Add(cancel);CancelButton=cancel;
        }
        void Add(string caption,Control control,int y){Controls.Add(new Label{Text=caption,Bounds=new Rectangle(20,y+3,205,26)});control.Bounds=new Rectangle(230,y,310,28);Controls.Add(control);}
    }
}

