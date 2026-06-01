using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SentinelForge.Services;

namespace SentinelForge.ViewModels;

/// <summary>
/// Productivity page: a Pomodoro focus timer (work/break cycles) and an
/// Eisenhower matrix (4-quadrant task board) persisted in SQLite.
/// </summary>
public partial class FocusPageViewModel : ObservableObject
{
    // Pomodoro phase durations (seconds).
    private const int WorkSeconds = 25 * 60;
    private const int ShortBreakSeconds = 5 * 60;
    private const int LongBreakSeconds = 15 * 60;

    private enum Phase { Work, ShortBreak, LongBreak }

    private readonly DispatcherTimer _timer;
    private Phase _phase = Phase.Work;
    private int _remaining = WorkSeconds;

    private StudyDatabase? _dbInstance;
    private StudyDatabase Db => _dbInstance ??= new StudyDatabase();

    // ---- Pomodoro bindable state ----
    [ObservableProperty] public partial string TimeDisplay { get; set; } = "25:00";
    [ObservableProperty] public partial string PhaseLabel { get; set; } = "Focus";
    [ObservableProperty] public partial string StartPauseLabel { get; set; } = "Start";
    [ObservableProperty] public partial int CompletedSessions { get; set; }

    private bool _running;

    // ---- Eisenhower bindable state ----
    public ObservableCollection<EisenhowerTask> Quadrant0 { get; } = new(); // Do
    public ObservableCollection<EisenhowerTask> Quadrant1 { get; } = new(); // Schedule
    public ObservableCollection<EisenhowerTask> Quadrant2 { get; } = new(); // Delegate
    public ObservableCollection<EisenhowerTask> Quadrant3 { get; } = new(); // Eliminate

    [ObservableProperty] public partial string NewTaskText { get; set; } = "";
    /// <summary>Selected quadrant for a new task (ComboBox SelectedIndex).</summary>
    [ObservableProperty] public partial int NewTaskQuadrant { get; set; }

    public FocusPageViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
        LoadTasks();
    }

    // ================= Pomodoro =================

    [RelayCommand]
    private void StartPause()
    {
        _running = !_running;
        if (_running) { _timer.Start(); StartPauseLabel = "Pause"; }
        else { _timer.Stop(); StartPauseLabel = "Start"; }
    }

    [RelayCommand]
    private void ResetTimer()
    {
        _timer.Stop();
        _running = false;
        _phase = Phase.Work;
        _remaining = WorkSeconds;
        PhaseLabel = "Focus";
        StartPauseLabel = "Start";
        UpdateTimeDisplay();
    }

    private void OnTick(object? sender, object e)
    {
        if (_remaining > 0)
        {
            _remaining--;
            UpdateTimeDisplay();
            return;
        }
        AdvancePhase();
    }

    private void AdvancePhase()
    {
        if (_phase == Phase.Work)
        {
            CompletedSessions++;
            NotificationService.Show("Focus session complete", "Time for a break.");
            // Every 4th work session earns a long break.
            if (CompletedSessions % 4 == 0) { _phase = Phase.LongBreak; _remaining = LongBreakSeconds; PhaseLabel = "Long Break"; }
            else { _phase = Phase.ShortBreak; _remaining = ShortBreakSeconds; PhaseLabel = "Short Break"; }
        }
        else
        {
            NotificationService.Show("Break over", "Back to focus.");
            _phase = Phase.Work;
            _remaining = WorkSeconds;
            PhaseLabel = "Focus";
        }
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
        => TimeDisplay = $"{_remaining / 60:00}:{_remaining % 60:00}";

    // ================= Eisenhower matrix =================

    /// <summary>Reload tasks from the DB (called when the Focus page becomes visible).</summary>
    public void ReloadTasks() => LoadTasks();

    private void LoadTasks()
    {
        Quadrant0.Clear(); Quadrant1.Clear(); Quadrant2.Clear(); Quadrant3.Clear();
        try
        {
            foreach (var t in Db.GetTasks())
                BucketFor(t.Quadrant).Add(t);
        }
        catch { /* DB issues shouldn't break the page */ }
    }

    private ObservableCollection<EisenhowerTask> BucketFor(int q) => q switch
    {
        0 => Quadrant0,
        1 => Quadrant1,
        2 => Quadrant2,
        _ => Quadrant3,
    };

    [RelayCommand]
    private void AddTask()
    {
        var text = NewTaskText?.Trim();
        if (string.IsNullOrEmpty(text)) return;
        int q = Math.Clamp(NewTaskQuadrant, 0, 3);
        Db.AddTask(q, text);
        NewTaskText = "";
        LoadTasks();
    }

    public void ToggleTask(EisenhowerTask task, bool done)
    {
        Db.SetTaskDone(task.Id, done);
        task.Done = done; // keep in-memory item in sync
    }

    [RelayCommand]
    private void DeleteTask(EisenhowerTask task)
    {
        Db.DeleteTask(task.Id);
        LoadTasks();
    }
}
