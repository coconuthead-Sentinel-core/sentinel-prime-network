using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using SentinelForge.Services;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SentinelForge;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The main application window. Use <c>App.Window</c> from any class that needs
    /// the window reference (for dialogs, pickers, interop, etc.).
    /// </summary>
    public static Window Window { get; private set; } = null!;

    /// <summary>
    /// The UI thread dispatcher. Use <c>App.DispatcherQueue</c> to marshal calls
    /// to the UI thread. Fully qualified to avoid CS0104 ambiguity with
    /// <see cref="Windows.System.DispatcherQueue"/>.
    /// </summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;

    /// <summary>
    /// The native window handle (HWND). Use for file pickers,
    /// <c>DataTransferManager</c>, and any WinRT interop that requires
    /// <c>InitializeWithWindow</c>.
    /// </summary>
    public static nint WindowHandle =>
        WinRT.Interop.WindowNative.GetWindowHandle(Window);

    /// <summary>
    /// A file path the app was activated with (file association double-click or a
    /// Jump List recent item). MainPage reads and clears this on load.
    /// </summary>
    public static string? LaunchFilePath { get; set; }

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Capture any unhandled startup/runtime exception to a log we can read.
        UnhandledException += (_, e) => LogCrash("UI", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogCrash("Domain", e.ExceptionObject as Exception);

        // Windows 11 ecosystem: register for toast notifications.
        NotificationService.Register();

        // Determine how we were activated — open a file if launched with one
        // (file association, or a Jump List recent item passing the path).
        CaptureLaunchFile();

        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }

    private static void LogCrash(string source, Exception? ex)
    {
        try
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sf_crash.log");
            System.IO.File.WriteAllText(path, $"[{source}] {DateTime.Now:O}\n{ex}\n");
        }
        catch { }
    }

    private static void CaptureLaunchFile()
    {
        try
        {
            var activated = AppInstance.GetCurrent().GetActivatedEventArgs();
            switch (activated.Kind)
            {
                case ExtendedActivationKind.File
                    when activated.Data is IFileActivatedEventArgs fileArgs
                         && fileArgs.Files.Count > 0:
                    LaunchFilePath = fileArgs.Files[0].Path;
                    break;

                case ExtendedActivationKind.Launch
                    when activated.Data is ILaunchActivatedEventArgs launchArgs
                         && !string.IsNullOrWhiteSpace(launchArgs.Arguments)
                         && System.IO.File.Exists(launchArgs.Arguments):
                    // Jump List recent items launch the app with the path as arguments.
                    LaunchFilePath = launchArgs.Arguments;
                    break;
            }
        }
        catch
        {
            // Activation parsing is best-effort; normal launch just opens empty.
        }
    }
}
