using System.Security.Cryptography;

namespace Crypen.Core.FileSystem;

/// <summary>
/// Implementation of secure file system operations
/// </summary>
public class SecureFileSystem : ISecureFileSystem
{
    // Constants
    private const string FileFormatVersion = "CRYPEN01";
    private const int OverwritePassCount = 3; // Number of times to overwrite files
    private const int BufferSize = 1024 * 1024; // 1 MB buffer for file operations
    
    /// <inheritdoc />
    public async Task SecureDeleteFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        // Get file info for later use
        FileInfo fileInfo = new FileInfo(filePath);
        long fileSize = fileInfo.Length;

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
        {
            // Perform multiple passes of overwriting
            for (int pass = 0; pass < OverwritePassCount; pass++)
            {
                fs.Position = 0;
                byte[] buffer;

                switch (pass)
                {
                    case 0: // First pass: random data
                        buffer = RandomNumberGenerator.GetBytes(BufferSize);
                        break;
                    case 1: // Second pass: all zeros
                        buffer = new byte[BufferSize];
                        break;
                    default: // Third pass: all ones
                        buffer = new byte[BufferSize];
                        Array.Fill<byte>(buffer, 0xFF);
                        break;
                }

                // Write the buffer in chunks
                long bytesRemaining = fileSize;
                while (bytesRemaining > 0)
                {
                    int bytesToWrite = (int)Math.Min(buffer.Length, bytesRemaining);
                    await fs.WriteAsync(buffer, 0, bytesToWrite);
                    bytesRemaining -= bytesToWrite;
                }

                // Flush the buffer to ensure data is written to disk
                await fs.FlushAsync();
            }
        }

        // Delete the file
        File.Delete(filePath);
    }

    /// <inheritdoc />
    public async Task SecureDeleteDirectoryAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        // Process all files in the directory
        foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            await SecureDeleteFileAsync(filePath);
        }

        // Delete all subdirectories (which should now be empty)
        Directory.Delete(directoryPath, true);
    }

    /// <inheritdoc />
    public async Task<bool> IsEncryptedFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            // Check if the file is long enough to contain the header
            if (fs.Length < FileFormatVersion.Length)
            {
                return false;
            }
            
            byte[] header = new byte[FileFormatVersion.Length];
            await fs.ReadAsync(header, 0, header.Length);
            
            string headerStr = System.Text.Encoding.UTF8.GetString(header);
            return headerStr == FileFormatVersion;
        }
        catch
        {
            return false;
        }
    }
}
