using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace XHLLauncher {
 public sealed class ConfigField {
  public string Path,Section,Key,Description;public object Value;internal Action<object> Set;
  public string Label {get{return ConfigurationDocument.FriendlyName(Key);}}
 }
 public static class ConfigurationDocument {
  static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
   {"name","Server or group name"},{"saveDirectory","World save folder"},{"logDirectory","Server log folder"},{"ip","Network address to listen on"},{"queryPort","Server browser port"},{"slotCount","Maximum players"},{"tags","Server browser tags"},
   {"voiceChatMode","Voice chat range"},{"enableVoiceChat","Allow voice chat"},{"enableTextChat","Allow text chat"},{"gameSettingsPreset","Difficulty preset"},
   {"playerHealthFactor","Player health multiplier"},{"playerManaFactor","Player mana multiplier"},{"playerStaminaFactor","Player stamina multiplier"},{"playerBodyHeatFactor","Cold resistance multiplier"},{"playerDivingTimeFactor","Underwater breathing time multiplier"},
   {"enableDurability","Equipment wears out"},{"enableStarvingDebuff","Apply a penalty when starving"},{"foodBuffDurationFactor","Food bonus duration multiplier"},{"fromHungerToStarving","Time until starvation (minutes)"},{"shroudTimeFactor","Time allowed in the Shroud multiplier"},
   {"tombstoneMode","Items dropped on death"},{"enableGliderTurbulences","Glider turbulence"},{"weatherFrequency","How often weather changes"},{"fishingDifficulty","Fishing difficulty"},
   {"miningDamageFactor","Mining strength multiplier"},{"plantGrowthSpeedFactor","Plant growth speed multiplier"},{"resourceDropStackAmountFactor","Resources gathered multiplier"},{"factoryProductionSpeedFactor","Crafting station speed multiplier"},{"perkUpgradeRecyclingFactor","Runes returned when salvaging upgraded weapons"},{"perkCostFactor","Weapon upgrade cost multiplier"},
   {"experienceCombatFactor","Combat experience multiplier"},{"experienceMiningFactor","Mining experience multiplier"},{"experienceExplorationQuestsFactor","Exploration and quest experience multiplier"},
   {"randomSpawnerAmount","Number of roaming enemies"},{"aggroPoolAmount","Number of enemies attacking together"},{"enemyDamageFactor","Enemy damage multiplier"},{"enemyHealthFactor","Enemy health multiplier"},{"enemyStaminaFactor","Enemy stamina multiplier"},{"enemyPerceptionRangeFactor","Enemy detection distance multiplier"},
   {"bossDamageFactor","Boss damage multiplier"},{"bossHealthFactor","Boss health multiplier"},{"threatBonus","Enemy attack frequency multiplier"},{"pacifyAllEnemies","Make all enemies peaceful"},{"tamingStartleRepercussion","Progress lost when an animal is startled"},
   {"dayTimeDuration","Day length (minutes)"},{"nightTimeDuration","Night length (minutes)"},{"curseModifier","Curse difficulty"},{"flameLevelFogRemoval","Flame level needed to build in the Shroud"},
   {"password","Group join password"},{"canKickBan","Allow kicking and banning players"},{"canAccessInventories","Allow access to player inventories"},{"canEditWorld","Allow changes to the world"},{"canEditBase","Allow building changes"},{"canExtendBase","Allow expanding bases"},{"reservedSlots","Player slots reserved for this group"},{"bannedAccounts","Blocked player accounts"}
  };
  public static string FriendlyName(string key){string label;if(Labels.TryGetValue(key,out label))return label;label=Regex.Replace(key,"([a-z0-9])([A-Z])","$1 $2");return label.Length==0?label:char.ToUpper(label[0])+label.Substring(1);}
  public static string GroupFor(ConfigField field){
   if(field.Path.StartsWith("userGroups"))return field.Section.Replace("Permissions & passwords / ","Access: ");
   if(!field.Path.StartsWith("gameSettings."))return field.Section=="Banned accounts"?"Blocked players":"Server & chat";
   string key=field.Key;
   if(key.StartsWith("enemy")||key.StartsWith("boss")||new[]{"randomSpawnerAmount","aggroPoolAmount","threatBonus","pacifyAllEnemies"}.Contains(key))return "Combat & enemies";
   if(key.StartsWith("experience")||key.StartsWith("perk"))return "Progression & upgrades";
   if(new[]{"miningDamageFactor","plantGrowthSpeedFactor","resourceDropStackAmountFactor","factoryProductionSpeedFactor"}.Contains(key))return "Gathering & crafting";
   if(key.StartsWith("player")||new[]{"enableDurability","enableStarvingDebuff","foodBuffDurationFactor","fromHungerToStarving","shroudTimeFactor","tombstoneMode"}.Contains(key))return "Player & survival";
   return "World & exploration";
  }
  public sealed class Choice{public string Text{get;set;}public string Value{get;set;}}
  public static List<Choice> Choices(string key,string current){Dictionary<string,string> values=null;
   if(key=="tombstoneMode")values=new Dictionary<string,string>{{"AddBackpackMaterials","Drop crafting materials only"},{"Everything","Drop everything in the backpack"},{"NoTombstone","Keep everything"}};
   if(key=="voiceChatMode")values=new Dictionary<string,string>{{"Proximity","Nearby players only"},{"Global","Everyone on the server"}};
   if(key=="gameSettingsPreset")values=new Dictionary<string,string>{{"Default","Standard difficulty"},{"Relaxed","Relaxed difficulty"},{"Hard","Hard difficulty"},{"Survival","Survival difficulty"},{"Custom","My custom rules"}};
   if(values==null)return null;if(!values.ContainsKey(current))values[current]=current;return values.Select(p=>new Choice{Text=p.Value,Value=p.Key}).ToList();
  }
  public static List<ConfigField> Flatten(Dictionary<string,object> root){var fields=new List<ConfigField>();Walk(root,"","Server",fields);return fields;}
  static void Walk(Dictionary<string,object> data,string prefix,string section,List<ConfigField> fields){foreach(var kv in data.ToArray()){
   string key=kv.Key,path=prefix+key;var child=kv.Value as Dictionary<string,object>;
   string group=key=="gameSettings"?"Gameplay rules":key=="userGroups"?"Permissions & passwords":key=="bannedAccounts"?"Banned accounts":section;
   if(child!=null){Walk(child,path+".",group,fields);continue;}
   var array=kv.Value as IList;if(array!=null){if(array.Count==0)fields.Add(new ConfigField{Path=path,Section=group,Key=key,Value="[]",Description="JSON list. Edit the full list in Advanced JSON.",Set=null});for(int i=0;i<array.Count;i++){var obj=array[i] as Dictionary<string,object>;if(obj!=null){object name;string label=obj.TryGetValue("name",out name)?Convert.ToString(name):"Entry "+(i+1);Walk(obj,path+"["+i+"].",group+" / "+label,fields);}else{int idx=i;var list=array;fields.Add(new ConfigField{Path=path+"["+i+"]",Section=group,Key=key+" "+(i+1),Value=array[i],Description="List entry",Set=v=>list[idx]=v});}}continue;}
   var owner=data;fields.Add(new ConfigField{Path=path,Section=section,Key=key,Value=kv.Value,Description=Help(key),Set=v=>owner[key]=v});
  }}
  public static string Help(string key){switch(key){
   case "dayTimeDuration":case "nightTimeDuration":case "fromHungerToStarving":return "Minutes (converted automatically to the game's stored time unit).";
   case "saveDirectory":return "Protected world path. Changing it could create a different world.";
   case "gameSettingsPreset":return "Custom applies the rules below. A named preset may override custom rules.";
   case "password":return "Group join password. Use Change password to replace the saved password.";
   case "reservedSlots":return "Slots reserved for this permission group.";
   case "enableGliderTurbulences":return "Game glider turbulence setting; independent of the pitch-hold mod.";
   case "tombstoneMode":return "Choose what is left in a tombstone: materials, everything, or nothing.";
   case "enableDurability":return "When enabled, tools and equipment lose durability with use.";
   case "enableStarvingDebuff":return "Enable or disable the starvation penalty.";
   case "pacifyAllEnemies":return "When enabled, enemies do not normally initiate combat.";
   case "plantGrowthSpeedFactor":return "Higher values make crops grow faster. 1 = normal speed.";
   case "factoryProductionSpeedFactor":return "Higher values make crafting stations produce items faster.";
   case "resourceDropStackAmountFactor":return "Adjust how many resources are received when gathering.";
   case "name":return "The server name in the browser, or this permission group's name.";
   case "ip":return "0.0.0.0 listens on all available network interfaces.";
   case "queryPort":return "Port used by the server browser to query this server.";
   case "slotCount":return "Maximum number of players who can join at once.";
   default:return key.EndsWith("Factor")?"1 = normal, 1.5 = 150%, 2 = double.":key.StartsWith("can")?"Enable this permission for players in this group.":"Change the saved value used when the server next starts.";
  }}
  public static object ConvertValue(ConfigField field,object value){
   string text=Convert.ToString(value,CultureInfo.InvariantCulture);if(field.Key=="dayTimeDuration"||field.Key=="nightTimeDuration"||field.Key=="fromHungerToStarving"){decimal minutes=decimal.Parse(text,CultureInfo.InvariantCulture);if(minutes<0||minutes>1440)throw new InvalidDataException("Use a duration from 0–1,440 minutes.");if(field.Key!="fromHungerToStarving"&&(minutes<2||minutes>60))throw new InvalidDataException("Day/night duration must be 2–60 minutes.");return checked((long)(minutes*60000000000m));}
   if(field.Key.EndsWith("Factor")){double factor=double.Parse(text,CultureInfo.InvariantCulture);if(factor<0||double.IsNaN(factor)||double.IsInfinity(factor))throw new InvalidDataException("Use a nonnegative finite multiplier.");return factor;}
   if(field.Value is bool)return Convert.ToBoolean(value,CultureInfo.InvariantCulture);
   if(field.Value is int)return int.Parse(text,CultureInfo.InvariantCulture);if(field.Value is long)return long.Parse(text,CultureInfo.InvariantCulture);
   if(field.Value is decimal)return decimal.Parse(text,CultureInfo.InvariantCulture);if(field.Value is double||field.Value is float){double n=double.Parse(text,CultureInfo.InvariantCulture);if(double.IsNaN(n)||double.IsInfinity(n))throw new InvalidDataException("A finite number is required.");return n;}
   return text;
  }
 }
 public sealed partial class ControlRoom {
  DataGridView configGrid;TabControl configTabs;TextBox configJson,configSearch;ComboBox configSection;Label configResult;
  FlowLayoutPanel settingCards;ListBox settingCategories;readonly HashSet<string> settingErrors=new HashSet<string>();
  Dictionary<string,object> editedConfig;string loadedConfigHash;bool configDirty,configLoading;List<ConfigField> configFields=new List<ConfigField>();
  Control BuildFullSettings(){
   var page=Page("Server rules & configuration","Edit the actual saved server configuration. The server must be stopped before saving.");
   configSection=new ComboBox{Visible=false,DropDownStyle=ComboBoxStyle.DropDownList};page.Controls.Add(configSection);
   var find=TextLabel("FIND A SETTING",8,Theme.Muted,true);find.Location=new Point(0,72);page.Controls.Add(find);
   configSearch=new TextBox{Location=new Point(0,96),Width=300,AccessibleName="Find a setting"};Theme.StyleTextBox(configSearch);page.Controls.Add(configSearch);
   ActionButton(page,"Reload from disk",510,72,155,delegate{if(!configDirty||MessageBox.Show("Discard unsaved configuration edits?","Reload",MessageBoxButtons.YesNo)==DialogResult.Yes)LoadConfiguration();});
   ActionButton(page,"Save configuration",678,72,180,delegate{SaveConfiguration();});
   configTabs=new TabControl{Location=new Point(0,140),Size=new Size(920,450)};page.Controls.Add(configTabs);
   var fields=new TabPage("Rules & settings"){BackColor=Theme.Bg};var raw=new TabPage("Advanced JSON / lists"){BackColor=Theme.Bg};configTabs.TabPages.Add(fields);configTabs.TabPages.Add(raw);
   configGrid=new DataGridView{ReadOnly=false,MultiSelect=false};Theme.StyleGrid(configGrid);configGrid.Columns.Add("Setting","Setting");configGrid.Columns.Add("Value","Value");configGrid.Columns.Add("Meaning","Description");
   var arrangement=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,BackColor=Theme.Bg};arrangement.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));arrangement.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));arrangement.RowStyles.Add(new RowStyle(SizeType.Percent,100));fields.Controls.Add(arrangement);
   settingCategories=new ListBox{Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,BackColor=Theme.Sidebar,ForeColor=Theme.Muted,Font=Theme.Font(9,FontStyle.Regular),DrawMode=DrawMode.OwnerDrawFixed,ItemHeight=42,IntegralHeight=false};arrangement.Controls.Add(settingCategories,0,0);
   settingCategories.DrawItem+=delegate(object sender,DrawItemEventArgs e){if(e.Index<0)return;bool selected=(e.State&DrawItemState.Selected)!=0;using(var brush=new SolidBrush(selected?Theme.Panel3:Theme.Sidebar))e.Graphics.FillRectangle(brush,e.Bounds);TextRenderer.DrawText(e.Graphics,Convert.ToString(settingCategories.Items[e.Index]),settingCategories.Font,new Rectangle(e.Bounds.X+12,e.Bounds.Y,e.Bounds.Width-20,e.Bounds.Height),selected?Theme.Cyan:Theme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix);};
   settingCards=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Theme.Bg,Padding=new Padding(14,5,0,10)};arrangement.Controls.Add(settingCards,1,0);
   settingCategories.SelectedIndexChanged+=delegate{if(settingCategories.SelectedItem!=null)configSection.SelectedItem=settingCategories.SelectedItem;};
   settingCards.Resize+=delegate{foreach(Control card in settingCards.Controls)card.Width=Math.Max(360,settingCards.ClientSize.Width-35);};
   configJson=new TextBox{Dock=DockStyle.Fill,Multiline=true,ScrollBars=ScrollBars.Both,WordWrap=false,Font=new Font("Consolas",10),BackColor=Theme.Panel,ForeColor=Theme.Text,BorderStyle=BorderStyle.None};raw.Controls.Add(configJson);
   configResult=TextLabel("",9,Theme.Muted,false);configResult.SetBounds(0,594,920,68);configResult.AutoSize=false;page.Controls.Add(configResult);
   configGrid.CellValueChanged+=delegate(object sender,DataGridViewCellEventArgs e){if(configLoading||e.RowIndex<0||e.ColumnIndex!=1)return;var row=configGrid.Rows[e.RowIndex];var f=row.Tag as ConfigField;if(f==null||f.Set==null)return;try{if(f.Key=="password")return;object v=ConfigurationDocument.ConvertValue(f,row.Cells[1].Value);f.Set(v);f.Value=v;configDirty=true;row.ErrorText="";SyncConfigurationJson();}catch(Exception ex){row.ErrorText=ex.Message;configResult.Text=ex.Message;}};
   configGrid.CurrentCellDirtyStateChanged+=delegate{if(configGrid.IsCurrentCellDirty)configGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);};
   configGrid.CellDoubleClick+=delegate(object sender,DataGridViewCellEventArgs e){if(e.RowIndex<0||e.ColumnIndex!=1)return;var f=configGrid.Rows[e.RowIndex].Tag as ConfigField;if(f!=null&&f.Key=="password")EditGroupPassword(f);};
   configGrid.DataError+=delegate(object sender,DataGridViewDataErrorEventArgs e){e.ThrowException=false;configResult.Text="Invalid value. Review the highlighted setting.";};
   configSection.SelectedIndexChanged+=delegate{if(settingCategories.SelectedItem!=configSection.SelectedItem)settingCategories.SelectedItem=configSection.SelectedItem;FillConfigurationGrid();};configSearch.TextChanged+=delegate{FillConfigurationGrid();};
   configTabs.Selecting+=delegate(object sender,TabControlCancelEventArgs e){if(e.TabPageIndex==0&&configTabs.SelectedIndex==1&&!configLoading){try{editedConfig=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(configJson.Text);configFields=ConfigurationDocument.Flatten(editedConfig);FillConfigurationGrid();}catch(Exception ex){e.Cancel=true;configResult.Text="JSON error: "+ex.Message;}}};
   configJson.TextChanged+=delegate{if(!configLoading)configDirty=true;};
   page.Resize+=delegate{int w=Math.Max(720,page.ClientSize.Width-20);configTabs.Width=configResult.Width=w;configTabs.Height=Math.Max(250,page.ClientSize.Height-220);configResult.Top=configTabs.Bottom+14;};
   LoadConfiguration();return page;
  }
  void LoadConfiguration(){try{configLoading=true;editedConfig=Maintenance.Config(settings.ServerRoot);loadedConfigHash=InstallationAudit.Hash(Path.Combine(settings.ServerRoot,"enshrouded_server.json"));configFields=ConfigurationDocument.Flatten(editedConfig);configSection.Items.Clear();settingCategories.Items.Clear();foreach(string section in configFields.Select(ConfigurationDocument.GroupFor).Distinct()){configSection.Items.Add(section);settingCategories.Items.Add(section);}if(configSection.Items.Count>0){configSection.SelectedIndex=0;settingCategories.SelectedIndex=0;}settingErrors.Clear();SyncConfigurationJson();configDirty=false;FillConfigurationGrid();configResult.Text=configFields.Count+" settings • Changes apply after a server restart • Original configuration is backed up on save.";}catch(Exception ex){configResult.Text="Configuration unavailable: "+ex.Message;}finally{configLoading=false;}}
  void FillConfigurationGrid(){if(configGrid==null||editedConfig==null)return;bool previous=configLoading;configLoading=true;configGrid.Rows.Clear();string section=Convert.ToString(configSection.SelectedItem),search=configSearch.Text;
   foreach(var f in configFields.Where(f=>(search.Length>0||ConfigurationDocument.GroupFor(f)==section)&&(f.Label+" "+f.Path+" "+f.Description).IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0)){
    object display=f.Value;if(f.Value is decimal||f.Value is double||f.Value is float)display=Convert.ToDecimal(f.Value).ToString("0.###",CultureInfo.InvariantCulture);if(f.Key=="dayTimeDuration"||f.Key=="nightTimeDuration"||f.Key=="fromHungerToStarving")display=(Convert.ToDecimal(f.Value)/60000000000m).ToString("0.##",CultureInfo.InvariantCulture);if(f.Key=="password")display="••••••••";
    int i=configGrid.Rows.Add((section=="All settings"?f.Section+" / ":"")+f.Label,display,f.Description);var row=configGrid.Rows[i];row.Tag=f;if(f.Value is bool)row.Cells[1]=new DataGridViewCheckBoxCell{Value=f.Value};row.Cells[1].ReadOnly=f.Set==null||f.Key=="saveDirectory"||f.Key=="logDirectory"||f.Key=="password";
    var choices=f.Value is string?ConfigurationDocument.Choices(f.Key,(string)f.Value):null;if(choices!=null)row.Cells[1]=new DataGridViewComboBoxCell{DataSource=choices,DisplayMember="Text",ValueMember="Value",Value=f.Value,FlatStyle=FlatStyle.Flat};
   }RenderSettingCards(section,search);configLoading=previous;
  }
  void RenderSettingCards(string section,string search){
   settingCards.SuspendLayout();foreach(Control child in settingCards.Controls.Cast<Control>().ToArray()){settingCards.Controls.Remove(child);child.Dispose();}
   var items=configFields.Where(f=>(search.Length>0||ConfigurationDocument.GroupFor(f)==section)&&(f.Label+" "+f.Path+" "+f.Description).IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0).ToArray();
   var title=TextLabel(search.Length>0?"Search results":section,17,Theme.Text,false);title.AutoSize=false;title.Height=45;title.Width=Math.Max(360,settingCards.ClientSize.Width-35);title.Margin=new Padding(0,5,0,5);settingCards.Controls.Add(title);
   foreach(var field in items){var f=field;var card=(RoundedPanel)Theme.Card();card.Size=new Size(Math.Max(360,settingCards.ClientSize.Width-35),104);card.Margin=new Padding(0,0,0,12);card.Padding=Padding.Empty;settingCards.Controls.Add(card);
    var label=TextLabel(f.Label,10,Theme.Text,true);label.AutoSize=false;label.SetBounds(18,15,card.Width-205,38);label.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;card.Controls.Add(label);
    var help=TextLabel(f.Description,8.5f,Theme.Muted,false);help.AutoSize=false;help.SetBounds(18,56,card.Width-205,38);help.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;card.Controls.Add(help);
    Control input=null;
    if(f.Value is bool){var toggle=new CheckBox{Appearance=Appearance.Button,FlatStyle=FlatStyle.Flat,Checked=(bool)f.Value,Text=(bool)f.Value?"Enabled":"Disabled",TextAlign=ContentAlignment.MiddleCenter,BackColor=(bool)f.Value?Color.FromArgb(40,69,63):Theme.Panel2,ForeColor=Theme.Text,Font=Theme.Font(9,FontStyle.Bold)};toggle.FlatAppearance.BorderColor=Theme.Border;toggle.CheckedChanged+=delegate{toggle.Text=toggle.Checked?"Enabled":"Disabled";toggle.BackColor=toggle.Checked?Color.FromArgb(40,69,63):Theme.Panel2;SetCardValue(f,toggle.Checked,card);};input=toggle;}
    else if(f.Set==null){var list=Theme.Button("Edit list…",Theme.Border);list.Click+=delegate{configTabs.SelectedIndex=1;configJson.Focus();int location=configJson.Text.IndexOf("\""+f.Key+"\"",StringComparison.Ordinal);if(location>=0){configJson.SelectionStart=location;configJson.ScrollToCaret();}};input=list;}
    else if(f.Key=="password"){var password=Theme.Button("Change password",Theme.Border);password.Click+=delegate{EditGroupPassword(f);};input=password;}
    else if(f.Value is ValueType){bool minutes=f.Key=="dayTimeDuration"||f.Key=="nightTimeDuration"||f.Key=="fromHungerToStarving";decimal value=Convert.ToDecimal(f.Value);if(minutes)value/=60000000000m;bool fraction=minutes||f.Key.EndsWith("Factor")||f.Key=="threatBonus"||f.Value is decimal||f.Value is double||f.Value is float;
     var numeric=new NumericUpDown{Minimum=0,Maximum=Math.Max(1000000m,value),DecimalPlaces=fraction?2:0,Increment=minutes?1m:fraction?0.25m:1m,Value=Math.Max(0,value),BackColor=Theme.Panel2,ForeColor=Theme.Text,Font=Theme.Font(11,FontStyle.Regular),BorderStyle=BorderStyle.FixedSingle,ThousandsSeparator=true};numeric.ValueChanged+=delegate{SetCardValue(f,numeric.Value,card);};input=numeric;
    }else{var choices=ConfigurationDocument.Choices(f.Key,Convert.ToString(f.Value));if(choices!=null){var drop=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,DataSource=choices,DisplayMember="Text",ValueMember="Value",SelectedValue=f.Value,FlatStyle=FlatStyle.Flat,BackColor=Theme.Panel2,ForeColor=Theme.Text};drop.SelectedValueChanged+=delegate{if(drop.SelectedValue is string)SetCardValue(f,drop.SelectedValue,card);};input=drop;}else{var text=new TextBox{Text=Convert.ToString(f.Value),ReadOnly=f.Key=="saveDirectory"||f.Key=="logDirectory"};Theme.StyleTextBox(text);text.TextChanged+=delegate{if(!text.ReadOnly)SetCardValue(f,text.Text,card);};input=text;}}
    input.SetBounds(card.Width-174,35,156,34);input.Anchor=AnchorStyles.Top|AnchorStyles.Right;input.AccessibleName=f.Label;card.Controls.Add(input);
   }if(items.Length==0){var empty=TextLabel("No settings match your search.",10,Theme.Muted,false);settingCards.Controls.Add(empty);}settingCards.ResumeLayout();
  }
  void SetCardValue(ConfigField field,object value,RoundedPanel card){if(configLoading||field.Set==null)return;try{object converted=ConfigurationDocument.ConvertValue(field,value);field.Set(converted);field.Value=converted;settingErrors.Remove(field.Path);card.BorderColor=Theme.Border;configDirty=true;SyncConfigurationJson();configResult.Text="Unsaved changes • Save configuration when the server is stopped.";}catch(Exception ex){settingErrors.Add(field.Path);card.BorderColor=Theme.Red;configResult.Text=ex.Message;}card.Invalidate();}
  void SyncConfigurationJson(){bool prev=configLoading;configLoading=true;configJson.Text=JsonFormatting.Pretty(new JavaScriptSerializer().Serialize(editedConfig));configLoading=prev;}
  void SaveConfiguration(){if(!controlsEnabled||operationBusy||updateBusy||lifecycle.StartupPending)return;configGrid.EndEdit();if(settingErrors.Count>0||configGrid.Rows.Cast<DataGridViewRow>().Any(r=>!string.IsNullOrEmpty(r.ErrorText))){configResult.Text="Fix invalid values before saving.";return;}string text=configTabs.SelectedIndex==1?configJson.Text:new JavaScriptSerializer().Serialize(editedConfig);var result=Maintenance.SaveConfiguration(settings.ServerRoot,text,loadedConfigHash);configResult.Text=result.Message;if(result.Success){configDirty=false;LoadConfiguration();configResult.Text="Full configuration saved with a rollback copy. Changes apply on the next server start.";}}
  void EditGroupPassword(ConfigField field){using(var dialog=new Form{Text="Change group password",Size=new Size(430,185),StartPosition=FormStartPosition.CenterParent,BackColor=Theme.Bg,ForeColor=Theme.Text,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){var input=new TextBox{Location=new Point(20,25),Width=370,UseSystemPasswordChar=true};Theme.StyleTextBox(input);var ok=Theme.Button("Use new password",Theme.Cyan);ok.SetBounds(20,75,200,36);ok.DialogResult=DialogResult.OK;dialog.Controls.Add(input);dialog.Controls.Add(ok);dialog.AcceptButton=ok;if(dialog.ShowDialog(this)==DialogResult.OK){field.Set(input.Text);field.Value=input.Text;configDirty=true;SyncConfigurationJson();}}}
 }
 public static class JsonFormatting {
  public static string Pretty(string json){var b=new System.Text.StringBuilder();bool quoted=false,escaped=false;int depth=0;foreach(char c in json){if(quoted){b.Append(c);if(escaped)escaped=false;else if(c=='\\')escaped=true;else if(c=='"')quoted=false;continue;}switch(c){case '"':quoted=true;b.Append(c);break;case '{':case '[':b.Append(c);b.AppendLine();depth++;b.Append(' ',depth*2);break;case '}':case ']':b.AppendLine();depth--;b.Append(' ',depth*2);b.Append(c);break;case ',':b.Append(c);b.AppendLine();b.Append(' ',depth*2);break;case ':':b.Append(": ");break;default:if(!char.IsWhiteSpace(c))b.Append(c);break;}}return b.ToString();}
 }
}


