using System.Security.Cryptography;
using System.Text;

namespace Crypen.Core.Security;

/// <summary>
/// Service for securely storing passwords using Windows Data Protection API (DPAPI)
/// </summary>
public class PasswordStorageService
{
    private readonly string _storagePath;
    
    public PasswordStorageService(string storageDirectory)
    {
        _storagePath = Path.Combine(storageDirectory, "passwords.dat");
        Directory.CreateDirectory(storageDirectory);
    }
    
    /// <summary>
    /// Saves a password for a specific encrypted item
    /// </summary>
    /// <param name="itemPath">Path to the encrypted item</param>
    /// <param name="password">Password to store</param>
    public void SavePassword(string itemPath, string password)
    {
        Dictionary<string, string> passwords = LoadPasswords();
        
        // Store the password for this item
        string itemId = GetItemId(itemPath);
        
        // Encrypt the password using DPAPI
        byte[] encryptedPassword = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(itemId),
            DataProtectionScope.CurrentUser);
        
        passwords[itemId] = Convert.ToBase64String(encryptedPassword);
        
        // Save the updated passwords dictionary
        SavePasswords(passwords);
    }
    
    /// <summary>
    /// Retrieves a stored password for a specific encrypted item
    /// </summary>
    /// <param name="itemPath">Path to the encrypted item</param>
    /// <returns>The stored password, or null if not found</returns>
    public string? GetPassword(string itemPath)
    {
        Dictionary<string, string> passwords = LoadPasswords();
        
        string itemId = GetItemId(itemPath);
        
        if (!passwords.TryGetValue(itemId, out string? encryptedPasswordBase64))
        {
            return null;
        }
        
        try
        {
            // Decrypt the password using DPAPI
            byte[] encryptedPassword = Convert.FromBase64String(encryptedPasswordBase64);
            byte[] passwordBytes = ProtectedData.Unprotect(
                encryptedPassword,
                Encoding.UTF8.GetBytes(itemId),
                DataProtectionScope.CurrentUser);
            
            return Encoding.UTF8.GetString(passwordBytes);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Removes a stored password
    /// </summary>
    /// <param name="itemPath">Path to the encrypted item</param>
    public void RemovePassword(string itemPath)
    {
        Dictionary<string, string> passwords = LoadPasswords();
        
        string itemId = GetItemId(itemPath);
        passwords.Remove(itemId);
        
        SavePasswords(passwords);
    }
    
    /// <summary>
    /// Lists all encrypted items with stored passwords
    /// </summary>
    /// <returns>A list of item paths</returns>
    public List<string> ListStoredItems()
    {
        Dictionary<string, string> passwords = LoadPasswords();
        return passwords.Keys.ToList();
    }
    
    private Dictionary<string, string> LoadPasswords()
    {
        if (!File.Exists(_storagePath))
        {
            return new Dictionary<string, string>();
        }
        
        try
        {
            string json = File.ReadAllText(_storagePath);
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
    
    private void SavePasswords(Dictionary<string, string> passwords)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(passwords);
        File.WriteAllText(_storagePath, json);
    }
    
    private string GetItemId(string itemPath)
    {
        // Use a normalized path as the identifier
        return itemPath.ToLowerInvariant().Replace('\\', '/');
    }
}
