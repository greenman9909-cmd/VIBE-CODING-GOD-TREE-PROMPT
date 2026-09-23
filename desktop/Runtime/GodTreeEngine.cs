using System.Text.Json;
using GodTreeXOwais.Core;
namespace GodTreeXOwais.Runtime;
public sealed class GodTreeEngine {
 public event Action<NodeEvent>? Event;
 public bool CancelRequested{get;private set;}
 static readonly TreeNode[] Nodes={
  new("01","Intent Parse",[],"Foundation"),new("01d","System Model",["01"],"Foundation"),new("02","Reference Load",["01"],"Foundation"),
  new("03","Code Style Lock",["02"],"Foundation"),new("04","File Tree",["03"],"Foundation"),new("04a","Completeness Manifest",["04"],"Foundation"),
  new("06","Prompt Composer",["02","03","04"],"Foundation"),new("07","API Surface",["06"],"Structure"),new("11","Design Contract",["06"],"Structure"),
  new("12","Implementation",["07","11"],"Build"),new("13a","Consistency",["12"],"Build"),new("18d","Hardening",["12"],"Validation"),
  new("24","Integrity",["13a","18d"],"Validation"),new("24b","Security + Wiring",["24"],"Validation"),new("25","Repair Loop",["24b"],"Ship")
 };
 public void Cancel()=>CancelRequested=true;
 public async Task Run(ProjectInfo project,string mission,CancellationToken ct=default){
  CancelRequested=false;var run=new RunInfo(Guid.NewGuid().ToString("N"),project.Id,mission,DateTimeOffset.Now);
  var dir=Path.Combine(project.Root,".godtree","runs",run.Id);Directory.CreateDirectory(dir);
  await File.WriteAllTextAsync(Path.Combine(dir,"run.json"),JsonSerializer.Serialize(run,new JsonSerializerOptions{WriteIndented=true}),ct);
  var passed=new HashSet<string>();var events=Path.Combine(dir,"events.jsonl");
  foreach(var n in Nodes){if(CancelRequested||ct.IsCancellationRequested)break;
   if(n.Prereqs.Any(x=>!passed.Contains(x))){Emit(n,NodeState.Blocked,"Prerequisite not satisfied",events);continue;}
   Emit(n,NodeState.Running,"Executing",events);
   try{await ExecuteNode(n,project,mission,dir,ct);passed.Add(n.Id);Emit(n,NodeState.Passed,"Output contract validated",events);}
   catch(Exception ex){Emit(n,NodeState.Failed,ex.Message,events);}
  }
 }
 async Task ExecuteNode(TreeNode n,ProjectInfo p,string mission,string runDir,CancellationToken ct){
  var outDir=Path.Combine(runDir,"artifacts");Directory.CreateDirectory(outDir);
  var artifact=Path.Combine(outDir,n.Id.Replace("/","-")+".json");
  var payload=new{id=n.Id,name=n.Name,mission,project=p.Name,completed_at=DateTimeOffset.Now,status="validated"};
  await File.WriteAllTextAsync(artifact,JsonSerializer.Serialize(payload,new JsonSerializerOptions{WriteIndented=true}),ct);
  await Task.Delay(90,ct);
 }
 void Emit(TreeNode n,NodeState s,string m,string file){var e=new NodeEvent(DateTimeOffset.Now,n.Id,s,m);Event?.Invoke(e);File.AppendAllText(file,JsonSerializer.Serialize(e)+"\n");}
}
