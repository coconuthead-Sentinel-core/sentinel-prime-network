using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SentinelForge.ViewModels;

namespace SentinelForge;

/// <summary>Library page: browse documents and tag them with cognitive-load zones.</summary>
public sealed partial class LibraryPage : Page
{
    public LibraryPageViewModel ViewModel { get; } = new();

    public LibraryPage()
    {
        InitializeComponent();
    }

    private void OnSetZone(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is LibraryItem item && fe is Button b)
            ViewModel.SetZone(item, b.Tag as string ?? "");
    }

    private async void OnOpen(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is LibraryItem item
            && App.Window is MainWindow mw)
        {
            await mw.OpenInReader(item.Path);
        }
    }
}
