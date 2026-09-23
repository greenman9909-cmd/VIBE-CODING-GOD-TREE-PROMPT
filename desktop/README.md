# GodTreeXOwais Desktop
Native Windows WinForms shell for the God Tree. It creates a local workspace, captures build missions, runs the visible orchestration stages, exports an AI-agent handoff, opens the workspace, and can commit workspace changes with Git.

## Build
`dotnet publish GodTreeXOwais.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist`

GitHub Actions builds `GodTreeXOwais.exe` automatically and uploads it as the **GodTreeXOwais-Windows-x64** artifact.
