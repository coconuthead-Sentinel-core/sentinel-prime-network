using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using SentinelForge.Services;
using Windows.Storage.Pickers;
using Windows.UI; // Color.FromArgb

namespace SentinelForge.ViewModels;

/// <summary>
/// ViewModel for the reading page. The UI is C#/WinUI; the document engine is
/// native C++ (SentinelForgeCore.dll) reached through <see cref="SentinelCore"/>.
/// </summary>
public partial class MainPageViewModel : ObservableObject
{
    /// <summary>Runtime proof the native C++ engine loaded and answered.</summary>
    [ObservableProperty]
    public partial string EngineStatus { get; set; } = "Checking C++ engine…";

    /// <summary>Name of the currently open document (or a hint).</summary>
    [ObservableProperty]
    public partial string DocumentTitle { get; set; } = "No document open";

    /// <summary>Text currently shown in the reading pane (the selected chapter).</summary>
    [ObservableProperty]
    public partial string DocumentText { get; set; } =
        "Click “Open Document” to load a file. Plain text & HTML are parsed by the "
        + "native C++ engine; PDF and Word docs by proven C# libraries. Chapters are "
        + "detected by the C++ engine. Use A− / A+ to size the text, and Read Aloud "
        + "to have Windows speak it.";

    /// <summary>Reading-pane font size (bound to the text block).</summary>
    [ObservableProperty]
    public partial double FontSize { get; set; } = 16;

    /// <summary>Detected chapters for the open document (for the jump dropdown).</summary>
    public ObservableCollection<SentinelCore.ChapterInfo> Chapters { get; } = new();

    /// <summary>Index of the chapter currently being shown.</summary>
    [ObservableProperty]
    public partial int SelectedChapterIndex { get; set; }

    private string _fullText = "";
    private string _currentDocPath = "";

    // Lazily created: avoid constructing speech/media objects during startup.
    private ReadAloudService? _readerInstance;
    private ReadAloudService Reader => _readerInstance ??= new ReadAloudService();

    // Lazily created: opens/initializes the SQLite DB on first study action.
    private StudyDatabase? _dbInstance;
    private StudyDatabase Db => _dbInstance ??= new StudyDatabase();

    /// <summary>Persisted study items for the open document.</summary>
    public ObservableCollection<Highlight> Highlights { get; } = new();
    public ObservableCollection<Note> Notes { get; } = new();
    public ObservableCollection<Bookmark> Bookmarks { get; } = new();

    /// <summary>Text typed into the note box before saving.</summary>
    [ObservableProperty]
    public partial string NoteDraft { get; set; } = "";

    // On-device dictation (Whisper). Lazy so mic/model init only on first use.
    private DictationService? _dictationInstance;
    private DictationService Dictation => _dictationInstance ??= new DictationService();

    [ObservableProperty] public partial bool IsDictating { get; set; }
    [ObservableProperty] public partial string DictationStatus { get; set; } = "";

    // ---- Sentinel session / cognitive-load zone (ported from the original app) ----

    /// <summary>Self-reported energy 1–10, driving the GREEN/YELLOW/RED zone.</summary>
    [ObservableProperty] public partial int EnergyLevel { get; set; } = 7;

    /// <summary>The one primary focus for this session (Sentinel spec: pick ONE).</summary>
    [ObservableProperty] public partial string PrimaryTask { get; set; } = "";

    /// <summary>Chosen action for the focus: "Do Now" (green) / "Wait" (yellow) / "Done" (red).</summary>
    [ObservableProperty] public partial string SessionAction { get; set; } = "";

    public string SessionActionDisplay => string.IsNullOrEmpty(SessionAction) ? "" : "   ·   " + SessionAction;

    partial void OnSessionActionChanged(string value) => OnPropertyChanged(nameof(SessionActionDisplay));

    public string ZoneName => EnergyLevel >= 7 ? "GREEN ZONE" : EnergyLevel >= 4 ? "YELLOW ZONE" : "RED ZONE";

    public SolidColorBrush ZoneBrush => new(
        EnergyLevel >= 7 ? Color.FromArgb(255, 46, 125, 50)   // green
        : EnergyLevel >= 4 ? Color.FromArgb(255, 245, 168, 37) // amber
        : Color.FromArgb(255, 198, 40, 40));                   // red

    public string EnergySummary => $"Energy {EnergyLevel}/10";

    // Ω1 always on; Joy active at load ≥ 7; Coconut always on.
    public string ProtocolSummary => $"Ω1 ✓    Joy {(EnergyLevel >= 7 ? "✓" : "—")}    Coconut ✓";

