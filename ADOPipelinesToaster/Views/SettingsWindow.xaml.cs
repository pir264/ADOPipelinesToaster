using System.Windows;
using ADOPipelinesToaster.Models;
using ADOPipelinesToaster.Services;

namespace ADOPipelinesToaster.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _current;

    public SettingsWindow(AppSettings current, SettingsService settingsService)
    {
        InitializeComponent();
        _current = current;
        _settingsService = settingsService;

        TxtOrgUrl.Text = current.OrganizationUrl;
        TxtProject.Text = current.ProjectName;
        TxtPat.Password = current.PatToken;
        TxtPollInterval.Text = current.PollIntervalSeconds.ToString();
        ChkAutoStart.IsChecked = App.Current.GetAutoStart();
        ChkAlwaysOnTop.IsChecked = current.AlwaysOnTop;
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

        var updated = new AppSettings
        {
            OrganizationUrl = TxtOrgUrl.Text.Trim(),
            ProjectName = TxtProject.Text.Trim(),
            PatToken = TxtPat.Password,
            PollIntervalSeconds = interval,
            AlwaysOnTop = ChkAlwaysOnTop.IsChecked == true,
        };

        await _settingsService.SaveAsync(updated);
        App.Current.SetAutoStart(ChkAutoStart.IsChecked == true);
        App.Current.ReinitializeAdoService(updated);
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
