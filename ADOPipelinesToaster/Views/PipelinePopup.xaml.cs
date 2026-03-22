using System.Windows;
using ADOPipelinesToaster.ViewModels;

namespace ADOPipelinesToaster.Views;

public partial class PipelinePopup : Window
{
    public PipelineStatusViewModel ViewModel { get; } = new();

    public PipelinePopup()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    // Position popup near the bottom-right (above the taskbar)
    public void PositionNearTray()
    {
        var screen = SystemParameters.WorkArea;
        Left = screen.Right - Width - 12;
        Top = screen.Bottom - ActualHeight - 12;
    }

    protected override void OnDeactivated(System.EventArgs e)
    {
        base.OnDeactivated(e);
        Hide();
    }
}
