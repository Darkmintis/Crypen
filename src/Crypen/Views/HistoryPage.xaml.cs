using Crypen.Core.Models;
using Crypen.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Crypen.Views;

/// <summary>
/// History page showing all encrypted items
/// </summary>
public sealed partial class HistoryPage : Page
{
    private AppService _appService;
    private List<EncryptedItem> _allItems;
    
    public HistoryPage()
    {
        this.InitializeComponent();
        
        // Get the app service
        _appService = App.Current.Services.GetRequiredService<AppService>();
        _allItems = new List<EncryptedItem>();
    }
    
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        LoadItems();
    }
    
    private void LoadItems()
    {
        _allItems = _appService.GetEncryptedItems();
        ApplyFilters();
    }
    
    private void ApplyFilters()
    {
        IEnumerable<EncryptedItem> filteredItems = _allItems;
        
        // Apply type filter
        if (TypeFilterComboBox.SelectedIndex > 0)
        {
            EncryptedItemType typeFilter = (EncryptedItemType)(TypeFilterComboBox.SelectedIndex - 1);
            filteredItems = filteredItems.Where(item => item.ItemType == typeFilter);
        }
        
        // Apply status filter
        if (StatusFilterComboBox.SelectedIndex > 0)
        {
            string statusFilter = StatusFilterComboBox.SelectedIndex == 1 ? "Encrypted" : "Decrypted";
            filteredItems = filteredItems.Where(item => item.Status == statusFilter);
        }
        
        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            string searchTerm = SearchBox.Text.ToLowerInvariant();
            filteredItems = filteredItems.Where(item => 
                item.Name.ToLowerInvariant().Contains(searchTerm) ||
                item.Path.ToLowerInvariant().Contains(searchTerm));
        }
        
        // Update the grid
        HistoryGrid.ItemsSource = filteredItems.ToList();
    }
    
    private void FilterControls_Changed(object sender, object e)
    {
        ApplyFilters();
    }
    
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadItems();
    }
    
    private void DecryptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string path)
        {
            App.Current.Window.ShowDecryptDialog(path);
            
            // Refresh the list after a short delay
            _ = Task.Delay(500).ContinueWith(_ => Dispatcher.TryEnqueue(() => LoadItems()));
        }
    }
    
    private async void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid itemId)
        {
            // Ask for confirmation
            ContentDialog dialog = new ContentDialog
            {
                Title = "Remove Item",
                Content = "Are you sure you want to remove this item from the history? This will not delete the encrypted file.",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            
            ContentDialogResult result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                _appService.RemoveEncryptedItem(itemId);
                LoadItems();
            }
        }
    }
    
    private void HistoryGrid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (HistoryGrid.SelectedItem is EncryptedItem item)
        {
            if (item.Status == "Encrypted")
            {
                App.Current.Window.ShowDecryptDialog(item.Path);
                
                // Refresh the list after a short delay
                _ = Task.Delay(500).ContinueWith(_ => Dispatcher.TryEnqueue(() => LoadItems()));
            }
        }
    }
}
