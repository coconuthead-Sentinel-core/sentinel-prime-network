using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SentinelForge;

/// <summary>
/// The application window. Hosts a NavigationView that switches between the
/// Reader and Focus pages. Page instances are cached so their state (e.g. the
/// open document) survives switching tabs.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MainPage? _readerPage;
    private FocusPage? _focusPage;
    private LibraryPage? _libraryPage;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        // Start on the Reader.
        Nav.SelectedItem = Nav.MenuItems[0];
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        ContentFrame.Content = (item.Tag as string) switch
        {
            "focus"   => _focusPage ??= new FocusPage(),
            "library" => _libraryPage ??= new LibraryPage(),
            _         => _readerPage ??= new MainPage(),
        };
    }

    /// <summary>Open a document in the Reader (called from the Library page).</summary>
    public async System.Threading.Tasks.Task OpenInReader(string path)
    {
        _readerPage ??= new MainPage();
        ContentFrame.Content = _readerPage;
        Nav.SelectedItem = Nav.MenuItems[0]; // visually select Reader
        await _readerPage.ViewModel.LoadDocumentAsync(path, System.IO.Path.GetFileName(path));
    }
}
