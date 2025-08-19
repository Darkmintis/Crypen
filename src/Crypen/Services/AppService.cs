using Crypen.Core.Crypto;
using Crypen.Core.FileSystem;
using Crypen.Core.Models;
using Crypen.Core.Security;
using Crypen.Core.Services;
using System.IO;

namespace Crypen.Services;

/// <summary>
/// Service that provides application functionality to the UI
/// </summary>
public class AppService
{
    private readonly ICryptoService _cryptoService;
    private readonly ISecureFileSystem _fileSystem;
    private readonly PasswordStorageService _passwordStorage;
    private readonly HistoryService _historyService;
    
    public AppService(
        ICryptoService cryptoService,
        ISecureFileSystem fileSystem,
        PasswordStorageService passwordStorage,
        HistoryService historyService)
    {
        _cryptoService = cryptoService;
        _fileSystem = fileSystem;
        _passwordStorage = passwordStorage;
        _historyService = historyService;
    }
    
    /// <summary>
    /// Encrypts a file
    /// </summary>
    /// <param name="filePath">Path to the file to encrypt</param>
    /// <param name="password">Password to use for encryption</param>
    /// <param name="rememberPassword">Whether to store the password</param>
    /// <returns>True if encryption succeeded, false otherwise</returns>
    public async Task<bool> EncryptFileAsync(string filePath, string password, bool rememberPassword)
    {
        try
        {
            string destinationPath = $"{filePath}.crypen";
            
            // Check if the file already exists
            if (File.Exists(destinationPath))
            {
                return false;
            }
            
            // Encrypt the file
            bool result = await _cryptoService.EncryptFileAsync(filePath, destinationPath, password);
            
            if (result)
            {
                // Create a history entry
                var item = new EncryptedItem
                {
                    Path = destinationPath,
                    Name = Path.GetFileName(filePath),
                    ItemType = EncryptedItemType.File,
                    Status = "Encrypted",
                    IsPasswordStored = rememberPassword
                };
                
                _historyService.AddItem(item);
                
                // Store password if requested
                if (rememberPassword)
                {
                    _passwordStorage.SavePassword(destinationPath, password);
                }
            }
            
            return result;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Encrypts a directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory to encrypt</param>
    /// <param name="password">Password to use for encryption</param>
    /// <param name="rememberPassword">Whether to store the password</param>
    /// <returns>True if encryption succeeded, false otherwise</returns>
    public async Task<bool> EncryptDirectoryAsync(string directoryPath, string password, bool rememberPassword)
    {
        try
        {
            string destinationPath = $"{directoryPath}.crypen";
            
            // Check if the file already exists
            if (File.Exists(destinationPath))
            {
                return false;
            }
            
            // Encrypt the directory
            bool result = await _cryptoService.EncryptDirectoryAsync(directoryPath, destinationPath, password);
            
            if (result)
            {
                // Create a history entry
                var item = new EncryptedItem
                {
                    Path = destinationPath,
                    Name = new DirectoryInfo(directoryPath).Name,
                    ItemType = EncryptedItemType.Directory,
                    Status = "Encrypted",
                    IsPasswordStored = rememberPassword
                };
                
                _historyService.AddItem(item);
                
                // Store password if requested
                if (rememberPassword)
                {
                    _passwordStorage.SavePassword(destinationPath, password);
                }
            }
            
            return result;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Encrypts a USB drive
    /// </summary>
    /// <param name="driveLetter">Drive letter of the USB drive</param>
    /// <param name="password">Password to use for encryption</param>
    /// <param name="rememberPassword">Whether to store the password</param>
    /// <returns>True if encryption succeeded, false otherwise</returns>
    public async Task<bool> EncryptUsbDriveAsync(string driveLetter, string password, bool rememberPassword)
    {
        try
        {
            // Get the drive info
            DriveInfo drive = new DriveInfo(driveLetter);
            
            // Encrypt the drive
            bool result = await _cryptoService.EncryptUsbDriveAsync(driveLetter, password);
            
            if (result)
            {
                // Create a history entry
                var item = new EncryptedItem
                {
                    Path = $"{driveLetter}:\\",
                    Name = drive.VolumeLabel.Length > 0 ? drive.VolumeLabel : $"Drive ({driveLetter}:)",
                    ItemType = EncryptedItemType.UsbDrive,
                    Status = "Encrypted",
                    IsPasswordStored = rememberPassword
                };
                
                _historyService.AddItem(item);
                
                // Store password if requested
                if (rememberPassword)
                {
                    _passwordStorage.SavePassword($"{driveLetter}:\\", password);
                }
            }
            
            return result;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Decrypts an encrypted file, directory, or USB drive
    /// </summary>
    /// <param name="path">Path to the encrypted item</param>
    /// <param name="password">Password to use for decryption</param>
    /// <param name="overwriteExisting">Whether to overwrite existing files</param>
    /// <returns>True if decryption succeeded, false otherwise</returns>
    public async Task<bool> DecryptAsync(string path, string password, bool overwriteExisting = false)
    {
        try
        {
            bool result = false;
            
            // Check if this is a USB drive
            if (path.EndsWith(":\\") && File.Exists(Path.Combine(path, ".crypen")))
            {
                result = await _cryptoService.DecryptUsbDriveAsync(path[0].ToString(), password);
            }
            // Check if this is an encrypted file
            else if (File.Exists(path) && await _fileSystem.IsEncryptedFileAsync(path))
            {
                // Determine the output path (remove .crypen extension)
                string outputPath = path.EndsWith(".crypen") ? path[..^7] : path + ".decrypted";
                
                // Check if the output path exists
                if (File.Exists(outputPath) && !overwriteExisting)
                {
                    return false;
                }
                
                // Determine if this was an encrypted directory
                EncryptedItem? item = _historyService.FindItemByPath(path);
                if (item?.ItemType == EncryptedItemType.Directory)
                {
                    result = await _cryptoService.DecryptDirectoryAsync(path, outputPath, password);
                }
                else
                {
                    result = await _cryptoService.DecryptFileAsync(path, outputPath, password);
                }
            }
            
            if (result)
            {
                // Update the history entry if it exists
                EncryptedItem? item = _historyService.FindItemByPath(path);
                if (item != null)
                {
                    item.Status = "Decrypted";
                    _historyService.UpdateItem(item);
                }
            }
            
            return result;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Verifies if the provided password is correct for the encrypted item
    /// </summary>
    /// <param name="path">Path to the encrypted item</param>
    /// <param name="password">Password to verify</param>
    /// <returns>True if the password is correct, false otherwise</returns>
    public async Task<bool> VerifyPasswordAsync(string path, string password)
    {
        try
        {
            if (path.EndsWith(":\\") && File.Exists(Path.Combine(path, ".crypen")))
            {
                // This is a USB drive - verify with the marker file
                string markerFile = Path.Combine(path, ".crypen.dat");
                return await _cryptoService.VerifyPasswordAsync(markerFile, password);
            }
            else if (File.Exists(path))
            {
                // This is a file - verify directly
                return await _cryptoService.VerifyPasswordAsync(path, password);
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets all encrypted items from history
    /// </summary>
    /// <returns>A list of encrypted items</returns>
    public List<EncryptedItem> GetEncryptedItems()
    {
        return _historyService.GetItems();
    }
    
    /// <summary>
    /// Removes an encrypted item from history
    /// </summary>
    /// <param name="itemId">ID of the item to remove</param>
    public void RemoveEncryptedItem(Guid itemId)
    {
        _historyService.RemoveItem(itemId);
    }
    
    /// <summary>
    /// Gets a stored password for an encrypted item
    /// </summary>
    /// <param name="path">Path to the encrypted item</param>
    /// <returns>The stored password, or null if not found</returns>
    public string? GetStoredPassword(string path)
    {
        return _passwordStorage.GetPassword(path);
    }
    
    /// <summary>
    /// Checks if a path corresponds to an encrypted item
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if the path is an encrypted item, false otherwise</returns>
    public async Task<bool> IsEncryptedItem(string path)
    {
        if (path.EndsWith(":\\") && File.Exists(Path.Combine(path, ".crypen")))
        {
            return true;
        }
        else if (File.Exists(path))
        {
            return path.EndsWith(".crypen") || await _fileSystem.IsEncryptedFileAsync(path);
        }
        
        return false;
    }
}
