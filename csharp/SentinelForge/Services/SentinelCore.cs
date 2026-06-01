// SentinelCore.cs — managed (C#) binding to the native C++ reading core
// (SentinelForgeCore.dll) via P/Invoke. This is the C#  <->  C++ bridge: the
// WinUI app stays in C#, the document engine runs in native C++.
//
// The DLL exposes a flat C ABI (see cpp/SentinelForge/core/core_c_api.h).
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SentinelForge.Services;

/// <summary>Thin, safe managed wrapper over the native C++ reading core.</summary>
public static class SentinelCore
{
    private const string Dll = "SentinelForgeCore.dll";

    [DllImport(Dll, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr sf_load_document_text(string path, out IntPtr errorOut);

    [DllImport(Dll, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int sf_detect_format(string path);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void sf_free_string(IntPtr str);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int sf_core_version();

    [DllImport(Dll, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr sf_detect_chapters_json(string text);

    public enum DocFormat { Unknown = 0, Txt = 1, Pdf = 2, Docx = 3, Html = 4 }

    /// <summary>A detected chapter/section heading and where it starts in the text.</summary>
    public sealed class ChapterInfo
    {
        public string Title { get; init; } = "";
        public int Offset { get; init; }
    }

    private sealed class ChapterDto
    {
        public string title { get; set; } = "";
        public int offset { get; set; }
    }

    /// <summary>
    /// Detect chapter/section headings in already-loaded text using the C++ engine.
    /// Works for text from any source (native core or C#-extracted PDF/DOCX).
    /// </summary>
    public static IReadOnlyList<ChapterInfo> DetectChapters(string text)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<ChapterInfo>();
        IntPtr p = sf_detect_chapters_json(text);
        if (p == IntPtr.Zero) return Array.Empty<ChapterInfo>();
        try
        {
            string json = Marshal.PtrToStringUni(p) ?? "[]";
            var dtos = JsonSerializer.Deserialize<List<ChapterDto>>(json) ?? new();
            var result = new List<ChapterInfo>(dtos.Count);
            foreach (var d in dtos)
                result.Add(new ChapterInfo { Title = d.title, Offset = d.offset });
            return result;
        }
        catch
        {
            return Array.Empty<ChapterInfo>();
        }
        finally
        {
            sf_free_string(p);
        }
    }

    /// <summary>Version of the loaded native core (sanity check the DLL is present).</summary>
    public static int Version() => sf_core_version();

    /// <summary>Detect a document's format from its path.</summary>
    public static DocFormat DetectFormat(string path) => (DocFormat)sf_detect_format(path);

    /// <summary>
    /// Load a document's full text via the native core. Throws on failure with
    /// the native error message. All native strings are freed here.
    /// </summary>
    public static string LoadDocumentText(string path)
    {
        IntPtr err = IntPtr.Zero;
        IntPtr result = sf_load_document_text(path, out err);
        try
        {
            if (result == IntPtr.Zero)
            {
                string message = err != IntPtr.Zero
                    ? Marshal.PtrToStringUni(err) ?? "Unknown native error."
                    : "Unknown native error.";
                throw new InvalidOperationException($"Native core failed to load '{path}': {message}");
            }
            return Marshal.PtrToStringUni(result) ?? string.Empty;
        }
        finally
        {
            if (result != IntPtr.Zero) sf_free_string(result);
            if (err != IntPtr.Zero) sf_free_string(err);
        }
    }
}
