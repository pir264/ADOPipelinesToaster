# ADOPipelinesToaster — Claude Instructions

## Project Overview

A Windows system tray application written in C# that monitors Azure DevOps (ADO) pipelines. The user clicks the tray icon to see their most recent pipeline runs (one row per pipeline definition), including individual stage statuses. Dark-themed popup UI.

## Goals & Scope

- Personal-use tool (single user)
- Single ADO organization + single project (configurable in the app)
- Authentication via Personal Access Token (PAT), stored in app settings
- Poll ADO every 60 seconds for pipeline updates
- Show the N most recently started pipeline runs triggered by the authenticated user (one row per pipeline definition, both running and completed)
- Show individual stage statuses per run as colored chips; stages link to ADO build logs for that stage
- Exclude specific pipelines from the popup (managed via ✕ button in popup; removable in Settings)
- Warn when someone else starts a newer run on the same pipeline definition
- Toast notifications for pipeline completion/failure (future phase)

## Recommended Tech Stack

- **UI Framework:** WPF (Windows Presentation Foundation)
- **System tray:** `Hardcodet.NotifyIcon.Wpf` NuGet package
- **ADO API:** Azure DevOps REST API via `HttpClient` + PAT token (Basic auth header)
  - Avoid the heavy TFS client SDK — raw REST is simpler and sufficient
- **Settings persistence:** `System.Text.Json` — store config in a local JSON file
- **Target framework:** .NET 8 (Windows)

## Architecture

```
ADOPipelinesToaster/
├── App.xaml / App.xaml.cs         # App entry point, tray icon setup, polling loop
├── Models/
│   ├── PipelineRun.cs             # Pipeline run data (id, name, status, stages)
│   ├── PipelineStage.cs           # Stage data (name, status, result, WebUrl)
│   ├── ExcludedPipeline.cs        # {Id, Name} pair for excluded pipeline definitions
│   └── AppSettings.cs             # Config model (org, project, PAT, exclusions)
├── Services/
│   ├── AdoService.cs              # ADO REST API calls (get runs, get stages)
│   └── SettingsService.cs         # Load/save settings from JSON
├── ViewModels/
│   └── PipelineStatusViewModel.cs # Data for the popup window
├── Views/
│   ├── PipelinePopup.xaml         # Popup shown on tray icon click
│   └── SettingsWindow.xaml        # Config window (org, project, PAT, exclusions)
└── TrayIcon/
    └── TaskbarIcon.xaml           # Tray icon definition (Hardcodet)
```

## Coding Conventions

- **Async/await** everywhere for I/O (HTTP calls, file access) — never block the UI thread
- No dependency injection framework — keep it simple with direct instantiation
- No unit tests in the initial phase — focus on getting it working first
- C# naming: PascalCase for classes/methods/properties, camelCase for local variables
- Keep classes small and focused on one responsibility
- Use `CancellationToken` for the polling loop so it can be cleanly stopped
- In WPF `DockPanel`, all `DockPanel.Dock="Right"` elements must be declared **before** the left-filling element, otherwise they get hidden

## Key ADO REST API Endpoints

Base URL: `https://dev.azure.com/{organization}/{project}/_apis`

- **Get builds by requester (running):**
  `GET /build/builds?$top=N&statusFilter=inProgress&queryOrder=startTimeDescending&requestedFor={email}&api-version=7.1`

- **Get builds by requester (completed):**
  `GET /build/builds?$top=N&statusFilter=completed&queryOrder=startTimeDescending&requestedFor={email}&api-version=7.1`
  — Both calls are made in parallel and merged. The default `statusFilter` is `inProgress` only, so both must be called explicitly to show completed runs.

- **Get current user identity:**
  `GET https://dev.azure.com/{org}/_apis/connectionData`
  — Parse `authenticatedUser.properties.Account.$value` for the email used in `requestedFor`.

- **Get timeline (stages) for a build:**
  `GET /build/builds/{buildId}/timeline?api-version=7.1`
  — Filter records by `type == "Stage"`. Stage `WebUrl` is built as `{buildWebUrl}&view=logs&j={stageId}`.

- **Auth header:** `Authorization: Basic {Base64(":" + PAT)}`

## Pipeline Fetching Logic

1. Fetch `inProgress` and `completed` builds in parallel (both filtered by `requestedFor`)
2. Merge results, group by `definition.id`, take the most recent run per definition
3. Filter out any definition IDs in `AppSettings.ExcludedPipelines`
4. Take the top N by `startTime` descending (N = `PipelineCount` setting)

## Settings Storage

Store settings in: `%AppData%\ADOPipelinesToaster\settings.json`

```json
{
  "OrganizationUrl": "https://dev.azure.com/myorg",
  "ProjectName": "MyProject",
  "PatToken": "...",
  "PollIntervalSeconds": 60,
  "PipelineCount": 2,
  "AlwaysOnTop": true,
  "ExcludedPipelines": [
    { "Id": 42, "Name": "My Pipeline" }
  ]
}
```

## Popup UI Layout Notes

- Dark theme: background `#1E1E1E`, cards `#2D2D2D`
- Each pipeline row shows: pipeline name (hyperlink) | result dot (colored circle, tooltip = result text) | ⚠ icon (tooltip = who started newer run) | ✕ exclude button
- Stage chips show the stage name as a hyperlink to ADO logs, colored by result/status
- Running stages show a spinning ⟳ icon
- The overall build `Result` field (succeeded/failed/etc.) is only populated once the build finishes — do not confuse with stage-level status

