using Crypen.Core.Crypto;
using Crypen.Core.FileSystem;
using Crypen.Core.Models;
using Crypen.Core.Security;
using Crypen.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Crypen.Services;

/// <summary>
/// Service for handling bulk encryption/decryption operations
/// </summary>
public class BulkOperationsService
{
    private readonly ICryptoService _cryptoService;
    private readonly ISecureFileSystem _fileSystem;
    private readonly PasswordStorageService _passwordStorage;
    private readonly HistoryService _historyService;
    
    public BulkOperationsService(
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
    /// Encrypts multiple files with progress reporting
    /// </summary>
    public async Task<List<string>> EncryptFilesAsync(
        IEnumerable<string> filePaths, 
        string password, 
        bool rememberPassword,
        IProgress<EncryptionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var successfulPaths = new List<string>();
        var fileList = filePaths.ToList();
        int totalFiles = fileList.Count;
        int processedFiles = 0;
        
        foreach (var filePath in fileList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                progress?.Report(new EncryptionProgress
                {
                    Operation = "Bulk encryption cancelled",
                    IsCancelled = true,
                    FilesProcessed = processedFiles,
                    TotalFiles = totalFiles
                });
                break;
            }
            
            try
            {
                string destinationPath = $"{filePath}.crypen";
                string fileName = Path.GetFileName(filePath);
                
                progress?.Report(new EncryptionProgress
                {
                    Operation = "Encrypting files",
                    CurrentFile = fileName,
                    FilesProcessed = processedFiles,
                    TotalFiles = totalFiles,
                    PercentComplete = (int)((processedFiles / (double)totalFiles) * 100)
                });
                
                bool success = await _cryptoService.EncryptFileAsync(filePath, destinationPath, password);
                
                if (success)
                {
                    successfulPaths.Add(destinationPath);
                    
                    // Add to history
                    var item = new EncryptedItem
                    {
                        Path = destinationPath,
                        Name = fileName,
                        ItemType = EncryptedItemType.File,
                        Status = "Encrypted",
                        IsPasswordStored = rememberPassword
                    };
                    
                    _historyService.AddItem(item);
                    
                    if (rememberPassword)
                    {
                        _passwordStorage.SavePassword(destinationPath, password);
                    }
                }
                
                processedFiles++;
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                Console.WriteLine($"Failed to encrypt {filePath}: {ex.Message}");
            }
        }
        
        progress?.Report(new EncryptionProgress
        {
            Operation = "Bulk encryption complete",
            IsComplete = true,
            FilesProcessed = processedFiles,
            TotalFiles = totalFiles,
            PercentComplete = 100
        });
        
        return successfulPaths;
    }
    
    /// <summary>
    /// Decrypts multiple files with progress reporting
    /// </summary>
    public async Task<List<string>> DecryptFilesAsync(
        IEnumerable<string> filePaths, 
        string password,
        bool overwriteExisting = false,
        IProgress<EncryptionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var successfulPaths = new List<string>();
        var fileList = filePaths.ToList();
        int totalFiles = fileList.Count;
        int processedFiles = 0;
        
        foreach (var filePath in fileList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                progress?.Report(new EncryptionProgress
                {
                    Operation = "Bulk decryption cancelled",
                    IsCancelled = true,
                    FilesProcessed = processedFiles,
                    TotalFiles = totalFiles
                });
                break;
            }
            
            try
            {
                string outputPath = filePath.EndsWith(".crypen") ? filePath[..^7] : filePath + ".decrypted";
                string fileName = Path.GetFileName(filePath);
                
                progress?.Report(new EncryptionProgress
                {
                    Operation = "Decrypting files",
                    CurrentFile = fileName,
                    FilesProcessed = processedFiles,
                    TotalFiles = totalFiles,
                    PercentComplete = (int)((processedFiles / (double)totalFiles) * 100)
                });
                
                if (File.Exists(outputPath) && !overwriteExisting)
                {
                    processedFiles++;
                    continue;
                }
                
                // Check if this was an encrypted directory
                EncryptedItem? item = _historyService.FindItemByPath(filePath);
                bool success;
                
                if (item?.ItemType == EncryptedItemType.Directory)
                {
                    success = await _cryptoService.DecryptDirectoryAsync(filePath, outputPath, password);
                }
                else
                {
                    success = await _cryptoService.DecryptFileAsync(filePath, outputPath, password);
                }
                
                if (success)
                {
                    successfulPaths.Add(outputPath);
                    
                    // Update history
                    if (item != null)
                    {
                        item.Status = "Decrypted";
                        _historyService.UpdateItem(item);
                    }
                }
                
                processedFiles++;
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                Console.WriteLine($"Failed to decrypt {filePath}: {ex.Message}");
            }
        }
        
        progress?.Report(new EncryptionProgress
        {
            Operation = "Bulk decryption complete",
            IsComplete = true,
            FilesProcessed = processedFiles,
            TotalFiles = totalFiles,
            PercentComplete = 100
        });
        
        return successfulPaths;
    }
}
