using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ADOPipelinesToaster.Models;
using ADOPipelinesToaster.Services;

namespace ADOPipelinesToaster.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _current;
    private readonly ObservableCollection<ExcludedPipeline> _excluded;

    public SettingsWindow(AppSettings current, SettingsService settingsService)
    {
        InitializeComponent();
        _current = current;
        _settingsService = settingsService;
        _excluded = new ObservableCollection<ExcludedPipeline>(current.ExcludedPipelines);

        TxtOrgUrl.Text = current.OrganizationUrl;
        TxtProject.Text = current.ProjectName;
        TxtPat.Password = current.PatToken;
        TxtPollInterval.Text = current.PollIntervalSeconds.ToString();
        TxtPipelineCount.Text = current.PipelineCount.ToString();
        ChkAutoStart.IsChecked = App.Current.GetAutoStart();
        ChkAlwaysOnTop.IsChecked = current.AlwaysOnTop;
        LstExcluded.ItemsSource = _excluded;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(TxtOrgUrl.Text) ||
            string.IsNullOrWhiteSpace(TxtProject.Text) ||
            string.IsNullOrWhiteSpace(TxtPat.Password))
        {
            TxtError.Text = "Organization URL, Project Name, and PAT Token are required.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }

        if (!int.TryParse(TxtPollInterval.Text, out var interval) || interval < 10)
        {
            TxtError.Text = "Poll interval must be a number \u2265 10 seconds.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }

        if (!int.TryParse(TxtPipelineCount.Text, out var pipelineCount) || pipelineCount < 1)
        {
            TxtError.Text = "Number of pipelines must be a number \u2265 1.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }

        var updated = new AppSettings
        {
            OrganizationUrl = TxtOrgUrl.Text.Trim(),
            ProjectName = TxtProject.Text.Trim(),
            PatToken = TxtPat.Password,
            PollIntervalSeconds = interval,
            PipelineCount = pipelineCount,
            AlwaysOnTop = ChkAlwaysOnTop.IsChecked == true,
            ExcludedPipelines = [.. _excluded],
        };

        await _settingsService.SaveAsync(updated);
        App.Current.SetAutoStart(ChkAutoStart.IsChecked == true);
        App.Current.ReinitializeAdoService(updated);
        DialogResult = true;
        Close();
    }

    private void OnRemoveExcluded(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ExcludedPipeline entry })
            _excluded.Remove(entry);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
