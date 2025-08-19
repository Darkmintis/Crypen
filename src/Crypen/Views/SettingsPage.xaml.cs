using Crypen.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Crypen.Views;

/// <summary>
/// Settings page for the application
/// </summary>
public sealed partial class SettingsPage : Page
{
    private RegistryService _registryService;
    
    public SettingsPage()
    {
        this.InitializeComponent();
        
        // Get the registry service
        _registryService = App.Current.Services.GetRequiredService<RegistryService>();
        
        // Set version
        PackageVersion version = Package.Current.Id.Version;
        VersionText.Text = $"{version.Major}.{version.Minor}.{version.Build}";
    }
    
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize settings UI
        // In a real app, these would be loaded from user settings
        RightClickToggle.IsOn = true;
        RememberPasswordsToggle.IsOn = true;
    }

    private void RightClickToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (RightClickToggle.IsOn)
        {
            _registryService.Register();
        }
        else
        {
            _registryService.Unregister();
        }
    }

    private void RememberPasswordsToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // In a real app, this would save the setting
    }

    private async void ClearPasswordsButton_Click(object sender, RoutedEventArgs e)
    {
        // Ask for confirmation
        ContentDialog dialog = new ContentDialog
        {
            Title = "Clear All Passwords",
            Content = "Are you sure you want to clear all stored passwords? This action cannot be undone.",
            PrimaryButtonText = "Clear Passwords",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        
        ContentDialogResult result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            // In a real app, this would clear all stored passwords
            
            // Show confirmation
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Passwords Cleared",
                Content = "All stored passwords have been cleared.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            
            await confirmDialog.ShowAsync();
        }
    }
    
    private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        // In a real app, this would check for updates
        
        // Show message
        ContentDialog dialog = new ContentDialog
        {
            Title = "Check for Updates",
            Content = "You are using the latest version of Crypen.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        
        await dialog.ShowAsync();
    }
}
