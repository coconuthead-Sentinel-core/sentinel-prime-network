using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SentinelForge.Services;
using Windows.Storage.Pickers;

namespace SentinelForge.ViewModels;

/// <summary>One document in the library, with its cognitive-load zone.</summary>
public partial class LibraryItem : ObservableObject
{
    public string FileName { get; set; } = "";
    public string Path { get; set; } = "";
    public string Format { get; set; } = "";

    [ObservableProperty] public partial string Zone { get; set; } = "";  // "", Green, Yellow, Red

    public string ZoneDisplay => Zone switch
    {
        "Green" => "🟢 Green",
        "Yellow" => "🟡 Yellow",
        "Red" => "🔴 Red",
        _ => "— unsorted",
    };

    partial void OnZoneChanged(string value) => OnPropertyChanged(nameof(ZoneDisplay));
}

/// <summary>
/// Library page: scans a Books folder for documents and lets you tag each with a
/// GREEN / YELLOW / RED cognitive-load zone, persisted in SQLite. Filter by zone;
/// click a document to open it in the Reader.
/// </summary>
public partial class LibraryPageViewModel : ObservableObject
{
    private static readonly HashSet<string> SupportedExt =
        new(StringComparer.OrdinalIgnoreCase) { ".txt", ".md", ".log", ".pdf", ".docx", ".html", ".htm" };

    private readonly List<LibraryItem> _all = new();

    private StudyDatabase? _dbInstance;
    private StudyDatabase Db => _dbInstance ??= new StudyDatabase();

    [ObservableProperty] public partial string BooksFolder { get; set; } = DefaultBooksFolder();
    [ObservableProperty] public partial string StatusText { get; set; } = "";
    /// <summary>0=All, 1=Green, 2=Yellow, 3=Red.</summary>
    [ObservableProperty] public partial int ZoneFilter { get; set; }

    public ObservableCollection<LibraryItem> Items { get; } = new();

    public LibraryPageViewModel() => Scan();

    private static string DefaultBooksFolder()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = Path.Combine(profile, "OneDrive", "Desktop", "Books");
        if (Directory.Exists(candidate)) return candidate;
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return docs;
    }

    [RelayCommand]
    private void Refresh() => Scan();

    private void Scan()
    {
        _all.Clear();
        if (string.IsNullOrEmpty(BooksFolder) || !Directory.Exists(BooksFolder))
        {
            StatusText = "Folder not found — choose a folder to scan.";
            ApplyFilter();
            return;
        }

        var zones = Db.GetAllZones();
        try
        {
            foreach (var f in Directory.EnumerateFiles(BooksFolder, "*", SearchOption.TopDirectoryOnly))
            {
                if (!SupportedExt.Contains(Path.GetExtension(f))) continue;
                zones.TryGetValue(f, out var z);
                _all.Add(new LibraryItem
                {
                    FileName = Path.GetFileName(f),
                    Path = f,
                    Format = Path.GetExtension(f).TrimStart('.').ToUpperInvariant(),
                    Zone = z ?? "",
                });
            }
        }
        catch (Exception ex) { StatusText = "Scan error: " + ex.Message; }

        StatusText = $"{_all.Count} document(s) in {BooksFolder}";
        ApplyFilter();
    }

    partial void OnZoneFilterChanged(int value) => ApplyFilter();

    private void ApplyFilter()
    {
        string? want = ZoneFilter switch { 1 => "Green", 2 => "Yellow", 3 => "Red", _ => null };
        Items.Clear();
        foreach (var item in _all)
            if (want is null || item.Zone == want)
                Items.Add(item);
    }

    /// <summary>Assign (or clear) a document's zone and persist it.</summary>
    public void SetZone(LibraryItem item, string zone)
    {
        if (item.Zone == zone) zone = ""; // clicking the active zone clears it
        Db.SetZone(item.Path, zone);
        item.Zone = zone;
        if (ZoneFilter != 0) ApplyFilter(); // may move the item out of the filtered view
    }

    [RelayCommand]
    private async Task ChooseFolderAsync()
    {
        var picker = new FolderPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.FileTypeFilter.Add("*");
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) return;
        BooksFolder = folder.Path;
        Scan();
    }
}
