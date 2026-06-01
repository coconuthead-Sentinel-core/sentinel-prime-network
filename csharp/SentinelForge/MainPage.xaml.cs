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

    /// <summary>The Sentinel "Session Start" wizard: energy → zone, one focus (typed or dictated).</summary>
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
            PlaceholderText = "Pick ONE focus… (or use the mic)",
            Text = ViewModel.PrimaryTask,
            AcceptsReturn = false,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Microphone button: dictate the primary focus (offline Whisper).
        var dictStatus = new TextBlock { Opacity = 0.7, TextWrapping = TextWrapping.Wrap };
        var micButton = new Button
        {
            Content = "🎤",
            MinWidth = 44,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        ToolTipService.SetToolTip(micButton, "Dictate the focus");
        bool recording = false;
        micButton.Click += async (_, __) =>
        {
            if (!recording)
            {
                bool ok = await ViewModel.RequestMicAsync();
                if (!ok) { dictStatus.Text = "⚠ Microphone access denied."; return; }
                ViewModel.BeginDictation();
                recording = true;
                micButton.Content = "⏹";
                dictStatus.Text = "🎤 Listening… click again to stop";
            }
            else
            {
                recording = false;
                micButton.Content = "🎤";
                dictStatus.Text = "Transcribing…";
                string text = await ViewModel.EndDictationAsync(new System.Progress<string>(s => dictStatus.Text = s));
                if (!string.IsNullOrWhiteSpace(text))
                    taskBox.Text = string.IsNullOrEmpty(taskBox.Text) ? text : taskBox.Text + " " + text;
                dictStatus.Text = "";
            }
        };

        var taskRow = new Grid();
        taskRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        taskRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(taskBox, 0);
        Grid.SetColumn(micButton, 1);
        taskRow.Children.Add(taskBox);
        taskRow.Children.Add(micButton);

        var panel = new StackPanel { Spacing = 10, MinWidth = 380 };
        panel.Children.Add(new TextBlock { Text = "How's your energy right now? (1–10)" });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        row.Children.Add(slider);
        row.Children.Add(zoneText);
        panel.Children.Add(row);
        panel.Children.Add(new TextBlock { Text = "One primary task for this session", Margin = new Thickness(0, 6, 0, 0) });
        panel.Children.Add(taskRow);
        panel.Children.Add(dictStatus);

        // Focus — each colored button is the action you choose to begin with.
        panel.Children.Add(new TextBlock { Text = "Focus", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 0) });

        string? chosen = null;
        ContentDialog dialog = null!;

        Button ActionButton(string word, byte r, byte g, byte b)
        {
            var btn = new Button
            {
                Content = word,
                MinWidth = 120,
                Margin = new Thickness(0, 0, 8, 0),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b)),
                BorderThickness = new Thickness(0),
            };
            btn.Click += (_, __) => { chosen = word; dialog.Hide(); };
            return btn;
        }

        var actions = new StackPanel { Orientation = Orientation.Horizontal };
        actions.Children.Add(ActionButton("Do Now", 46, 125, 50));   // green
        actions.Children.Add(ActionButton("Wait", 245, 168, 37));    // yellow / amber
        actions.Children.Add(ActionButton("Done", 198, 40, 40));     // red
        panel.Children.Add(actions);

        dialog = new ContentDialog
        {
            Title = "Session Start",
            Content = panel,
            CloseButtonText = "Skip for now",
            XamlRoot = this.XamlRoot,
        };

        await dialog.ShowAsync();
        if (chosen != null)
        {
            ViewModel.ApplySession((int)slider.Value, taskBox.Text);
            ViewModel.SessionAction = chosen;
        }
    }

    // Save the current reading-pane text selection as a highlight.
    private void OnAddHighlight(object sender, RoutedEventArgs e)
        => ViewModel.AddHighlight(ReadingText.SelectedText);

    // Clicking a bookmark jumps the reading view to that chapter.
    private void OnBookmarkClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Bookmark b) ViewModel.GoToChapter(b.ChapterIndex);
    }

    private void OnDeleteHighlight(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is Highlight h)
            ViewModel.DeleteHighlightCommand.Execute(h);
    }

    private void OnDeleteNote(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is Note n)
            ViewModel.DeleteNoteCommand.Execute(n);
    }

    private void OnDeleteBookmark(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is Bookmark b)
            ViewModel.DeleteBookmarkCommand.Execute(b);
    }
}
