using Crypen.Core.FileSystem;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Crypen.Core.Crypto;

/// <summary>
/// Implementation of ICryptoService using AES-256-GCM for encryption/decryption
/// </summary>
public class CryptoService : ICryptoService
{
    // Constants for the file format
    private const string FileFormatVersion = "CRYPEN01";
    private const int NonceSizeBytes = 12;      // 96 bits (recommended for GCM)
    private const int AuthTagSizeBytes = 16;    // 128 bits (maximum security for GCM)
    private const int ChunkSize = 1024 * 1024;  // 1 MB chunks for processing large files
    
    private readonly ISecureFileSystem _fileSystem;
    
    public CryptoService(ISecureFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    /// <inheritdoc />
    public async Task<bool> EncryptFileAsync(string sourceFilePath, string destinationFilePath, string password)
    {
        try
        {
            using FileStream sourceFile = File.OpenRead(sourceFilePath);
            using FileStream destinationFile = File.Create(destinationFilePath);
            
            // Generate a random salt for key derivation
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            
            // Derive encryption key from password
            (byte[] key, _) = KeyDerivationService.DeriveKey(password, salt);
            
            // Generate a random nonce (Number used ONCE)
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            
            // Write file header with format version
            await destinationFile.WriteAsync(System.Text.Encoding.UTF8.GetBytes(FileFormatVersion));
            
            // Write the salt and nonce
            await destinationFile.WriteAsync(salt);
            await destinationFile.WriteAsync(nonce);
            
            // Create position for the file size
            long fileSizePosition = destinationFile.Position;
            await destinationFile.WriteAsync(BitConverter.GetBytes((long)0)); // Placeholder for file size
            
            using var aes = new AesGcm(key);
            byte[] buffer = new byte[ChunkSize];
            byte[] encryptedBuffer = new byte[ChunkSize + AuthTagSizeBytes];
            
            long totalBytesRead = 0;
            int bytesRead;
            
            // Process the file in chunks
            while ((bytesRead = await sourceFile.ReadAsync(buffer, 0, ChunkSize)) > 0)
            {
                byte[] tag = new byte[AuthTagSizeBytes];
                byte[] actualBuffer = bytesRead < ChunkSize ? buffer[..bytesRead] : buffer;
                
                // Calculate the nonce for this chunk
                byte[] chunkNonce = new byte[NonceSizeBytes];
                Array.Copy(nonce, chunkNonce, NonceSizeBytes);
                
                // XOR the chunk index into the last 8 bytes of the nonce
                long chunkIndex = totalBytesRead / ChunkSize;
                for (int i = 0; i < 8; i++)
                {
                    chunkNonce[NonceSizeBytes - 8 + i] ^= (byte)(chunkIndex >> (i * 8));
                }
                
                // Encrypt the chunk
                aes.Encrypt(
                    chunkNonce,
                    actualBuffer,
                    encryptedBuffer[..bytesRead],
                    tag);
                
                // Write encrypted data and authentication tag
                await destinationFile.WriteAsync(encryptedBuffer[..bytesRead]);
                await destinationFile.WriteAsync(tag);
                
                totalBytesRead += bytesRead;
            }
            
            // Go back and write the original file size
            destinationFile.Position = fileSizePosition;
            await destinationFile.WriteAsync(BitConverter.GetBytes(sourceFile.Length));
            
            // Delete the original file securely
            await _fileSystem.SecureDeleteFileAsync(sourceFilePath);
            
            return true;
        }
        catch (Exception ex)
        {
            // Log exception details here
            Console.WriteLine($"Encryption failed: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> DecryptFileAsync(string sourceFilePath, string destinationFilePath, string password)
    {
        try
        {
            using FileStream sourceFile = File.OpenRead(sourceFilePath);
            
            // Read and verify header
            byte[] headerBytes = new byte[FileFormatVersion.Length];
            await sourceFile.ReadAsync(headerBytes, 0, headerBytes.Length);
            string header = System.Text.Encoding.UTF8.GetString(headerBytes);
            
            if (header != FileFormatVersion)
            {
                throw new InvalidOperationException("File is not a valid Crypen encrypted file.");
            }
            
            // Read salt and nonce
            byte[] salt = new byte[16];
            byte[] nonce = new byte[NonceSizeBytes];
            await sourceFile.ReadAsync(salt, 0, salt.Length);
            await sourceFile.ReadAsync(nonce, 0, nonce.Length);
            
            // Read original file size
            byte[] fileSizeBytes = new byte[8];
            await sourceFile.ReadAsync(fileSizeBytes, 0, fileSizeBytes.Length);
            long originalFileSize = BitConverter.ToInt64(fileSizeBytes, 0);
            
            // Derive the key using the stored salt
            (byte[] key, _) = KeyDerivationService.DeriveKey(password, salt);
            
            // Create the destination file
            using FileStream destinationFile = File.Create(destinationFilePath);
            
            using var aes = new AesGcm(key);
            byte[] encryptedBuffer = new byte[ChunkSize];
            byte[] decryptedBuffer = new byte[ChunkSize];
            byte[] tag = new byte[AuthTagSizeBytes];
            
            long totalBytesDecrypted = 0;
            long remainingBytes = originalFileSize;
            long chunkIndex = 0;
            
            while (remainingBytes > 0)
            {
                // Calculate how many bytes to read in this chunk
                int bytesToRead = (int)Math.Min(ChunkSize, remainingBytes);
                
                // Read encrypted data
                await sourceFile.ReadAsync(encryptedBuffer, 0, bytesToRead);
                
                // Read authentication tag
                await sourceFile.ReadAsync(tag, 0, AuthTagSizeBytes);
                
                // Calculate the nonce for this chunk
                byte[] chunkNonce = new byte[NonceSizeBytes];
                Array.Copy(nonce, chunkNonce, NonceSizeBytes);
                
                // XOR the chunk index into the last 8 bytes of the nonce
                for (int i = 0; i < 8; i++)
                {
                    chunkNonce[NonceSizeBytes - 8 + i] ^= (byte)(chunkIndex >> (i * 8));
                }
                
                // Decrypt the chunk
                try
                {
                    aes.Decrypt(
                        chunkNonce,
                        encryptedBuffer[..bytesToRead],
                        tag,
                        decryptedBuffer[..bytesToRead]);
                }
                catch (CryptographicException)
                {
                    // If decryption fails, most likely the password is wrong
                    destinationFile.Close();
                    File.Delete(destinationFilePath);
                    return false;
                }
                
                // Write the decrypted data
                await destinationFile.WriteAsync(decryptedBuffer, 0, bytesToRead);
                
                remainingBytes -= bytesToRead;
                totalBytesDecrypted += bytesToRead;
                chunkIndex++;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            // Log exception details here
            Console.WriteLine($"Decryption failed: {ex.Message}");
            
            // Cleanup partial file if it exists
            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
            }
            
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> EncryptDirectoryAsync(string sourceDirectoryPath, string destinationFilePath, string password)
    {
        try
        {
            // Create a temporary zip file
            string tempZipFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            
            try
            {
                // Compress the directory
                ZipFile.CreateFromDirectory(sourceDirectoryPath, tempZipFile, CompressionLevel.Optimal, false);
                
                // Encrypt the zip file
                bool result = await EncryptFileAsync(tempZipFile, destinationFilePath, password);
                
                if (result)
                {
                    // Delete the original directory securely
                    await _fileSystem.SecureDeleteDirectoryAsync(sourceDirectoryPath);
                }
                
                return result;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempZipFile))
                {
                    File.Delete(tempZipFile);
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception details here
            Console.WriteLine($"Directory encryption failed: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> DecryptDirectoryAsync(string sourceFilePath, string destinationDirectoryPath, string password)
    {
        try
        {
            // Create a temporary file for the decrypted zip
            string tempZipFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            
            try
            {
                // Decrypt the file
                bool result = await DecryptFileAsync(sourceFilePath, tempZipFile, password);
                
                if (!result)
                {
                    return false;
                }
                
                // Create the destination directory if it doesn't exist
                Directory.CreateDirectory(destinationDirectoryPath);
                
                // Extract the zip file
                ZipFile.ExtractToDirectory(tempZipFile, destinationDirectoryPath);
                
                return true;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempZipFile))
                {
                    File.Delete(tempZipFile);
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception details here
            Console.WriteLine($"Directory decryption failed: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> EncryptUsbDriveAsync(string driveLetter, string password)
    {
        try
        {
            // Validate drive letter
            if (!Directory.Exists($"{driveLetter}:\\"))
            {
                throw new DirectoryNotFoundException($"Drive {driveLetter}: not found or not ready.");
            }
            
            // Create a special file at the root of the drive to mark it as encrypted by Crypen
            string crypenMarkerFile = $"{driveLetter}:\\.crypen";
            string crypenDataFile = $"{driveLetter}:\\.crypen.dat";
            
            // Encrypt each file on the drive individually
            DriveInfo drive = new DriveInfo(driveLetter);
            DirectoryInfo rootDir = drive.RootDirectory;
            
            // Create a manifest of files that will be encrypted
            List<string> fileManifest = new List<string>();
            
            foreach (FileInfo file in rootDir.GetFiles("*", SearchOption.AllDirectories))
            {
                // Skip the marker files
                if (file.FullName.EndsWith(".crypen") || file.FullName.EndsWith(".crypen.dat"))
                {
                    continue;
                }
                
                string relativePath = file.FullName.Substring(drive.RootDirectory.FullName.Length);
                fileManifest.Add(relativePath);
                
                // Encrypt the file
                string encryptedFilePath = $"{file.FullName}.crypen";
                await EncryptFileAsync(file.FullName, encryptedFilePath, password);
            }
            
            // Write the manifest
            File.WriteAllText(crypenMarkerFile, "This drive is encrypted with Crypen.");
            File.WriteAllLines(crypenDataFile, fileManifest);
            
            return true;
        }
        catch (Exception ex)
        {
            // Log exception details here
            Console.WriteLine($"USB drive encryption failed: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> DecryptUsbDriveAsync(string driveLetter, string password)
    {
        try
        {
            // Validate drive letter and check if it's a Crypen-encrypted drive
            string crypenMarkerFile = $"{driveLetter}:\\.crypen";
            string crypenDataFile = $"{driveLetter}:\\.crypen.dat";
            
            if (!File.Exists(crypenMarkerFile) || !File.Exists(crypenDataFile))
            {
                throw new InvalidOperationException($"Drive {driveLetter}: is not encrypted with Crypen.");
            }
            
            // Read the manifest
            string[] fileManifest = File.ReadAllLines(crypenDataFile);
            
            // Decrypt each file listed in the manifest
            foreach (string relativePath in fileManifest)
            {
                string encryptedFilePath = $"{driveLetter}:\\{relativePath}.crypen";
                string decryptedFilePath = $"{driveLetter}:\\{relativePath}";
                
                // Create directory structure if needed
                string directoryPath = Path.GetDirectoryName(decryptedFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                // Decrypt the file
                await DecryptFileAsync(encryptedFilePath, decryptedFilePath, password);
                
                // Delete the encrypted file
                File.Delete(encryptedFilePath);
            }
            
            // Remove the marker files
            File.Delete(crypenMarkerFile);
            File.Delete(crypenDataFile);
            
            return true;
        }
        catch (Exception ex)
        {
            // Log exception details here
            Console.WriteLine($"USB drive decryption failed: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> VerifyPasswordAsync(string encryptedFilePath, string password)
    {
        try
        {
            using FileStream sourceFile = File.OpenRead(encryptedFilePath);
            
            // Read and verify header
            byte[] headerBytes = new byte[FileFormatVersion.Length];
            await sourceFile.ReadAsync(headerBytes, 0, headerBytes.Length);
            string header = System.Text.Encoding.UTF8.GetString(headerBytes);
            
            if (header != FileFormatVersion)
            {
                return false;
            }
            
            // Read salt and nonce
            byte[] salt = new byte[16];
            byte[] nonce = new byte[NonceSizeBytes];
            await sourceFile.ReadAsync(salt, 0, salt.Length);
            await sourceFile.ReadAsync(nonce, 0, nonce.Length);
            
            // Skip file size
            sourceFile.Position += 8;
            
            // Derive the key using the stored salt
            (byte[] key, _) = KeyDerivationService.DeriveKey(password, salt);
            
            using var aes = new AesGcm(key);
            
            // Just read the first chunk to verify
            byte[] encryptedBuffer = new byte[Math.Min(ChunkSize, (int)(sourceFile.Length - sourceFile.Position - AuthTagSizeBytes))];
            byte[] decryptedBuffer = new byte[encryptedBuffer.Length];
            byte[] tag = new byte[AuthTagSizeBytes];
            
            // Read first chunk of encrypted data
            await sourceFile.ReadAsync(encryptedBuffer, 0, encryptedBuffer.Length);
            
            // Read authentication tag
            await sourceFile.ReadAsync(tag, 0, tag.Length);
            
            // Attempt to decrypt
            try
            {
                aes.Decrypt(nonce, encryptedBuffer, tag, decryptedBuffer);
                return true; // If no exception, password is correct
            }
            catch (CryptographicException)
            {
                return false; // Incorrect password
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}
