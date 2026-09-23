namespace GodTreeXOwais.Runtime;
public sealed record ToolDefinition(string Id,string Executable,string Description,bool Enabled=true);
public sealed class ToolRegistry{
 readonly Dictionary<string,ToolDefinition> tools=new(StringComparer.OrdinalIgnoreCase);
 public ToolRegistry(){Register(new("git","git","Version control"));Register(new("dotnet","dotnet",".NET SDK"));Register(new("node","node","Node.js runtime"));Register(new("npm","npm","Node package manager"));Register(new("python","python","Python runtime"));}
 public IEnumerable<ToolDefinition> All=>tools.Values;
 public void Register(ToolDefinition t)=>tools[t.Id]=t;
 public ToolDefinition Get(string id)=>tools.TryGetValue(id,out var t)&&t.Enabled?t:throw new KeyNotFoundException("Tool unavailable: "+id);
}
