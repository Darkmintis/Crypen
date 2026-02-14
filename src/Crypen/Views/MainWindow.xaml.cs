using Crypen.Dialogs;
using Crypen.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace Crypen.Views;

/// <summary>
/// The main window of the application
/// </summary>
public sealed partial class MainWindow : Window
{
    private AppService _appService;
    private KeyboardShortcutHandler _keyboardHandler;
    private bool _hasRegisteredExtensions = false;
    
    // Content pages
    private DashboardPage? _dashboardPage;
    private EncryptPage? _encryptPage;
    private HistoryPage? _historyPage;
    private SettingsPage? _settingsPage;
    
    public MainWindow()
    {
        this.InitializeComponent();
        
        // Set title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(NavView);
        
        // Get app service
        _appService = App.Current.Services.GetRequiredService<AppService>();
        
        // Set up keyboard shortcuts
        _keyboardHandler = new KeyboardShortcutHandler(this);
        this.Content.KeyDown += Content_KeyDown;
        
        // Set initial page
        NavigateTo("dashboard");
    }
    
    private void Content_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        _keyboardHandler.HandleKeyDown(e);
    }
    
    // Public navigation methods for keyboard shortcuts
    public void NavigateToDashboard() => NavigateTo("dashboard");
    public void NavigateToEncryptPage() => NavigateTo("encrypt");
    public void NavigateToHistory() => NavigateTo("history");
    public void NavigateToSettings() => NavigateTo("settings");
    
    public void RefreshCurrentPage()
    {
        // Trigger a refresh on the current page if it supports it
        if (ContentFrame.Content is DashboardPage dashboardPage)
        {
            // Dashboard has a LoadRecentItems method but it's private
            // We could trigger a navigation refresh
            ContentFrame.Navigate(typeof(DashboardPage));
        }
        else if (ContentFrame.Content is HistoryPage historyPage)
        {
            ContentFrame.Navigate(typeof(HistoryPage));
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            string tag = item.Tag?.ToString() ?? string.Empty;
            NavigateTo(tag);
        }
    }
    
    private void NavigateTo(string tag)
    {
        switch (tag.ToLowerInvariant())
        {
            case "dashboard":
                _dashboardPage ??= new DashboardPage();
                ContentFrame.Navigate(typeof(DashboardPage));
                break;
                
            case "encrypt":
                _encryptPage ??= new EncryptPage();
                ContentFrame.Navigate(typeof(EncryptPage));
                break;
                
            case "history":
                _historyPage ??= new HistoryPage();
                ContentFrame.Navigate(typeof(HistoryPage));
                break;
                
            case "settings":
                _settingsPage ??= new SettingsPage();
                ContentFrame.Navigate(typeof(SettingsPage));
                break;
        }
    }
    
    /// <summary>
    /// Shows the encryption dialog for a file
    /// </summary>
    /// <param name="filePath">Path to the file to encrypt</param>
    public async void ShowEncryptFileDialog(string filePath)
    {
        if (!_hasRegisteredExtensions)
        {
            EnsureExtensionsRegistered();
        }
        
        // Create dialog
        ContentDialog dialog = new ContentDialog
        {
            Title = "Encrypt File",
            Content = new EncryptDialog(filePath, EncryptionItemType.File),
            PrimaryButtonText = "Encrypt",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };
        
        // Show dialog
        ContentDialogResult result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var dialogContent = dialog.Content as EncryptDialog;
            if (dialogContent != null)
            {
                bool success = await _appService.EncryptFileAsync(
                    filePath, 
                    dialogContent.Password, 
                    dialogContent.RememberPassword);
                
                if (success)
                {
                    ShowSuccessMessage("File encrypted successfully!");
                }
                else
                {
                    ShowErrorMessage("Failed to encrypt file.");
                }
            }
        }
    }
    
    /// <summary>
    /// Shows the encryption dialog for a directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory to encrypt</param>
    public async void ShowEncryptDirectoryDialog(string directoryPath)
    {
        if (!_hasRegisteredExtensions)
        {
            EnsureExtensionsRegistered();
        }
        
        // Create dialog
        ContentDialog dialog = new ContentDialog
        {
            Title = "Encrypt Folder",
            Content = new EncryptDialog(directoryPath, EncryptionItemType.Directory),
            PrimaryButtonText = "Encrypt",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };
        
        // Show dialog
        ContentDialogResult result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var dialogContent = dialog.Content as EncryptDialog;
            if (dialogContent != null)
            {
                bool success = await _appService.EncryptDirectoryAsync(
                    directoryPath, 
                    dialogContent.Password, 
                    dialogContent.RememberPassword);
                
                if (success)
                {
                    ShowSuccessMessage("Folder encrypted successfully!");
                }
                else
                {
                    ShowErrorMessage("Failed to encrypt folder.");
                }
            }
        }
    }
    
    /// <summary>
    /// Shows the encryption dialog for a USB drive
    /// </summary>
    /// <param name="driveLetter">Drive letter of the USB drive to encrypt</param>
    public async void ShowEncryptDriveDialog(string driveLetter)
    {
        if (!_hasRegisteredExtensions)
        {
            EnsureExtensionsRegistered();
        }
        
        // Create dialog
        ContentDialog dialog = new ContentDialog
        {
            Title = "Encrypt USB Drive",
            Content = new EncryptDialog(driveLetter, EncryptionItemType.UsbDrive),
            PrimaryButtonText = "Encrypt",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };
        
        // Show dialog
        ContentDialogResult result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var dialogContent = dialog.Content as EncryptDialog;
            if (dialogContent != null)
            {
                bool success = await _appService.EncryptUsbDriveAsync(
                    driveLetter, 
                    dialogContent.Password, 
                    dialogContent.RememberPassword);
                
                if (success)
                {
                    ShowSuccessMessage("USB drive encrypted successfully!");
                }
                else
                {
                    ShowErrorMessage("Failed to encrypt USB drive.");
                }
            }
        }
    }
    
    /// <summary>
    /// Shows the decryption dialog for an encrypted item
    /// </summary>
    /// <param name="path">Path to the encrypted item</param>
    public async void ShowDecryptDialog(string path)
    {
        if (!_hasRegisteredExtensions)
        {
            EnsureExtensionsRegistered();
        }
        
        // Check if the item is encrypted
        bool isEncrypted = await _appService.IsEncryptedItem(path);
        if (!isEncrypted)
        {
            ShowErrorMessage("This file is not encrypted with Crypen.");
            return;
        }
        
        // Check if we have a stored password
        string? storedPassword = _appService.GetStoredPassword(path);
        if (storedPassword != null)
        {
            // Use stored password to decrypt
            bool success = await _appService.DecryptAsync(path, storedPassword);
            
            if (success)
            {
                ShowSuccessMessage("Item decrypted successfully!");
                return;
            }
        }
        
        // Create dialog for password entry
        ContentDialog dialog = new ContentDialog
        {
            Title = "Decrypt Item",
            Content = new DecryptDialog(path),
            PrimaryButtonText = "Decrypt",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };
        
        // Show dialog
        ContentDialogResult result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var dialogContent = dialog.Content as DecryptDialog;
            if (dialogContent != null)
            {
                bool success = await _appService.DecryptAsync(
                    path, 
                    dialogContent.Password,
                    dialogContent.OverwriteExisting);
                
                if (success)
                {
                    ShowSuccessMessage("Item decrypted successfully!");
                }
                else
                {
                    ShowErrorMessage("Failed to decrypt item. Wrong password or corrupted file.");
                }
            }
        }
    }
    
    private void EnsureExtensionsRegistered()
    {
        var registryService = App.Current.Services.GetRequiredService<Core.Services.RegistryService>();
        registryService.Register();
        _hasRegisteredExtensions = true;
    }
    
    private async void ShowSuccessMessage(string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = "Success",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };
        
        await dialog.ShowAsync();
    }
    
    private async void ShowErrorMessage(string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };
        
        await dialog.ShowAsync();
    }
}
