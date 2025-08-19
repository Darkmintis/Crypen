using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace Crypen.Dialogs;

/// <summary>
/// Dialog for decrypting items
/// </summary>
public sealed partial class DecryptDialog : UserControl
{
    private readonly string _itemPath;
    
    public DecryptDialog(string itemPath)
    {
        this.InitializeComponent();
        
        _itemPath = itemPath;
        
        // Display the path
        string displayPath;
        if (itemPath.EndsWith(":\\"))
        {
            // This is a USB drive
            try
            {
                var drive = new DriveInfo(itemPath[0].ToString());
                string label = drive.VolumeLabel.Length > 0 ? drive.VolumeLabel : $"Drive ({itemPath[0]}:)";
                displayPath = $"USB Drive: {label}\r\nDrive Letter: {itemPath}";
            }
            catch
            {
                displayPath = $"USB Drive: {itemPath}";
            }
        }
        else
        {
            // This is a file or folder
            displayPath = $"Encrypted Item: {Path.GetFileName(itemPath)}\r\nLocation: {Path.GetDirectoryName(itemPath)}";
        }
        
        ItemPathText.Text = displayPath;
    }
    
    /// <summary>
    /// Gets the password entered by the user
    /// </summary>
    public string Password => PasswordBox.Password;
    
    /// <summary>
    /// Gets whether the user wants to overwrite existing files
    /// </summary>
    public bool OverwriteExisting => OverwriteExistingCheckbox.IsChecked ?? false;

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ValidatePassword();
    }
    
    private void ValidatePassword()
    {
        if (string.IsNullOrEmpty(PasswordBox.Password))
        {
            ShowError("Password cannot be empty.");
            return;
        }
        
        HideError();
    }
    
    private void ShowError(string message)
    {
        ErrorMessageText.Text = message;
        ErrorMessageText.Visibility = Visibility.Visible;
    }
    
    private void HideError()
    {
        ErrorMessageText.Visibility = Visibility.Collapsed;
    }
}
