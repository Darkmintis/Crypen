namespace Crypen.Core.FileSystem;

/// <summary>
/// Interface for secure file system operations
/// </summary>
public interface ISecureFileSystem
{
    /// <summary>
    /// Securely deletes a file by overwriting its content before deletion
    /// </summary>
    /// <param name="filePath">Path to the file to delete</param>
    Task SecureDeleteFileAsync(string filePath);
    
    /// <summary>
    /// Securely deletes a directory and all its contents
    /// </summary>
    /// <param name="directoryPath">Path to the directory to delete</param>
    Task SecureDeleteDirectoryAsync(string directoryPath);
    
    /// <summary>
    /// Checks if a file is a Crypen encrypted file
    /// </summary>
    /// <param name="filePath">Path to the file to check</param>
    /// <returns>True if the file is a Crypen encrypted file, false otherwise</returns>
    Task<bool> IsEncryptedFileAsync(string filePath);
}
