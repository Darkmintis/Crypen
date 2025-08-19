using Crypen.Core.Crypto;
using Crypen.Core.FileSystem;
using Crypen.Core.Security;
using Crypen.Core.Services;
using Crypen.Services;
using Crypen.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Crypen;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        // Set up dependency injection
        Services = ConfigureServices();
    }

    /// <summary>
    /// Gets the current App instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;
    
    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();
        
        // Process command line arguments if any
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        if (commandLineArgs.Length > 2)
        {
            string command = commandLineArgs[1].ToLowerInvariant();
            string path = commandLineArgs[2];
            
            if (command == "encrypt")
            {
                // Show encrypt dialog
                if (File.Exists(path))
                {
                    Window.ShowEncryptFileDialog(path);
                }
                else if (Directory.Exists(path))
                {
                    Window.ShowEncryptDirectoryDialog(path);
                }
                else if (path.EndsWith(":\\"))
                {
                    Window.ShowEncryptDriveDialog(path[0].ToString());
                }
            }
            else if (command == "decrypt")
            {
                if (File.Exists(path))
                {
                    Window.ShowDecryptDialog(path);
                }
            }
        }
    }

    /// <summary>
    /// Gets the main window of the app.
    /// </summary>
    public MainWindow Window { get; private set; }
    
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Get the application data folder
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Crypen");
        
        Directory.CreateDirectory(appDataPath);
        
        // Register core services
        services.AddSingleton<ISecureFileSystem, SecureFileSystem>();
        services.AddSingleton<ICryptoService, CryptoService>();
        
        // Register DPAPI password storage
        string passwordsPath = Path.Combine(appDataPath, "Passwords");
        services.AddSingleton(new PasswordStorageService(passwordsPath));
        
        // Register history service
        string historyPath = Path.Combine(appDataPath, "History");
        services.AddSingleton(new HistoryService(historyPath));
        
        // Register registry service for context menu integration
        string executablePath = Process.GetCurrentProcess().MainModule!.FileName;
        services.AddSingleton(new RegistryService(executablePath));
        
        // Register app service
        services.AddSingleton<AppService>();
        
        return services.BuildServiceProvider();
    }
}
