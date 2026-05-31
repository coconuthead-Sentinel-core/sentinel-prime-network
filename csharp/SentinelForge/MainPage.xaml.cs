using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
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

        // On first show: run the Session Start wizard, then (if launched with a
        // file via association / Jump List) open that document.
        Loaded += async (_, _) =>
        {
            if (!_sessionPrompted)
            {
                _sessionPrompted = true;
                await ShowSessionStartAsync();
            }

            var path = App.LaunchFilePath;
            if (!string.IsNullOrEmpty(path))
            {
                App.LaunchFilePath = null;
                await ViewModel.LoadDocumentAsync(path, System.IO.Path.GetFileName(path));
            }
        };
    }

    private bool _sessionPrompted;

    private async void OnNewSession(object sender, RoutedEventArgs e) => await ShowSessionStartAsync();

    /// <summary>The Sentinel "Session Start" wizard: energy → zone, one focus, protocols.</summary>
    private async Task ShowSessionStartAsync()
    {
        var slider = new Slider { Minimum = 1, Maximum = 10, StepFrequency = 1, Value = ViewModel.EnergyLevel, Width = 280 };
        var zoneText = new TextBlock { FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        void UpdateZone()
        {
            int en = (int)slider.Value;
            zoneText.Text = en >= 7 ? "→ GREEN" : en >= 4 ? "→ YELLOW" : "→ RED";
        }
        UpdateZone();
        slider.ValueChanged += (_, __) => UpdateZone();

        var taskBox = new TextBox
        {
            PlaceholderText = "Pick ONE focus…",
            Text = ViewModel.PrimaryTask,
            AcceptsReturn = false,
        };

        var panel = new StackPanel { Spacing = 10, MinWidth = 360 };
        panel.Children.Add(new TextBlock { Text = "How's your energy right now? (1–10)" });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        row.Children.Add(slider);
        row.Children.Add(zoneText);
        panel.Children.Add(row);
        panel.Children.Add(new TextBlock { Text = "One primary task for this session", Margin = new Thickness(0, 6, 0, 0) });
        panel.Children.Add(taskBox);
        panel.Children.Add(new TextBlock
        {
            Text = "Active protocols:  Ω1 always on    ·    Joy at load ≥ 7    ·    Coconut always on",
            Opacity = 0.7,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
        });

        var dialog = new ContentDialog
        {
            Title = "Session Start",
            Content = panel,
            PrimaryButtonText = "Begin Session",
            CloseButtonText = "Skip for now",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            ViewModel.ApplySession((int)slider.Value, taskBox.Text);
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
