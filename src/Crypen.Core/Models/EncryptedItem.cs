namespace Crypen.Core.Models;

/// <summary>
/// Represents the type of encrypted item
/// </summary>
public enum EncryptedItemType
{
    File,
    Directory,
    UsbDrive
}

/// <summary>
/// Represents an encrypted item in the history
/// </summary>
public class EncryptedItem
{
    /// <summary>
    /// Unique identifier for the encrypted item
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Full path to the encrypted item
    /// </summary>
    public required string Path { get; set; }
    
    /// <summary>
    /// Display name for the encrypted item (filename or folder name)
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Type of the encrypted item
    /// </summary>
    public EncryptedItemType ItemType { get; set; }
    
    /// <summary>
    /// Date and time when the item was encrypted
    /// </summary>
    public DateTime EncryptedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Whether the password is stored for this item
    /// </summary>
    public bool IsPasswordStored { get; set; }
    
    /// <summary>
    /// Current status of the encrypted item (Encrypted or Decrypted)
    /// </summary>
    public string Status { get; set; } = "Encrypted";
}
