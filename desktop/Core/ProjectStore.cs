using System.Text.Json;
namespace GodTreeXOwais.Core;
public sealed class ProjectStore {
 public string Home {get;}=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GodTreeXOwais");
 public ProjectStore(){Directory.CreateDirectory(Home);Directory.CreateDirectory(Path.Combine(Home,"projects"));}
 public ProjectInfo Create(string name,string? source=null){var id=DateTime.Now.ToString("yyyyMMddHHmmss");var root=source??Path.Combine(Home,"projects",id);Directory.CreateDirectory(root);Directory.CreateDirectory(Path.Combine(root,".godtree","runs"));var p=new ProjectInfo(id,name,root,DateTimeOffset.Now);File.WriteAllText(Path.Combine(root,".godtree","project.json"),JsonSerializer.Serialize(p,new JsonSerializerOptions{WriteIndented=true}));return p;}
 public IEnumerable<ProjectInfo> List(){foreach(var f in Directory.EnumerateFiles(Path.Combine(Home,"projects"),"project.json",SearchOption.AllDirectories)){ProjectInfo? p=null;try{p=JsonSerializer.Deserialize<ProjectInfo>(File.ReadAllText(f));}catch{}if(p!=null)yield return p;}}
}
