using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Text.RegularExpressions;

namespace Crypen.Dialogs;

/// <summary>
/// Dialog for encrypting items
/// </summary>
public sealed partial class EncryptDialog : UserControl
{
    private readonly string _itemPath;
    private readonly EncryptionItemType _itemType;
    
    public EncryptDialog(string itemPath, EncryptionItemType itemType)
    {
        this.InitializeComponent();
        
        _itemPath = itemPath;
        _itemType = itemType;
        
        // Display the path nicely based on type
        string displayPath;
        switch (itemType)
        {
            case EncryptionItemType.File:
                displayPath = $"File: {Path.GetFileName(itemPath)}\r\nLocation: {Path.GetDirectoryName(itemPath)}";
                break;
            case EncryptionItemType.Directory:
                displayPath = $"Folder: {Path.GetFileName(itemPath)}\r\nLocation: {Path.GetDirectoryName(itemPath)}";
                break;
            case EncryptionItemType.UsbDrive:
                // Try to get the drive label
                try
                {
                    var drive = new DriveInfo(itemPath);
                    string label = drive.VolumeLabel.Length > 0 ? drive.VolumeLabel : $"Drive ({itemPath}:)";
                    displayPath = $"USB Drive: {label}\r\nDrive Letter: {itemPath}:\\";
                }
                catch
                {
                    displayPath = $"USB Drive: {itemPath}:\\";
                }
                break;
            default:
                displayPath = itemPath;
                break;
        }
        
        ItemPathText.Text = displayPath;
    }
    
    /// <summary>
    /// Gets the password entered by the user
    /// </summary>
    public string Password => PasswordBox.Password;
    
    /// <summary>
    /// Gets whether the user wants to remember the password
    /// </summary>
    public bool RememberPassword => RememberPasswordCheckbox.IsChecked ?? false;

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ValidatePasswords();
        UpdatePasswordStrength();
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ValidatePasswords();
    }
    
    private void ValidatePasswords()
    {
        if (string.IsNullOrEmpty(PasswordBox.Password))
        {
            ShowError("Password cannot be empty.");
            return;
        }
        
        if (PasswordBox.Password.Length < 8)
        {
            ShowError("Password must be at least 8 characters long.");
            return;
        }
        
        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            ShowError("Passwords do not match.");
            return;
        }
        
        HideError();
    }
    
    private void UpdatePasswordStrength()
    {
        string password = PasswordBox.Password;
        
        if (string.IsNullOrEmpty(password))
        {
            PasswordStrengthBar.Value = 0;
            PasswordStrengthText.Text = "Weak";
            PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            return;
        }
        
        // Calculate password strength
        int strength = 0;
        
        // Length
        if (password.Length >= 8) strength += 20;
        if (password.Length >= 12) strength += 10;
        if (password.Length >= 16) strength += 10;
        
        // Complexity
        if (Regex.IsMatch(password, @"[A-Z]")) strength += 15; // Uppercase
        if (Regex.IsMatch(password, @"[a-z]")) strength += 15; // Lowercase
        if (Regex.IsMatch(password, @"[0-9]")) strength += 15; // Numbers
        if (Regex.IsMatch(password, @"[^A-Za-z0-9]")) strength += 15; // Special chars
        
        // Set the strength indicator
        PasswordStrengthBar.Value = strength;
        
        if (strength < 40)
        {
            PasswordStrengthText.Text = "Weak";
            PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        }
        else if (strength < 70)
        {
            PasswordStrengthText.Text = "Medium";
            PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
        }
        else
        {
            PasswordStrengthText.Text = "Strong";
            PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        }
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
