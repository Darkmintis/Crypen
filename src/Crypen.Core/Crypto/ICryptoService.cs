namespace Crypen.Core.Crypto;

/// <summary>
/// Interface defining cryptographic operations for Crypen
/// </summary>
public interface ICryptoService
{
    /// <summary>
    /// Encrypts a file using AES-256-GCM with password-derived key
    /// </summary>
    /// <param name="sourceFilePath">Path to the source file to encrypt</param>
    /// <param name="destinationFilePath">Path where the encrypted file should be saved</param>
    /// <param name="password">Password used for encryption</param>
    /// <returns>True if encryption succeeded, false otherwise</returns>
    Task<bool> EncryptFileAsync(string sourceFilePath, string destinationFilePath, string password);
    
    /// <summary>
    /// Decrypts a file that was encrypted using Crypen
    /// </summary>
    /// <param name="sourceFilePath">Path to the encrypted file</param>
    /// <param name="destinationFilePath">Path where the decrypted file should be saved</param>
    /// <param name="password">Password used for decryption</param>
    /// <returns>True if decryption succeeded, false otherwise</returns>
    Task<bool> DecryptFileAsync(string sourceFilePath, string destinationFilePath, string password);
    
    /// <summary>
    /// Encrypts a directory by creating an encrypted archive
    /// </summary>
    /// <param name="sourceDirectoryPath">Path to the source directory to encrypt</param>
    /// <param name="destinationFilePath">Path where the encrypted archive should be saved</param>
    /// <param name="password">Password used for encryption</param>
    /// <returns>True if encryption succeeded, false otherwise</returns>
    Task<bool> EncryptDirectoryAsync(string sourceDirectoryPath, string destinationFilePath, string password);
    
    /// <summary>
    /// Decrypts a directory archive that was encrypted using Crypen
    /// </summary>
    /// <param name="sourceFilePath">Path to the encrypted directory archive</param>
    /// <param name="destinationDirectoryPath">Path where the decrypted directory should be extracted</param>
    /// <param name="password">Password used for decryption</param>
    /// <returns>True if decryption succeeded, false otherwise</returns>
    Task<bool> DecryptDirectoryAsync(string sourceFilePath, string destinationDirectoryPath, string password);
    
    /// <summary>
    /// Encrypts a USB drive
    /// </summary>
    /// <param name="driveLetter">Drive letter of the USB drive</param>
    /// <param name="password">Password used for encryption</param>
    /// <returns>True if encryption succeeded, false otherwise</returns>
    Task<bool> EncryptUsbDriveAsync(string driveLetter, string password);
    
    /// <summary>
    /// Decrypts a USB drive that was encrypted using Crypen
    /// </summary>
    /// <param name="driveLetter">Drive letter of the encrypted USB drive</param>
    /// <param name="password">Password used for decryption</param>
    /// <returns>True if decryption succeeded, false otherwise</returns>
    Task<bool> DecryptUsbDriveAsync(string driveLetter, string password);
    
    /// <summary>
    /// Tests if the provided password can decrypt the specified encrypted file
    /// </summary>
    /// <param name="encryptedFilePath">Path to the encrypted file</param>
    /// <param name="password">Password to test</param>
    /// <returns>True if password is correct, false otherwise</returns>
    Task<bool> VerifyPasswordAsync(string encryptedFilePath, string password);
}
