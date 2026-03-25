# ADO Pipelines Toaster

A lightweight Windows system tray app that keeps an eye on your Azure DevOps pipelines — without needing a browser open.

## What it does

Click the tray icon to see a popup showing your most recently started pipelines. Each row shows the pipeline name, its individual stages (color-coded by status), and a result indicator once it finishes. Running stages show a spinning icon. Completed stages link directly to the ADO build logs for that stage.

The app polls ADO every 60 seconds (configurable) so the popup is always up to date.

### Popup at a glance

| Element | Meaning |
|---|---|
| Pipeline name (blue link) | Opens the build in your browser |
| Colored stage chips | Green = succeeded, Red = failed, Blue = running |
| Stage name (link) | Opens the logs for that specific stage in ADO |
| Colored dot (right side) | Overall build result — only shown once the build finishes |
| ⚠ icon | Someone else started a newer run on this pipeline |
| ✕ button | Exclude this pipeline from the popup |

## Features

- **One row per pipeline definition** — shows the most recent run (running or completed)
- **Running + completed** — you see a pipeline whether it's in progress or just finished
- **Stage-level detail** — see exactly which stage passed or failed
- **Exclude pipelines** — hide pipelines you don't care about via the ✕ button; manage the list in Settings
- **Newer run warning** — get notified when a colleague starts a run on the same pipeline after you
- **Configurable count** — choose how many pipelines to watch at once
- **Always-on-top popup** — optional, so it floats above other windows
- **Auto-start with Windows** — optional

## Setup

### Prerequisites

- Windows 10/11
- .NET 8 runtime (or build from source)
- An Azure DevOps Personal Access Token (PAT) with **Build (Read)** permission

### First run

1. Start the app — it appears in the system tray
2. Right-click the tray icon → **Settings**
3. Fill in:
   - **Organization URL** — e.g. `https://dev.azure.com/myorg`
   - **Project Name** — e.g. `MyProject`
   - **Personal Access Token** — generate one in ADO under User Settings → Personal Access Tokens
4. Click **Save**

The app starts polling immediately.

### Creating a PAT in Azure DevOps

1. Go to `https://dev.azure.com/{yourorg}` → User Settings (top right) → Personal Access Tokens
2. Click **New Token**
3. Set scope to **Build → Read**
4. Copy the token and paste it into the Settings window

## Settings

| Setting | Default | Description |
|---|---|---|
| Organization URL | — | Your ADO org URL |
| Project Name | — | The ADO project to monitor |
| Personal Access Token | — | PAT with Build (Read) scope |
| Poll Interval | 60s | How often to check for updates (minimum 10s) |
| Number of pipelines | 2 | How many pipeline definitions to show |
| Always on top | On | Popup floats above other windows |
| Start with Windows | Off | Launch automatically on login |
| Excluded Pipelines | — | Pipelines hidden from the popup (remove here to restore) |

## Excluding a pipeline

Click the **✕** button on any pipeline row in the popup. The pipeline disappears immediately and won't show up again until you remove it from the exclusion list in Settings.

## Building from source

```
git clone https://github.com/yourname/ADOPipelinesToaster
cd ADOPipelinesToaster
dotnet build
dotnet run --project ADOPipelinesToaster
```

Requires Visual Studio 2022 or the .NET 8 SDK with Windows targeting.

## Settings file location

`%AppData%\ADOPipelinesToaster\settings.json`

Your PAT is stored in plain text in this file. Keep it private.
