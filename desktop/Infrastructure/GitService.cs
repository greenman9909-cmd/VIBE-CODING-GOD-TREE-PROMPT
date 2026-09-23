using System.Diagnostics;
namespace GodTreeXOwais.Infrastructure;
public sealed class GitService {
 public async Task<string> Run(string root,string args){var p=new Process{StartInfo=new("git",args){WorkingDirectory=root,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false,CreateNoWindow=true}};p.Start();var a=p.StandardOutput.ReadToEndAsync();var b=p.StandardError.ReadToEndAsync();await p.WaitForExitAsync();return ((await a)+"\n"+(await b)).Trim();}
 public Task<string> Status(string root)=>Run(root,"status --short");
 public async Task<string> Commit(string root,string message){await Run(root,"add -A");return await Run(root,$"commit -m \"{message.Replace(""","'")}\"");}
}
