using Avalonia.Controls;
using Avalonia.Interactivity;
using Moon_WiiVC_Injector.ViewModels;

namespace Moon_WiiVC_Injector;

internal sealed partial class NintendontConfigView : UserControl
{
    public NintendontConfigView()
    {
        InitializeComponent();
    }

    private async void OnAdvancedConfigurationChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggle || e.Source != toggle ||
            DataContext is not NintendontConfigViewModel config ||
            toggle.IsChecked is not bool enabled || enabled == config.IsAdvanced)
            return;

        try
        {
            await config.SetAdvancedAsync(enabled);
        }
        finally
        {
            toggle.SetCurrentValue(ToggleSwitch.IsCheckedProperty, config.IsAdvanced);
        }
    }
}
