namespace Crypen.Core.Models;

/// <summary>
/// Represents progress information for encryption/decryption operations
/// </summary>
public class EncryptionProgress
{
    /// <summary>
    /// Current operation being performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Percentage of completion (0-100)
    /// </summary>
    public int PercentComplete { get; set; }
    
    /// <summary>
    /// Number of bytes processed
    /// </summary>
    public long BytesProcessed { get; set; }
    
    /// <summary>
    /// Total number of bytes to process
    /// </summary>
    public long TotalBytes { get; set; }
    
    /// <summary>
    /// Current file being processed (for batch operations)
    /// </summary>
    public string? CurrentFile { get; set; }
    
    /// <summary>
    /// Number of files processed (for batch operations)
    /// </summary>
    public int FilesProcessed { get; set; }
    
    /// <summary>
    /// Total number of files to process (for batch operations)
    /// </summary>
    public int TotalFiles { get; set; }
    
    /// <summary>
    /// Whether the operation is complete
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Whether the operation was cancelled
    /// </summary>
    public bool IsCancelled { get; set; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
