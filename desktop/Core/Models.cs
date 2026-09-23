using System.Text.Json.Serialization;
namespace GodTreeXOwais.Core;
public enum NodeState { Queued, Running, Passed, Failed, Blocked }
public sealed record TreeNode(string Id,string Name,string[] Prereqs,string Tier);
public sealed record NodeEvent(DateTimeOffset At,string NodeId,NodeState State,string Message);
public sealed record ProjectInfo(string Id,string Name,string Root,DateTimeOffset CreatedAt);
public sealed record RunInfo(string Id,string ProjectId,string Mission,DateTimeOffset StartedAt);
