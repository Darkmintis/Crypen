using Crypen.Views;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Crypen.Services;

/// <summary>
/// Service for handling keyboard shortcuts
/// </summary>
public class KeyboardShortcutHandler
{
    private readonly MainWindow _mainWindow;
    
    public KeyboardShortcutHandler(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }
    
    /// <summary>
    /// Handles keyboard accelerators
    /// </summary>
    public void HandleKeyDown(KeyRoutedEventArgs e)
    {
        var ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        var shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        var alt = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        
        // Ctrl+O: Open file to encrypt
        if (ctrl && !shift && !alt && e.Key == VirtualKey.O)
        {
            // Navigate to encrypt page
            _mainWindow.NavigateToEncryptPage();
            e.Handled = true;
        }
        // Ctrl+D: Open file to decrypt
        else if (ctrl && !shift && !alt && e.Key == VirtualKey.D)
        {
            // Navigate to dashboard
            _mainWindow.NavigateToDashboard();
            e.Handled = true;
        }
        // Ctrl+H: View history
        else if (ctrl && !shift && !alt && e.Key == VirtualKey.H)
        {
            _mainWindow.NavigateToHistory();
            e.Handled = true;
        }
        // Ctrl+, (comma): Settings
        else if (ctrl && !shift && !alt && e.Key == VirtualKey.Number188) // Comma
        {
            _mainWindow.NavigateToSettings();
            e.Handled = true;
        }
        // F5: Refresh current page
        else if (!ctrl && !shift && !alt && e.Key == VirtualKey.F5)
        {
            _mainWindow.RefreshCurrentPage();
            e.Handled = true;
        }
        // Escape: Close dialogs or return to main view
        else if (!ctrl && !shift && !alt && e.Key == VirtualKey.Escape)
        {
            // This is typically handled by the dialog itself
        }
    }
}
