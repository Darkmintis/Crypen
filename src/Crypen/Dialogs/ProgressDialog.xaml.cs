using Crypen.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crypen.Dialogs;

/// <summary>
/// Dialog for showing encryption/decryption progress
/// </summary>
public sealed partial class ProgressDialog : ContentDialog, INotifyPropertyChanged
{
    private bool _canCancel = true;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public ProgressDialog(CancellationTokenSource cancellationTokenSource)
    {
        this.InitializeComponent();
        _cancellationTokenSource = cancellationTokenSource;
    }
    
    public bool CanCancel
    {
        get => _canCancel;
        set
        {
            if (_canCancel != value)
            {
                _canCancel = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// Updates the progress display
    /// </summary>
    public void UpdateProgress(EncryptionProgress progress)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            OperationText.Text = progress.Operation;
            ProgressBar.Value = progress.PercentComplete;
            ProgressBar.IsIndeterminate = progress.PercentComplete == 0 && !progress.IsComplete;
            
            // Update progress details
            if (progress.TotalFiles > 1)
            {
                ProgressDetailsText.Text = $"{progress.PercentComplete}% complete - File {progress.FilesProcessed}/{progress.TotalFiles}";
                
                if (!string.IsNullOrEmpty(progress.CurrentFile))
                {
                    CurrentFileText.Text = $"Processing: {progress.CurrentFile}";
                    CurrentFileText.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (progress.TotalBytes > 0)
                {
                    ProgressDetailsText.Text = $"{progress.PercentComplete}% complete - {FormatSize(progress.BytesProcessed)} / {FormatSize(progress.TotalBytes)}";
                }
                else
                {
                    ProgressDetailsText.Text = $"{progress.PercentComplete}% complete";
                }
            }
            
            // Show completion or error status
            if (progress.IsComplete)
            {
                StatusText.Text = "Operation completed successfully!";
                StatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                StatusText.Visibility = Visibility.Visible;
                CanCancel = false;
            }
            else if (!string.IsNullOrEmpty(progress.ErrorMessage))
            {
                StatusText.Text = $"Error: {progress.ErrorMessage}";
                StatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                StatusText.Visibility = Visibility.Visible;
                CanCancel = false;
            }
            else if (progress.IsCancelled)
            {
                StatusText.Text = "Operation cancelled";
                StatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
                StatusText.Visibility = Visibility.Visible;
                CanCancel = false;
            }
        });
    }
    
    private void CancelButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _cancellationTokenSource?.Cancel();
        StatusText.Text = "Cancelling operation...";
        StatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
        StatusText.Visibility = Visibility.Visible;
        CanCancel = false;
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
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
