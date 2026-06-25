# AGENTS.md

Cody for Visual Studio — a Windows-only Visual Studio extension (.NET Framework 4.7.2) that
bundles a Node.js-based Cody agent. C# code lives in `src/`; the agent build scripts live in
`agent/`. See `CONTRIBUTING.md` for the full development setup.

## Layout

- `src/Cody.sln` — main solution.
- `src/Cody.Core/` — core library shared across the extension.
- `src/Cody.UI/` — UI components.
- `src/Cody.VisualStudio/` — the Visual Studio extension (VSIX) entry point.
- `src/Cody.VisualStudio.Completions/` — autocomplete component (built separately, see `src/build.ps1`).
- `src/Cody.Core.Tests/`, `src/Cody.VisualStudio.Tests/` — test projects.
- `agent/` — PowerShell scripts and version pin (`agent.version`) for building the Node.js agent.

## Setup

Requires Windows, Visual Studio (Pro), the .NET SDK, .NET Framework 4.7.2, Node.js, and
`pnpm` (the repo pins `8.6.7`). Configure Git with `git config core.autocrlf false` — the repo
uses Windows line endings.

## Build

The agent must be built before building or debugging the extension for the first time.

```powershell
# Build the Node.js agent (run from the agent/ directory or with the relative path below)
./agent/runBuildAgent.ps1

# Build the extension (configuration defaults to Debug)
./src/build.ps1 -configuration Debug
```

You can also open `src/Cody.sln` in Visual Studio and press `F5` to build and debug.

## Test

```powershell
# Run tests against the built test assemblies (matches CI)
dotnet test src/*Tests/bin/Debug/*.Tests.dll --logger:trx --verbosity detailed
```

Integration/UI tests can also be run from Visual Studio's Test Explorer (right-click
`Cody.VisualStudio.Tests` → Run). UI tests require an access token; see `CONTRIBUTING.md`.

## Lint / Format

CI enforces formatting with `dotnet format`. Verify locally before committing:

```powershell
dotnet format ./src/ --verify-no-changes -v:diag
```

Style rules are defined in `src/.editorconfig`.

## Conventions

- Developer behavior (tracing, agent debug logs, remote agent port) is controlled via
  `src/CodyDevConfig.json`; this file is stripped from release builds.
- During development you can supply your own Cody token via the `SourcegraphCodyToken`
  environment variable, which overrides user settings and is never persisted.
- CI (`.github/workflows/build.yml`) builds the agent, then runs `src/build.ps1`, then tests.
