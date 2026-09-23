using System.Diagnostics;
namespace GodTreeXOwais.Runtime;
public sealed record ToolResult(int ExitCode,string Stdout,string Stderr,TimeSpan Duration);
public sealed class ProcessToolRunner{
 static readonly HashSet<string> Allowed=new(StringComparer.OrdinalIgnoreCase){"git","dotnet","node","npm","npx","python","python3","pwsh","powershell","cmd"};
 public async Task<ToolResult> Run(string executable,string arguments,string cwd,TimeSpan timeout,CancellationToken ct=default){
  if(!Allowed.Contains(Path.GetFileNameWithoutExtension(executable)))throw new InvalidOperationException("Tool is not allow-listed: "+executable);
  var sw=Stopwatch.StartNew();using var linked=CancellationTokenSource.CreateLinkedTokenSource(ct);linked.CancelAfter(timeout);
  using var p=new Process{StartInfo=new(executable,arguments){WorkingDirectory=cwd,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false,CreateNoWindow=true}};
  p.Start();var o=p.StandardOutput.ReadToEndAsync();var e=p.StandardError.ReadToEndAsync();
  try{await p.WaitForExitAsync(linked.Token);}catch(OperationCanceledException){try{p.Kill(true);}catch{}throw new TimeoutException($"Tool exceeded {timeout}.");}
  return new ToolResult(p.ExitCode,await o,await e,sw.Elapsed);
 }
}
