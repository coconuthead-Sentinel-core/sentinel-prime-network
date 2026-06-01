// DocumentReader.h — opens a file from disk and produces a Document.
//
// Mirrors the C# plan's "DocumentLoader" service. Plain C++ so it can be unit-
// tested and reused independently of the WinUI front end.
//
// Phase 1 scope: TXT is fully implemented now. PDF/DOCX/HTML are declared and
// will be filled in once the native parsing libraries are wired up (the C++
// counterparts to PdfPig / OpenXML SDK / HtmlAgilityPack).
#pragma once

#include "Document.h"
#include <filesystem>
#include <optional>
#include <string>

namespace sentinel {

// Detect the document format purely from the file extension (case-insensitive).
DocumentFormat detectFormat(const std::filesystem::path& path);

// Scan plain text for chapter/section headings (Markdown #, "Chapter N",
// "Part", "Prologue", etc.) and return them with their character offsets.
std::vector<Chapter> detectChapters(const std::wstring& text);

class DocumentReader {
public:
    // Load a file into a Document. Returns std::nullopt if the file cannot be
    // read (missing, unreadable). errorOut, if provided, receives a message.
    std::optional<Document> load(const std::filesystem::path& path,
                                 std::wstring* errorOut = nullptr);

private:
    // Format-specific loaders. Each fills doc.text (and chapters where known).
    bool loadTxt (const std::filesystem::path& path, Document& doc, std::wstring* err);
    bool loadPdf (const std::filesystem::path& path, Document& doc, std::wstring* err);
    bool loadDocx(const std::filesystem::path& path, Document& doc, std::wstring* err);
    bool loadHtml(const std::filesystem::path& path, Document& doc, std::wstring* err);
};

} // namespace sentinel
