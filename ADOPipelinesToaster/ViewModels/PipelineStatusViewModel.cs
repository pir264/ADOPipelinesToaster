using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ADOPipelinesToaster.Models;

namespace ADOPipelinesToaster.ViewModels;

public class PipelineStatusViewModel : INotifyPropertyChanged
{
    private ObservableCollection<PipelineRun> _runs = new();
    private string _statusMessage = "Loading...";
    private bool _isLoading = true;

    public ObservableCollection<PipelineRun> Runs
    {
        get => _runs;
        set { _runs = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasContent)); }
    }

    public bool HasContent => !_isLoading;

    public void UpdateRuns(System.Collections.Generic.List<PipelineRun> runs, string? error)
    {
        IsLoading = false;
        Runs.Clear();
        if (error != null)
        {
            StatusMessage = $"Error: {error}";
            return;
        }
        if (runs.Count == 0)
        {
            StatusMessage = "No recent pipeline runs found.";
            return;
        }
        StatusMessage = string.Empty;
        foreach (var run in runs)
            Runs.Add(run);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
