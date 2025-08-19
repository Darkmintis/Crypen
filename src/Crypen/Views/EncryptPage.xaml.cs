using Crypen.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Crypen.Views;

/// <summary>
/// Page for encryption functionality
/// </summary>
public sealed partial class EncryptPage : Page
{
    private AppService _appService;
    private StorageFile? _selectedFile;
    private StorageFolder? _selectedFolder;
    
    public EncryptPage()
    {
        this.InitializeComponent();
        
        // Get the app service
        _appService = App.Current.Services.GetRequiredService<AppService>();
    }
    
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshUsbDrives();
    }
    
    private class UsbDriveInfo
    {
        public string DriveLetter { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string FreeSpace { get; set; } = string.Empty;
        public string TotalSize { get; set; } = string.Empty;
    }

    private async void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add("*");
        
        // Initialize the picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Current.Window));
        
        _selectedFile = await picker.PickSingleFileAsync();
        if (_selectedFile != null)
        {
            SelectedFileText.Text = _selectedFile.Path;
            
            // Show the encryption dialog
            App.Current.Window.ShowEncryptFileDialog(_selectedFile.Path);
        }
    }

    private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        
        // Initialize the picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Current.Window));
        
        _selectedFolder = await picker.PickSingleFolderAsync();
        if (_selectedFolder != null)
        {
            SelectedFolderText.Text = _selectedFolder.Path;
            
            // Show the encryption dialog
            App.Current.Window.ShowEncryptDirectoryDialog(_selectedFolder.Path);
        }
    }

    private void RefreshDrivesButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshUsbDrives();
    }
    
    private void RefreshUsbDrives()
    {
        // Get all removable drives
        var drives = DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
            .Select(d => new UsbDriveInfo
            {
                DriveLetter = d.Name[0].ToString(),
                DisplayName = !string.IsNullOrEmpty(d.VolumeLabel) ? d.VolumeLabel : $"USB Drive ({d.Name[0]}:)",
                Path = d.Name,
                FreeSpace = FormatSize(d.AvailableFreeSpace),
                TotalSize = FormatSize(d.TotalSize)
            })
            .ToList();
        
        UsbDrivesListView.ItemsSource = drives;
        
        // Show/hide the empty message
        NoDrivesTextBlock.Visibility = drives.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    
    private string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;
        
        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }
        
        return $"{size:0.##} {suffixes[suffixIndex]}";
    }
    
    private void UsbDrivesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // We don't need to do anything here, as we have a button for each item
    }
    
    private void EncryptDriveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string driveLetter)
        {
            // Show the encryption dialog
            App.Current.Window.ShowEncryptDriveDialog(driveLetter);
        }
    }
}