    public string PrimaryTaskDisplay => string.IsNullOrWhiteSpace(PrimaryTask) ? "(no focus set)" : PrimaryTask;

    partial void OnEnergyLevelChanged(int value)
    {
        OnPropertyChanged(nameof(ZoneName));
        OnPropertyChanged(nameof(ZoneBrush));
        OnPropertyChanged(nameof(EnergySummary));
        OnPropertyChanged(nameof(ProtocolSummary));
    }

    partial void OnPrimaryTaskChanged(string value) => OnPropertyChanged(nameof(PrimaryTaskDisplay));

    /// <summary>Apply the results of the Session Start wizard.</summary>
    public void ApplySession(int energy, string primaryTask)
    {
        EnergyLevel = Math.Clamp(energy, 1, 10);
        PrimaryTask = primaryTask?.Trim() ?? "";
    }

    public MainPageViewModel()
    {
        try
        {
            int v = SentinelCore.Version();
            EngineStatus = $"✅ Native C++ engine v{v} connected";
        }
        catch (Exception ex)
        {
            EngineStatus = "⚠ C++ engine failed to load: " + ex.Message;
        }
    }

    /// <summary>Pick a file and load it through the format-aware loader.</summary>
    [RelayCommand]
    private async Task OpenDocumentAsync()
    {
        var picker = new FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        foreach (var ext in new[] { ".txt", ".md", ".log", ".pdf", ".docx", ".html", ".htm" })
            picker.FileTypeFilter.Add(ext);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        await LoadDocumentAsync(file.Path, file.Name);
    }

    /// <summary>
    /// Shared document-load path used by the Open button AND by file/Jump-List
    /// activation. Routes the format to the right engine, detects chapters via the
    /// C++ engine, then fires the Win11 toast + Jump List.
    /// </summary>
    public async Task LoadDocumentAsync(string path, string displayName)
    {
        Reader.Stop();
        DocumentTitle = displayName;
        _currentDocPath = path;
        try
        {
            // Format-aware load: C++ engine for txt/html, C# libs for pdf/docx.
            _fullText = DocumentLoader.LoadText(path);
            NotificationService.Show("Document opened", displayName);
        }
        catch (Exception ex)
        {
            _fullText = ex.Message;
            NotificationService.Show("Couldn't open document", ex.Message);
        }

        BuildChapters();
        RefreshStudyLists();

        // Add to the Windows 11 taskbar Jump List (recent documents).
        await JumpListService.AddRecentAsync(path, displayName);
    }

    // ---- Study tools (SQLite-backed; persist across sessions) ----

    private void RefreshStudyLists()
    {
        Highlights.Clear();
        Notes.Clear();
        Bookmarks.Clear();
        if (string.IsNullOrEmpty(_currentDocPath)) return;
        try
        {
            foreach (var h in Db.GetHighlights(_currentDocPath)) Highlights.Add(h);
            foreach (var n in Db.GetNotes(_currentDocPath)) Notes.Add(n);
            foreach (var b in Db.GetBookmarks(_currentDocPath)) Bookmarks.Add(b);
        }
        catch { /* DB issues shouldn't break reading */ }
    }

    private string CurrentChapterTitle =>
        (SelectedChapterIndex >= 0 && SelectedChapterIndex < Chapters.Count)
            ? Chapters[SelectedChapterIndex].Title : "";

    /// <summary>Save the reading-pane text selection as a highlight (called from code-behind).</summary>
    public void AddHighlight(string? selectedText)
    {
        var text = selectedText?.Trim();
        if (string.IsNullOrEmpty(_currentDocPath) || string.IsNullOrEmpty(text)) return;
        Db.AddHighlight(_currentDocPath, CurrentChapterTitle, text);
        RefreshStudyLists();
        NotificationService.Show("Highlight saved", text.Length > 60 ? text[..60] + "…" : text);
    }

    /// <summary>Toggle dictation: start recording, or stop and transcribe into the note box.</summary>
    [RelayCommand]
    private async Task ToggleDictateAsync()
    {
        if (!IsDictating)
        {
            bool ok = await Dictation.RequestMicAccessAsync();
            if (!ok) { DictationStatus = "⚠ Microphone access denied."; return; }
            Dictation.StartRecording();
            IsDictating = true;
            DictationStatus = "🎤 Listening… click again to stop";
            return;
        }

        IsDictating = false;
        DictationStatus = "Transcribing…";
        try
        {
            var progress = new Progress<string>(s => DictationStatus = s);
            string text = await Dictation.StopAndTranscribeAsync(progress);
            if (!string.IsNullOrWhiteSpace(text))
            {
                NoteDraft = string.IsNullOrEmpty(NoteDraft) ? text : NoteDraft + " " + text;
                DictationStatus = "✅ Inserted into note — review and Save.";
            }
            else
            {
                DictationStatus = "No speech detected.";
            }
        }
        catch (Exception ex)
        {
            DictationStatus = "Dictation error: " + ex.Message;
        }
    }

