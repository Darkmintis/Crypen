using Crypen.Core.Models;
using Crypen.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Crypen.Views;

/// <summary>
/// Dashboard page showing recent items and quick actions
/// </summary>
public sealed partial class DashboardPage : Page
{
    private AppService _appService;
    
    public DashboardPage()
    {
        this.InitializeComponent();
        
        // Get the app service
        _appService = App.Current.Services.GetRequiredService<AppService>();
        
        // Load recent items
        LoadRecentItems();
    }
    
    private void LoadRecentItems()
    {
        var recentItems = _appService.GetEncryptedItems()
            .OrderByDescending(item => item.EncryptedAt)
            .Take(10)
            .ToList();
        
        RecentItemsGrid.ItemsSource = recentItems;
    }

    private async void EncryptFileButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add("*");
        
        // Initialize the picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Current.Window));
        
        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            App.Current.Window.ShowEncryptFileDialog(file.Path);
            
            // Refresh the recent items after a short delay
            await Task.Delay(500);
            LoadRecentItems();
        }
    }

    private async void EncryptFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        
        // Initialize the picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Current.Window));
        
        StorageFolder folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            App.Current.Window.ShowEncryptDirectoryDialog(folder.Path);
            
            // Refresh the recent items after a short delay
            await Task.Delay(500);
            LoadRecentItems();
        }
    }

    private async void EncryptUsbButton_Click(object sender, RoutedEventArgs e)
    {
        // Get available drives
        var drives = DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
            .ToList();
        
        if (drives.Count == 0)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "No USB Drives Found",
                Content = "No USB drives are connected to your computer.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            
            await dialog.ShowAsync();
            return;
        }
        
        // If there's only one drive, use it directly
        if (drives.Count == 1)
        {
            App.Current.Window.ShowEncryptDriveDialog(drives[0].Name[0].ToString());
            
            // Refresh the recent items after a short delay
            await Task.Delay(500);
            LoadRecentItems();
            return;
        }
        
        // If there are multiple drives, ask the user which one to encrypt
        ContentDialog driveDialog = new ContentDialog
        {
            Title = "Select USB Drive",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };
        
        // Create the content
        ListView driveList = new ListView
        {
            SelectionMode = ListViewSelectionMode.Single,
            ItemsSource = drives.Select(d => new { 
                Drive = d, 
                Display = $"{(string.IsNullOrEmpty(d.VolumeLabel) ? "USB Drive" : d.VolumeLabel)} ({d.Name})" 
            }).ToList()
        };
        
        driveList.ItemTemplate = (DataTemplate)Resources["DriveItemTemplate"];
        driveDialog.Content = driveList;
        
        driveDialog.PrimaryButtonText = "Select";
        driveDialog.PrimaryButtonClick += (s, args) =>
        {
            if (driveList.SelectedItem != null)
            {
                var drive = ((dynamic)driveList.SelectedItem).Drive as DriveInfo;
                App.Current.Window.ShowEncryptDriveDialog(drive.Name[0].ToString());
                
                // Refresh the recent items after a short delay
                _ = Task.Delay(500).ContinueWith(_ => Dispatcher.TryEnqueue(() => LoadRecentItems()));
            }
        };
        
        await driveDialog.ShowAsync();
    }
    
    private void DecryptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string path)
        {
            App.Current.Window.ShowDecryptDialog(path);
            
            // Refresh the recent items after a short delay
            _ = Task.Delay(500).ContinueWith(_ => Dispatcher.TryEnqueue(() => LoadRecentItems()));
        }
    }

    private void RecentItemsGrid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (RecentItemsGrid.SelectedItem is EncryptedItem item)
        {
            if (item.Status == "Encrypted")
            {
                App.Current.Window.ShowDecryptDialog(item.Path);
                
                // Refresh the recent items after a short delay
                _ = Task.Delay(500).ContinueWith(_ => Dispatcher.TryEnqueue(() => LoadRecentItems()));
            }
        }
    }
}
