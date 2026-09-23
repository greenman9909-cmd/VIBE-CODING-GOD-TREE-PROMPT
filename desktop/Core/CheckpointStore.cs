using System.Text.Json;
namespace GodTreeXOwais.Core;
public sealed record Checkpoint(string NodeId,DateTimeOffset At,string[] Artifacts);
public sealed class CheckpointStore{
 public void Save(string runDir,Checkpoint c){var d=Path.Combine(runDir,"checkpoints");Directory.CreateDirectory(d);File.WriteAllText(Path.Combine(d,c.NodeId+".json"),JsonSerializer.Serialize(c,new JsonSerializerOptions{WriteIndented=true}));}
 public HashSet<string> Passed(string runDir)=>Directory.Exists(Path.Combine(runDir,"checkpoints"))?Directory.EnumerateFiles(Path.Combine(runDir,"checkpoints"),"*.json").Select(Path.GetFileNameWithoutExtension).ToHashSet() : new();
}