    [RelayCommand]
    private void SaveNote()
    {
        var text = NoteDraft?.Trim();
        if (string.IsNullOrEmpty(_currentDocPath) || string.IsNullOrEmpty(text)) return;
        Db.AddNote(_currentDocPath, text);
        NoteDraft = "";
        RefreshStudyLists();
    }

    [RelayCommand]
    private void BookmarkChapter()
    {
        if (string.IsNullOrEmpty(_currentDocPath) || Chapters.Count == 0) return;
        Db.AddBookmark(_currentDocPath, SelectedChapterIndex, CurrentChapterTitle);
        RefreshStudyLists();
        NotificationService.Show("Bookmark added", CurrentChapterTitle);
    }

    /// <summary>Jump the reading view to a bookmarked chapter.</summary>
    public void GoToChapter(int index)
    {
        if (index >= 0 && index < Chapters.Count) SelectedChapterIndex = index;
    }

    [RelayCommand]
    private void DeleteHighlight(Highlight h) { Db.Delete("highlights", h.Id); RefreshStudyLists(); }

    [RelayCommand]
    private void DeleteNote(Note n) { Db.Delete("notes", n.Id); RefreshStudyLists(); }

    [RelayCommand]
    private void DeleteBookmark(Bookmark b) { Db.Delete("bookmarks", b.Id); RefreshStudyLists(); }

    /// <summary>Detect chapters (C++ engine) and reset the reading view to the top.</summary>
    private void BuildChapters()
    {
        Chapters.Clear();
        var detected = SentinelCore.DetectChapters(_fullText);
        if (detected.Count == 0)
        {
            Chapters.Add(new SentinelCore.ChapterInfo { Title = "Full document", Offset = 0 });
        }
        else
        {
            // If the text doesn't start with a heading, prepend an opening section.
            if (detected[0].Offset > 0)
                Chapters.Add(new SentinelCore.ChapterInfo { Title = "Beginning", Offset = 0 });
            foreach (var c in detected) Chapters.Add(c);
        }

        SelectedChapterIndex = 0;
        UpdateVisibleText();
    }

    partial void OnSelectedChapterIndexChanged(int value) => UpdateVisibleText();

    private void UpdateVisibleText()
    {
        if (Chapters.Count == 0 || string.IsNullOrEmpty(_fullText))
        {
            DocumentText = _fullText;
            return;
        }

        int i = Math.Clamp(SelectedChapterIndex, 0, Chapters.Count - 1);
        int start = Math.Clamp(Chapters[i].Offset, 0, _fullText.Length);
        int end = (i + 1 < Chapters.Count)
            ? Math.Clamp(Chapters[i + 1].Offset, start, _fullText.Length)
            : _fullText.Length;

        DocumentText = _fullText.Substring(start, end - start).Trim();
    }

    [RelayCommand]
    private void NextChapter()
    {
        if (SelectedChapterIndex < Chapters.Count - 1) SelectedChapterIndex++;
    }

    [RelayCommand]
    private void PreviousChapter()
    {
        if (SelectedChapterIndex > 0) SelectedChapterIndex--;
    }

    [RelayCommand]
    private void IncreaseFont() => FontSize = Math.Min(40, FontSize + 2);

    [RelayCommand]
    private void DecreaseFont() => FontSize = Math.Max(10, FontSize - 2);

    /// <summary>Read the visible chapter aloud using Windows speech synthesis.</summary>
    [RelayCommand]
    private async Task ReadAloudAsync() => await Reader.SpeakAsync(DocumentText);

    /// <summary>Stop read-aloud playback.</summary>
    [RelayCommand]
    private void StopReading() => Reader.Stop();

    // Dictation accessors so the Session Start dialog can voice-fill the focus field.
    public Task<bool> RequestMicAsync() => Dictation.RequestMicAccessAsync();
    public void BeginDictation() => Dictation.StartRecording();
    public Task<string> EndDictationAsync(IProgress<string>? progress = null) => Dictation.StopAndTranscribeAsync(progress);
}
