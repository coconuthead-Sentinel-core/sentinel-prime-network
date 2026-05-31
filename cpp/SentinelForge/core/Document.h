// Document.h — the core data model for an open book/document.
//
// This is plain, framework-independent C++ (no WinRT, no WinUI). It is the
// foundation that the C++/WinRT WinUI layer will sit on top of, mirroring the
// MVVM "Model" role from the C# plan (Book / Highlight / Bookmark models).
//
// Part of the Sentinel Forge Book Reader rewrite: Python/Tkinter -> C++/WinRT.
#pragma once

#include <string>
#include <vector>
#include <filesystem>

namespace sentinel {

// The kind of file we loaded, detected from the extension.
enum class DocumentFormat {
    Unknown,
    Txt,
    Pdf,
    Docx,
    Html,
};

// A single detected chapter / section heading and where it starts in the text.
struct Chapter {
    std::wstring title;
    std::size_t  startOffset = 0;  // index into Document::text
};

// An open document: its source path, detected format, full extracted text,
// and any chapters we found. The WinUI reading pane binds to this.
struct Document {
    std::filesystem::path     path;
    DocumentFormat            format = DocumentFormat::Unknown;
    std::wstring              title;     // display title (defaults to filename)
    std::wstring              text;      // full plain-text content
    std::vector<Chapter>      chapters;

    bool isEmpty() const { return text.empty(); }
};

} // namespace sentinel
