// JumpListService.cs — Windows 11 taskbar Jump List of recent documents.
// Right-click the app's taskbar/Start icon to see recently opened files.
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace SentinelForge.Services;

public static class JumpListService
{
    private const int MaxRecent = 10;

    /// <summary>Add (or promote) a document in the system Jump List's Recent group.</summary>
    public static async Task AddRecentAsync(string path, string displayName)
    {
        try
        {
            if (!JumpList.IsSupported()) return;

            var jumpList = await JumpList.LoadCurrentAsync();
            jumpList.SystemGroupKind = JumpListSystemGroupKind.None;

            // De-dupe: drop any existing entry for this path, then add to the top.
            var dupes = jumpList.Items.Where(i => string.Equals(i.Arguments, path, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var d in dupes) jumpList.Items.Remove(d);

            var item = JumpListItem.CreateWithArguments(path, displayName);
            item.Description = path;
            item.GroupName = "Recent";
            jumpList.Items.Insert(0, item);

            while (jumpList.Items.Count > MaxRecent)
                jumpList.Items.RemoveAt(jumpList.Items.Count - 1);

            await jumpList.SaveAsync();
        }
        catch
        {
            // Jump List is a convenience; never let it break document opening.
        }
    }
}
