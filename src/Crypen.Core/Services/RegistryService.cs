using Microsoft.Win32;

namespace Crypen.Core.Services;

/// <summary>
/// Service for managing Windows Registry integration for right-click menu
/// </summary>
public class RegistryService
{
    private const string CrypenFileExtension = ".crypen";
    private const string FileContextMenuKey = @"*\shell\EncryptWithCrypen";
    private const string DirectoryContextMenuKey = @"Directory\shell\EncryptWithCrypen";
    private const string DriveContextMenuKey = @"Drive\shell\EncryptWithCrypen";
    private const string CrypenFileContextMenuKey = @"CrypenEncrypted\shell\DecryptWithCrypen";
    
    private readonly string _executablePath;
    
    public RegistryService(string executablePath)
    {
        _executablePath = executablePath;
    }
    
    /// <summary>
    /// Registers Crypen in the Windows Registry for context menu integration
    /// </summary>
    /// <returns>True if registration succeeded, false otherwise</returns>
    public bool Register()
    {
        try
        {
            // Register file extension
            using (RegistryKey fileExtKey = Registry.ClassesRoot.CreateSubKey(CrypenFileExtension))
            {
                fileExtKey.SetValue("", "CrypenEncrypted");
            }
            
            // Register file type
            using (RegistryKey fileTypeKey = Registry.ClassesRoot.CreateSubKey("CrypenEncrypted"))
            {
                fileTypeKey.SetValue("", "Crypen Encrypted File");
                
                using (RegistryKey iconKey = fileTypeKey.CreateSubKey("DefaultIcon"))
                {
                    iconKey.SetValue("", $"{_executablePath},0");
                }
                
                using (RegistryKey shellKey = fileTypeKey.CreateSubKey("shell"))
                {
                    shellKey.SetValue("", "open");
                    
                    using (RegistryKey openKey = shellKey.CreateSubKey("open"))
                    {
                        openKey.SetValue("", "Unlock with Crypen");
                        
                        using (RegistryKey commandKey = openKey.CreateSubKey("command"))
                        {
                            commandKey.SetValue("", $"\"{_executablePath}\" decrypt \"%1\"");
                        }
                    }
                }
            }
            
            // Register context menu for files
            using (RegistryKey fileMenuKey = Registry.ClassesRoot.CreateSubKey(FileContextMenuKey))
            {
                fileMenuKey.SetValue("", "Encrypt with Crypen");
                fileMenuKey.SetValue("Icon", $"{_executablePath},0");
                
                using (RegistryKey commandKey = fileMenuKey.CreateSubKey("command"))
                {
                    commandKey.SetValue("", $"\"{_executablePath}\" encrypt \"%1\"");
                }
            }
            
            // Register context menu for directories
            using (RegistryKey dirMenuKey = Registry.ClassesRoot.CreateSubKey(DirectoryContextMenuKey))
            {
                dirMenuKey.SetValue("", "Encrypt with Crypen");
                dirMenuKey.SetValue("Icon", $"{_executablePath},0");
                
                using (RegistryKey commandKey = dirMenuKey.CreateSubKey("command"))
                {
                    commandKey.SetValue("", $"\"{_executablePath}\" encrypt \"%1\"");
                }
            }
            
            // Register context menu for drives
            using (RegistryKey driveMenuKey = Registry.ClassesRoot.CreateSubKey(DriveContextMenuKey))
            {
                driveMenuKey.SetValue("", "Encrypt with Crypen");
                driveMenuKey.SetValue("Icon", $"{_executablePath},0");
                
                using (RegistryKey commandKey = driveMenuKey.CreateSubKey("command"))
                {
                    commandKey.SetValue("", $"\"{_executablePath}\" encrypt \"%1\"");
                }
            }
            
            // Register context menu for Crypen files
            using (RegistryKey crypenMenuKey = Registry.ClassesRoot.CreateSubKey(CrypenFileContextMenuKey))
            {
                crypenMenuKey.SetValue("", "Decrypt with Crypen");
                crypenMenuKey.SetValue("Icon", $"{_executablePath},0");
                
                using (RegistryKey commandKey = crypenMenuKey.CreateSubKey("command"))
                {
                    commandKey.SetValue("", $"\"{_executablePath}\" decrypt \"%1\"");
                }
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Unregisters Crypen from the Windows Registry
    /// </summary>
    /// <returns>True if unregistration succeeded, false otherwise</returns>
    public bool Unregister()
    {
        try
        {
            // Remove file context menu
            Registry.ClassesRoot.DeleteSubKeyTree(FileContextMenuKey, false);
            
            // Remove directory context menu
            Registry.ClassesRoot.DeleteSubKeyTree(DirectoryContextMenuKey, false);
            
            // Remove drive context menu
            Registry.ClassesRoot.DeleteSubKeyTree(DriveContextMenuKey, false);
            
            // Remove file type association
            Registry.ClassesRoot.DeleteSubKeyTree("CrypenEncrypted", false);
            Registry.ClassesRoot.DeleteSubKeyTree(CrypenFileExtension, false);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
