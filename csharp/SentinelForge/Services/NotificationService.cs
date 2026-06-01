// NotificationService.cs — Windows 11 toast notifications via Windows App SDK.
// Registered once at startup; used to surface document-open results.
using System;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace SentinelForge.Services;

public static class NotificationService
{
    private static bool _registered;

    /// <summary>Register the app to send/receive toast notifications. Safe to call once at startup.</summary>
    public static void Register()
    {
        if (_registered) return;
        try
        {
            AppNotificationManager.Default.Register();
            _registered = true;
        }
        catch
        {
            // Notifications are a nice-to-have; never let registration crash the app.
        }
    }

    public static void Unregister()
    {
        if (!_registered) return;
        try { AppNotificationManager.Default.Unregister(); } catch { }
        _registered = false;
    }

    /// <summary>Show a simple two-line Windows 11 toast.</summary>
    public static void Show(string title, string message)
    {
        if (!_registered) return;
        try
        {
            var toast = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message)
                .BuildNotification();
            AppNotificationManager.Default.Show(toast);
        }
        catch
        {
            // Non-fatal.
        }
    }
}
