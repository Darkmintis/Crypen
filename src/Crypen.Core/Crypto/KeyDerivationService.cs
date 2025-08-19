using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Crypen.Core.Crypto;

/// <summary>
/// Service for deriving cryptographic keys from passwords
/// </summary>
public class KeyDerivationService
{
    // Argon2 parameters - chosen for good security while maintaining reasonable performance
    private const int SaltSizeBytes = 16;
    private const int DerivedKeySizeBytes = 32; // 256 bits for AES-256
    private const int MemorySize = 65536;       // 64 MB
    private const int Iterations = 4;
    private const int DegreeOfParallelism = 4;
    
    /// <summary>
    /// Derives a cryptographic key from a password using Argon2id
    /// </summary>
    /// <param name="password">The password to derive the key from</param>
    /// <param name="salt">The salt to use (will be generated if null)</param>
    /// <returns>A tuple containing the derived key and the salt used</returns>
    public static (byte[] DerivedKey, byte[] Salt) DeriveKey(string password, byte[]? salt = null)
    {
        // Generate a random salt if none was provided
        salt ??= RandomNumberGenerator.GetBytes(SaltSizeBytes);

        // Convert password to bytes
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        // Use Argon2id (hybrid of Argon2i and Argon2d) for best security
        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize,
        };

        // Derive the key
        byte[] derivedKey = argon2.GetBytes(DerivedKeySizeBytes);
        
        return (derivedKey, salt);
    }
}
