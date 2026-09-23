using System.Text.Json;
namespace GodTreeXOwais.Core;
public sealed class EventStore{
 public void Append(string path,object evt){Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.AppendAllText(path,JsonSerializer.Serialize(evt)+"\n");}
 public IEnumerable<T> Read<T>(string path){if(!File.Exists(path))yield break;foreach(var l in File.ReadLines(path)){T? x=default;try{x=JsonSerializer.Deserialize<T>(l);}catch{}if(x!=null)yield return x;}}
}
