# GodTreeXOwais — Full Desktop Architecture

## Product
A local-first Windows agentic engineering workstation. The desktop shell is only the presentation layer; the product is an orchestration runtime that turns a mission into an inspectable execution graph, runs tools/agents, persists artifacts, validates results, and exports a portable handoff.

## Layers
```
Desktop Shell (WinUI/WPF)
  ├─ Command Center
  ├─ Project Explorer
  ├─ Agent Graph
  ├─ Tool Registry
  ├─ Terminal / Logs
  ├─ Diff + Git
  ├─ Artifacts / Preview
  └─ Settings / Providers
        │
Application Core
  ├─ MissionService
  ├─ ProjectService
  ├─ RunService
  ├─ AgentService
  ├─ ToolService
  ├─ ArtifactService
  ├─ GitService
  └─ ExportService
        │
GodTree Runtime
  ├─ Tree Loader
  ├─ Dependency Resolver
  ├─ DAG Scheduler
  ├─ Context Compiler
  ├─ Model Router
  ├─ Tool Executor
  ├─ Checkpoint Engine
  ├─ Retry/Recovery Engine
  ├─ Policy/Permission Gate
  └─ Event Bus
        │
Adapters
  ├─ AI providers
  ├─ MCP servers
  ├─ CLI/process tools
  ├─ Git/GitHub
  ├─ browser/reference acquisition
  └─ filesystem
        │
Persistence
  ├─ SQLite state
  ├─ workspace files
  ├─ immutable run events
  ├─ artifacts
  └─ secrets via Windows Credential Manager
```

## Runtime entities
Project, Mission, Run, NodeDefinition, NodeRun, Agent, Tool, ToolInvocation, Artifact, Checkpoint, Event, Provider, SecretReference, GitSnapshot, ValidationReport.

## Run lifecycle
CREATED -> PLANNING -> READY -> RUNNING -> VALIDATING -> REPAIRING -> COMPLETED
                                            \-> BLOCKED
Any active state can enter FAILED or CANCELLED. Runs are resumable from checkpoints.

## Scheduler
Load machine-readable God Tree; resolve prerequisites; create DAG; execute independent nodes concurrently; stream structured events; collect artifacts; run wiring/integrity/security/completeness gates; invoke repair loop only for observed failures; checkpoint after every successful node.

## Tool execution contract
Every invocation contains id, run_id, node_id, tool_id, args, cwd, timeout, permissions, started_at. Every result contains exit_code/status, stdout/stderr references, structured output, produced artifacts, duration, error classification. No tool receives unrestricted filesystem/network/process access by default.

## Workspace
```
%LOCALAPPDATA%/GodTreeXOwais/
  config/
  db/godtree.db
  logs/
  cache/
  projects/<project-id>/
    project.json
    source/
    .godtree/
      mission.json
      plan.json
      runs/<run-id>/
        events.jsonl
        checkpoints/
        artifacts/
        reports/
        export/
```

## Desktop screens
1. Home — recent projects, create/import.
2. Command Center — mission prompt, mode, provider, run/stop/resume.
3. Agent Graph — live DAG with queued/running/passed/failed/blocked nodes.
4. Workspace — file tree + editor/diff + artifact preview.
5. Tools — installed/discovered tools, capabilities, permissions, health.
6. Runs — history, token/time/tool metrics, replay.
7. Git — status, diff, branch, commits, push/PR actions.
8. Export — portable context pack for Antigravity/Claude/Codex/other agents.
9. Settings — providers, MCP, GitHub, paths, security.

## Architecture rule
The UI never pretends a stage executed. It renders runtime events. A node is green only after its output contract validates. Git actions operate on the selected project repository, never the app data directory.

## Implementation phases
P0 foundation: solution split into Desktop/Core/Runtime/Infrastructure/Contracts/Tests.
P1 persistence + project/run state.
P2 DAG scheduler + event bus + checkpoints.
P3 process/MCP/model adapters + permission broker.
P4 complete desktop shell and graph.
P5 God Tree contracts/schemas + validators.
P6 Git/export/reference acquisition.
P7 installer, updater, signing-ready release pipeline, crash recovery, integration tests.
