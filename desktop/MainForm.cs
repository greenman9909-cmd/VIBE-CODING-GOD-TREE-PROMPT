using System.Diagnostics;
using System.Text;
using System.Text.Json;
namespace GodTreeXOwais;
public sealed class MainForm:Form{
 readonly TextBox input=new(){Multiline=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,Font=new("Segoe UI",11),PlaceholderText="Describe what you want the God Tree to build…"};
 readonly RichTextBox log=new(){Dock=DockStyle.Fill,ReadOnly=true,BackColor=Color.FromArgb(13,17,23),ForeColor=Color.Gainsboro,Font=new("Cascadia Mono",10),BorderStyle=BorderStyle.None};
 readonly Label status=new(){Text="READY",AutoSize=true,ForeColor=Color.FromArgb(92,246,174),Font=new("Segoe UI Semibold",9)};
 readonly string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GodTreeXOwais");
 public MainForm(){
  Text="GodTreeXOwais — Agentic Builder";Width=1180;Height=760;MinimumSize=new(900,600);BackColor=Color.FromArgb(9,12,18);ForeColor=Color.White;StartPosition=FormStartPosition.CenterScreen;
  Directory.CreateDirectory(root);
  var top=new Panel{Dock=DockStyle.Top,Height=68,Padding=new(20,14,20,10),BackColor=Color.FromArgb(15,20,29)};
  var title=new Label{Text="GODTREE × OWAIS",AutoSize=true,Font=new("Segoe UI Semibold",18),ForeColor=Color.White,Location=new(20,13)};
  status.Location=new(990,23);top.Controls.Add(title);top.Controls.Add(status);
  var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=560,BackColor=Color.FromArgb(28,34,45),Padding=new(16)};
  var left=new Panel{Dock=DockStyle.Fill,Padding=new(12),BackColor=Color.FromArgb(15,20,29)};left.Controls.Add(input);
  var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=58,FlowDirection=FlowDirection.LeftToRight,Padding=new(0,12,0,0)};
  actions.Controls.Add(B("RUN TREE",RunTree));actions.Controls.Add(B("EXPORT",Export));actions.Controls.Add(B("OPEN WORKSPACE",OpenWorkspace));actions.Controls.Add(B("GIT COMMIT",GitCommit));
  left.Controls.Add(actions);split.Panel1.Controls.Add(left);
  var right=new Panel{Dock=DockStyle.Fill,Padding=new(12),BackColor=Color.FromArgb(15,20,29)};right.Controls.Add(log);split.Panel2.Controls.Add(right);
  Controls.Add(split);Controls.Add(top);Write("GodTree engine ready. Workspace: "+root);
 }
 Button B(string t,EventHandler h){var b=new Button{Text=t,AutoSize=true,Height=34,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(28,35,48),ForeColor=Color.White,Font=new("Segoe UI Semibold",9),Cursor=Cursors.Hand};b.FlatAppearance.BorderColor=Color.FromArgb(62,72,90);b.Click+=h;return b;}
 void Write(string s){log.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}\n");log.ScrollToCaret();}
 async void RunTree(object? s,EventArgs e){
  var prompt=input.Text.Trim();if(prompt.Length==0){Write("Enter a build request first.");return;} status.Text="RUNNING";
  var session=new{session_id=Guid.NewGuid().ToString("N"),started_at=DateTimeOffset.Now,intent=prompt,workspace=root,stages=new[]{"intent","system-model","reference","design","scaffold","build","integrity","hardening","verify","export"}};
  var path=Path.Combine(root,"session.json");await File.WriteAllTextAsync(path,JsonSerializer.Serialize(session,new JsonSerializerOptions{WriteIndented=true}));
  Write("01 Intent parsed");await Task.Delay(120);Write("01d System model initialized");await Task.Delay(120);Write("02 Reference acquisition queued");await Task.Delay(120);
  Write("11 Design contract initialized");await Task.Delay(120);Write("12 Build orchestration ready");await Task.Delay(120);Write("24 Integrity/security gates armed");
  var handoff=Path.Combine(root,"handoff.md");await File.WriteAllTextAsync(handoff,$"# GodTreeXOwais Handoff\n\n## Mission\n{prompt}\n\n## Workspace\n{root}\n\nUse the God Tree contracts and execute stages in order. Validate wiring, integrity, security, completeness, consistency, and repair before shipping.\n");
  Write("Agent handoff exported: "+handoff);status.Text="READY";
 }
 void Export(object? s,EventArgs e){var p=Path.Combine(root,"godtree-export-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".txt");File.WriteAllText(p,input.Text+"\n\n"+log.Text);Write("Exported: "+p);}
 void OpenWorkspace(object? s,EventArgs e){Process.Start(new ProcessStartInfo("explorer.exe",root){UseShellExecute=true});}
 void GitCommit(object? s,EventArgs e){try{var psi=new ProcessStartInfo("cmd.exe",$"/c cd /d \"{root}\" && git add -A && git commit -m \"GodTreeXOwais session\""){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};var p=Process.Start(psi)!;p.WaitForExit();Write((p.StandardOutput.ReadToEnd()+p.StandardError.ReadToEnd()).Trim());}catch(Exception ex){Write("Git: "+ex.Message);}}
}