# ADOPipelinesToaster — Claude Instructions

## Project Overview

A Windows system tray application written in C# that monitors Azure DevOps (ADO) pipelines. The user can click the tray icon to see the last 2 pipeline runs they triggered, including individual stage statuses. Windows toast notifications are planned for a later phase.

## Goals & Scope

- Personal-use tool (single user)
- Single ADO organization + single project (configurable in the app)
- Authentication via Personal Access Token (PAT), stored in app settings
- Poll ADO every 60 seconds for pipeline updates
- Show the 2 most recent pipeline runs triggered by the authenticated user
- Show individual stage statuses per run (e.g. Build: Succeeded, Deploy-Dev: Failed)
- Toast notifications for pipeline completion/failure (future phase)

## Recommended Tech Stack

- **UI Framework:** WPF (Windows Presentation Foundation)
- **System tray:** `Hardcodet.NotifyIcon.Wpf` NuGet package
- **ADO API:** Azure DevOps REST API via `HttpClient` + PAT token (Bearer/Basic auth header)
  - Avoid the heavy TFS client SDK — raw REST is simpler and sufficient
- **Settings persistence:** `System.Text.Json` — store config in a local JSON file (org URL, project name, PAT token)
- **Target framework:** .NET 8 (Windows)

## Architecture

```
ADOPipelinesToaster/
├── App.xaml / App.xaml.cs         # App entry point, tray icon setup
├── Models/
│   ├── PipelineRun.cs             # Pipeline run data (id, name, status, stages)
│   └── AppSettings.cs             # Config model (org, project, PAT)
├── Services/
│   ├── AdoService.cs              # ADO REST API calls (get runs, get stages)
│   └── SettingsService.cs         # Load/save settings from JSON
├── ViewModels/
│   └── PipelineStatusViewModel.cs # Data for the popup window
├── Views/
│   ├── PipelinePopup.xaml         # Popup shown on tray icon click
│   └── SettingsWindow.xaml        # Config window (org, project, PAT)
└── TrayIcon/
    └── TaskbarIcon.xaml           # Tray icon definition (Hardcodet)
```

## Coding Conventions

- **Async/await** everywhere for I/O (HTTP calls, file access) — never block the UI thread
- No dependency injection framework for now — keep it simple with direct instantiation
- No unit tests in the initial phase — focus on getting it working first
- C# naming: PascalCase for classes/methods/properties, camelCase for local variables
- Keep classes small and focused on one responsibility
- Use `CancellationToken` for the polling loop so it can be cleanly stopped

## Key ADO REST API Endpoints

Base URL: `https://dev.azure.com/{organization}/{project}/_apis`

- **List pipeline runs (filtered by requester):**
  `GET /pipelines/{pipelineId}/runs?api-version=7.1`
  — filter by `requestedFor.uniqueName` matching the PAT owner

- **Get all builds (easier for filtering by requester):**
  `GET /build/builds?requestedFor={userId}&$top=10&api-version=7.1`

- **Get timeline (stages) for a build:**
  `GET /build/builds/{buildId}/timeline?api-version=7.1`

- **Auth header:** `Authorization: Basic {Base64(":"  + PAT)}`

## Settings Storage

Store settings in: `%AppData%\ADOPipelinesToaster\settings.json`

```json
{
  "OrganizationUrl": "https://dev.azure.com/myorg",
  "ProjectName": "MyProject",
  "PatToken": "...",
  "PollIntervalSeconds": 60
}
```

## User Context

- The developer is an experienced C# developer but new to Windows desktop (WPF/WinForms) development
- Prefers async/await patterns
- Start simple — no over-engineering, get it working first
- Development happens on a Windows machine
