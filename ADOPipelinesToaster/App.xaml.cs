using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ADOPipelinesToaster.Models;
using ADOPipelinesToaster.Services;
using ADOPipelinesToaster.Views;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

namespace ADOPipelinesToaster;

public partial class App : Application
{
    private const string AutoStartKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AutoStartName = "ADOPipelinesToaster";

    private TaskbarIcon? _trayIcon;
    private SettingsService _settingsService = new();
    private AdoService? _adoService;
    private AppSettings _settings = new();
    private CancellationTokenSource _pollCts = new();
    private PipelinePopup? _popup;

    public List<PipelineRun> LatestRuns { get; private set; } = new();
    public string? LastError { get; private set; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.OrganizationUrl)
                              && !string.IsNullOrWhiteSpace(_settings.PatToken);

    public static new App Current => (App)Application.Current;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settings = await _settingsService.LoadAsync();

        var dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/TrayIcon/TaskbarIcon.xaml")
        };
        _trayIcon = (TaskbarIcon)dict["TrayIcon"];

        var contextMenu = _trayIcon.ContextMenu;
        WireMenuItem(contextMenu, "MenuStatus", OnStatusClicked);
        WireMenuItem(contextMenu, "MenuSettings", OnSettingsClicked);
        WireMenuItem(contextMenu, "MenuExit", OnExitClicked);

        _trayIcon.TrayLeftMouseUp += (_, _) => OnStatusClicked(null, null);

        if (!IsConfigured)
        {
            // Prompt user to configure on first launch
            LastError = "Not configured. Open Settings to enter your ADO details.";
            _trayIcon.ShowBalloonTip("ADO Pipelines Toaster",
                "Please configure your ADO settings.", BalloonIcon.Warning);
            OnSettingsClicked(null, null);
        }
        else
        {
            _adoService = new AdoService(_settings);
            StartPolling();
        }
    }

    private static void WireMenuItem(ContextMenu menu, string name, RoutedEventHandler handler)
    {
        foreach (var item in menu.Items)
        {
            if (item is MenuItem mi && mi.Name == name)
            {
                mi.Click += handler;
                return;
            }
        }
    }

    private void OnStatusClicked(object? sender, EventArgs? e)
    {
        if (_popup == null || !_popup.IsLoaded)
        {
            _popup = new PipelinePopup();
            _popup.Topmost = _settings.AlwaysOnTop;
            _popup.Show();
            _popup.PositionNearTray();
        }
        else if (_popup.IsVisible)
        {
            _popup.Hide();
            return;
        }
        else
        {
            _popup.Topmost = _settings.AlwaysOnTop;
            _popup.Show();
            _popup.PositionNearTray();
        }

        _popup.ViewModel.UpdateRuns(LatestRuns, LastError);
        _popup.Activate();
    }

    private void OnSettingsClicked(object? sender, EventArgs? e)
    {
        var win = new SettingsWindow(_settings, _settingsService);
        win.ShowDialog();
    }

    private void OnExitClicked(object? sender, EventArgs? e)
    {
        _pollCts.Cancel();
        _trayIcon?.Dispose();
        Shutdown();
    }

    public void StartPolling()
    {
        _pollCts.Cancel();
        _pollCts = new CancellationTokenSource();
        var ct = _pollCts.Token;

        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.PollIntervalSeconds));
            await PollAsync(ct);
            while (await timer.WaitForNextTickAsync(ct))
                await PollAsync(ct);
        }, ct);
    }

    private async Task PollAsync(CancellationToken ct)
    {
        if (_adoService == null) return;
        try
        {
            LatestRuns = await _adoService.GetRecentRunsAsync(ct);
            LastError = null;
            System.Diagnostics.Debug.WriteLine($"[Poll] Fetched {LatestRuns.Count} runs.");

            Dispatcher.Invoke(() =>
            {
                if (_popup?.IsVisible == true)
                    _popup.ViewModel.UpdateRuns(LatestRuns, LastError);
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LastError = ex.Message;
            System.Diagnostics.Debug.WriteLine($"[Poll] Error: {ex.Message}");

            Dispatcher.Invoke(() =>
            {
                if (_popup?.IsVisible == true)
                    _popup.ViewModel.UpdateRuns(LatestRuns, LastError);
            });
        }
    }

    public void ReinitializeAdoService(AppSettings newSettings)
    {
        _settings = newSettings;
        _adoService = new AdoService(newSettings);
        LastError = null;
        LatestRuns = new List<PipelineRun>();
        if (_popup != null) _popup.Topmost = newSettings.AlwaysOnTop;
        StartPolling();
    }

    // Auto-start helpers (used by SettingsWindow)
    public bool GetAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey);
        return key?.GetValue(AutoStartName) != null;
    }

    public void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey, writable: true);
        if (key == null) return;
        if (enable)
            key.SetValue(AutoStartName, $"\"{Environment.ProcessPath}\"");
        else
            key.DeleteValue(AutoStartName, throwOnMissingValue: false);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _pollCts.Cancel();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
