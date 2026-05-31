using Microsoft.UI.Xaml.Controls;
using SentinelForge.Services;
using SentinelForge.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SentinelForge;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();

        // If the app was launched by opening a file (file association) or a
        // Jump List recent item, load that document once the UI is ready.
        Loaded += async (_, _) =>
        {
            var path = App.LaunchFilePath;
            if (!string.IsNullOrEmpty(path))
            {
                App.LaunchFilePath = null;
                await ViewModel.LoadDocumentAsync(path, System.IO.Path.GetFileName(path));
            }
        };
    }

    // Save the current reading-pane text selection as a highlight.
    private void OnAddHighlight(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => ViewModel.AddHighlight(ReadingText.SelectedText);

    // Clicking a bookmark jumps the reading view to that chapter.
    private void OnBookmarkClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Bookmark b) ViewModel.GoToChapter(b.ChapterIndex);
    }

    private void OnDeleteHighlight(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (((Microsoft.UI.Xaml.FrameworkElement)sender).DataContext is Highlight h)
            ViewModel.DeleteHighlightCommand.Execute(h);
    }

    private void OnDeleteNote(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (((Microsoft.UI.Xaml.FrameworkElement)sender).DataContext is Note n)
            ViewModel.DeleteNoteCommand.Execute(n);
    }

    private void OnDeleteBookmark(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (((Microsoft.UI.Xaml.FrameworkElement)sender).DataContext is Bookmark b)
            ViewModel.DeleteBookmarkCommand.Execute(b);
    }
}
