using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SentinelForge.Services;
using SentinelForge.ViewModels;

namespace SentinelForge;

/// <summary>Productivity page: Pomodoro timer + Eisenhower matrix.</summary>
public sealed partial class FocusPage : Page
{
    public FocusPageViewModel ViewModel { get; } = new();

    public FocusPage()
    {
        InitializeComponent();
    }

    private void OnTaskChecked(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is EisenhowerTask t)
            ViewModel.ToggleTask(t, true);
    }

    private void OnTaskUnchecked(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is EisenhowerTask t)
            ViewModel.ToggleTask(t, false);
    }

    private void OnDeleteTask(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is EisenhowerTask t)
            ViewModel.DeleteTaskCommand.Execute(t);
    }
}
