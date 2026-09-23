using System.Diagnostics;
using GodTreeXOwais.Core;
using GodTreeXOwais.Runtime;
using GodTreeXOwais.Infrastructure;
namespace GodTreeXOwais;
public sealed class MainForm:Form{
 readonly ProjectStore store=new();readonly GodTreeEngine engine=new();readonly GitService git=new();readonly ExportService exporter=new();
 ProjectInfo? project;
 readonly TextBox mission=new(){Multiline=true,Dock=DockStyle.Fill,Font=new("Segoe UI",11),PlaceholderText="Give GodTree a mission…",BackColor=Color.FromArgb(18,23,32),ForeColor=Color.White,BorderStyle=BorderStyle.FixedSingle};
 readonly RichTextBox log=new(){Dock=DockStyle.Fill,ReadOnly=true,BackColor=Color.FromArgb(9,12,18),ForeColor=Color.Gainsboro,Font=new("Cascadia Mono",9),BorderStyle=BorderStyle.None};
 readonly ListView graph=new(){Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,BackColor=Color.FromArgb(13,17,23),ForeColor=Color.White,BorderStyle=BorderStyle.None};
 readonly TreeView files=new(){Dock=DockStyle.Fill,BackColor=Color.FromArgb(13,17,23),ForeColor=Color.Gainsboro,BorderStyle=BorderStyle.None};
 readonly Label state=new(){AutoSize=true,Text="NO PROJECT",ForeColor=Color.FromArgb(122,162,247),Font=new("Segoe UI Semibold",9)};
 public MainForm(){
  Text="GodTreeXOwais";Width=1400;Height=860;MinimumSize=new(1050,680);StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(8,11,17);ForeColor=Color.White;
  graph.Columns.Add("Node",70);graph.Columns.Add("Stage",190);graph.Columns.Add("State",90);graph.Columns.Add("Message",300);
  var header=new Panel{Dock=DockStyle.Top,Height=64,BackColor=Color.FromArgb(13,17,23)};
  header.Controls.Add(new Label{Text="GODTREE × OWAIS",AutoSize=true,Location=new(18,16),Font=new("Segoe UI Semibold",18),ForeColor=Color.White});state.Location=new(1190,23);header.Controls.Add(state);
  var nav=new FlowLayoutPanel{Dock=DockStyle.Left,Width=180,FlowDirection=FlowDirection.TopDown,Padding=new(12,16,12,12),BackColor=Color.FromArgb(11,15,22)};
  nav.Controls.Add(B("＋ NEW PROJECT",NewProject));nav.Controls.Add(B("▶ RUN TREE",Run));nav.Controls.Add(B("■ STOP",Stop));nav.Controls.Add(B("↻ REFRESH FILES",(_,_)=>LoadFiles()));nav.Controls.Add(B("GIT STATUS",GitStatus));nav.Controls.Add(B("COMMIT",Commit));nav.Controls.Add(B("EXPORT HANDOFF",Export));nav.Controls.Add(B("OPEN FOLDER",OpenFolder));
  var main=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=370,BackColor=Color.FromArgb(30,36,48)};
  var leftSplit=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=260};leftSplit.Panel1.Padding=new(10);leftSplit.Panel1.Controls.Add(mission);leftSplit.Panel2.Padding=new(10);leftSplit.Panel2.Controls.Add(files);main.Panel1.Controls.Add(leftSplit);
  var rightSplit=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=410};rightSplit.Panel1.Padding=new(10);rightSplit.Panel1.Controls.Add(graph);rightSplit.Panel2.Padding=new(10);rightSplit.Panel2.Controls.Add(log);main.Panel2.Controls.Add(rightSplit);
  Controls.Add(main);Controls.Add(nav);Controls.Add(header);
  engine.Event+=e=>BeginInvoke(()=>OnEvent(e));NewProject(null!,EventArgs.Empty);
 }
 Button B(string t,EventHandler h){var b=new Button{Text=t,Width=150,Height=38,Margin=new(0,0,0,8),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(22,28,39),ForeColor=Color.White,Cursor=Cursors.Hand};b.FlatAppearance.BorderColor=Color.FromArgb(45,55,72);b.Click+=h;return b;}
 void NewProject(object? s,EventArgs e){project=store.Create("Project "+DateTime.Now.ToString("HHmm"));state.Text=project.Name.ToUpperInvariant();Write("Project created: "+project.Root);LoadFiles();}
 async void Run(object? s,EventArgs e){if(project==null||string.IsNullOrWhiteSpace(mission.Text)){Write("Mission required.");return;}graph.Items.Clear();state.Text="RUNNING";Write("Run started.");await engine.Run(project,mission.Text);state.Text="READY";LoadFiles();Write("Run finished.");}
 void Stop(object? s,EventArgs e){engine.Cancel();state.Text="STOPPING";Write("Cancellation requested.");}
 void OnEvent(NodeEvent e){var item=new ListViewItem(new[]{e.NodeId,e.NodeId,e.State.ToString().ToUpperInvariant(),e.Message});graph.Items.Add(item);graph.EnsureVisible(graph.Items.Count-1);Write($"{e.NodeId} {e.State}: {e.Message}");}
 void Write(string s){log.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}\n");log.ScrollToCaret();}
 void LoadFiles(){files.Nodes.Clear();if(project==null)return;var root=new TreeNode(project.Name){Tag=project.Root};files.Nodes.Add(root);Fill(root,project.Root,2);root.Expand();}
 void Fill(TreeNode node,string dir,int depth){if(depth<0)return;try{foreach(var d in Directory.EnumerateDirectories(dir).Take(40)){var n=node.Nodes.Add(Path.GetFileName(d));Fill(n,d,depth-1);}foreach(var f in Directory.EnumerateFiles(dir).Take(80))node.Nodes.Add(Path.GetFileName(f));}catch{}}
 async void GitStatus(object? s,EventArgs e){if(project!=null)Write(await git.Status(project.Root));}
 async void Commit(object? s,EventArgs e){if(project!=null)Write(await git.Commit(project.Root,"GodTree checkpoint"));}
 void Export(object? s,EventArgs e){if(project!=null)Write("Exported: "+exporter.Export(project.Root));}
 void OpenFolder(object? s,EventArgs e){if(project!=null)Process.Start(new ProcessStartInfo("explorer.exe",project.Root){UseShellExecute=true});}
}
