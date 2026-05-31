// StudyDatabase.cs — SQLite-backed persistence for study tools (highlights,
// notes, bookmarks). The DB lives in the app's local data folder and survives
// across sessions. Uses Microsoft.Data.Sqlite.
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace SentinelForge.Services;

public sealed record Highlight(long Id, string Chapter, string Text, string CreatedAt);
public sealed record Note(long Id, string Text, string CreatedAt);
public sealed record Bookmark(long Id, int ChapterIndex, string ChapterTitle, string CreatedAt);

/// <summary>An Eisenhower-matrix task. Quadrant: 0=Do, 1=Schedule, 2=Delegate, 3=Eliminate.</summary>
public sealed class EisenhowerTask
{
    public long Id { get; set; }
    public int Quadrant { get; set; }
    public string Text { get; set; } = "";
    public bool Done { get; set; }
    public string CreatedAt { get; set; } = "";
}

public sealed class StudyDatabase
{
    private readonly string _connStr;

    public StudyDatabase()
    {
        string dir;
        try
        {
            // Packaged app: per-user local app data folder.
            dir = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }
        catch
        {
            dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SentinelForge");
            Directory.CreateDirectory(dir);
        }

        var dbPath = Path.Combine(dir, "study.db");
        _connStr = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        Initialize();
    }

    private SqliteConnection Open()
    {
        var con = new SqliteConnection(_connStr);
        con.Open();
        return con;
    }

    private void Initialize()
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS highlights (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    doc_path TEXT NOT NULL, chapter TEXT, text TEXT NOT NULL, created_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS notes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    doc_path TEXT NOT NULL, text TEXT NOT NULL, created_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS bookmarks (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    doc_path TEXT NOT NULL, chapter_index INTEGER NOT NULL, chapter_title TEXT, created_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS tasks (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    quadrant INTEGER NOT NULL, text TEXT NOT NULL, done INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS library_zones (
    path TEXT PRIMARY KEY, zone TEXT NOT NULL);";
        cmd.ExecuteNonQuery();
    }

    private static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    // ---- Inserts ----

    public long AddHighlight(string docPath, string chapter, string text)
        => Insert("INSERT INTO highlights(doc_path,chapter,text,created_at) VALUES($p,$c,$t,$ts)",
            ("$p", docPath), ("$c", chapter), ("$t", text), ("$ts", Now()));

    public long AddNote(string docPath, string text)
        => Insert("INSERT INTO notes(doc_path,text,created_at) VALUES($p,$t,$ts)",
            ("$p", docPath), ("$t", text), ("$ts", Now()));

    public long AddBookmark(string docPath, int chapterIndex, string chapterTitle)
        => Insert("INSERT INTO bookmarks(doc_path,chapter_index,chapter_title,created_at) VALUES($p,$i,$c,$ts)",
            ("$p", docPath), ("$i", chapterIndex), ("$c", chapterTitle), ("$ts", Now()));

    private long Insert(string sql, params (string, object)[] ps)
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = sql + "; SELECT last_insert_rowid();";
        foreach (var (k, v) in ps) cmd.Parameters.AddWithValue(k, v);
        return (long)(cmd.ExecuteScalar() ?? 0L);
    }

    // ---- Queries (per document) ----

    public List<Highlight> GetHighlights(string docPath)
    {
        var list = new List<Highlight>();
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id,chapter,text,created_at FROM highlights WHERE doc_path=$p ORDER BY id DESC";
        cmd.Parameters.AddWithValue("$p", docPath);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Highlight(r.GetInt64(0), r.IsDBNull(1) ? "" : r.GetString(1), r.GetString(2), r.GetString(3)));
        return list;
    }

    public List<Note> GetNotes(string docPath)
    {
        var list = new List<Note>();
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id,text,created_at FROM notes WHERE doc_path=$p ORDER BY id DESC";
        cmd.Parameters.AddWithValue("$p", docPath);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Note(r.GetInt64(0), r.GetString(1), r.GetString(2)));
        return list;
    }

    public List<Bookmark> GetBookmarks(string docPath)
    {
        var list = new List<Bookmark>();
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id,chapter_index,chapter_title,created_at FROM bookmarks WHERE doc_path=$p ORDER BY id DESC";
        cmd.Parameters.AddWithValue("$p", docPath);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Bookmark(r.GetInt64(0), r.GetInt32(1), r.IsDBNull(2) ? "" : r.GetString(2), r.GetString(3)));
        return list;
    }

    // ---- Eisenhower tasks (global, not per-document) ----

    public long AddTask(int quadrant, string text)
        => Insert("INSERT INTO tasks(quadrant,text,done,created_at) VALUES($q,$t,0,$ts)",
            ("$q", quadrant), ("$t", text), ("$ts", Now()));

    public List<EisenhowerTask> GetTasks()
    {
        var list = new List<EisenhowerTask>();
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id,quadrant,text,done,created_at FROM tasks ORDER BY id ASC";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new EisenhowerTask
            {
                Id = r.GetInt64(0), Quadrant = r.GetInt32(1), Text = r.GetString(2),
                Done = r.GetInt32(3) != 0, CreatedAt = r.GetString(4)
            });
        return list;
    }

    public void SetTaskDone(long id, bool done)
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE tasks SET done=$d WHERE id=$id";
        cmd.Parameters.AddWithValue("$d", done ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void DeleteTask(long id) => Delete("tasks", id);

    // ---- Library zones (GREEN/YELLOW/RED per file path) ----

    public void SetZone(string path, string zone)
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        if (string.IsNullOrEmpty(zone))
            cmd.CommandText = "DELETE FROM library_zones WHERE path=$p";
        else
            cmd.CommandText = "INSERT INTO library_zones(path,zone) VALUES($p,$z) ON CONFLICT(path) DO UPDATE SET zone=$z";
        cmd.Parameters.AddWithValue("$p", path);
        if (!string.IsNullOrEmpty(zone)) cmd.Parameters.AddWithValue("$z", zone);
        cmd.ExecuteNonQuery();
    }

    public Dictionary<string, string> GetAllZones()
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT path,zone FROM library_zones";
        using var r = cmd.ExecuteReader();
        while (r.Read()) d[r.GetString(0)] = r.GetString(1);
        return d;
    }

    // ---- Deletes ----

    public void Delete(string table, long id)
    {
        if (table is not ("highlights" or "notes" or "bookmarks" or "tasks")) return; // guard against injection
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = $"DELETE FROM {table} WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}
