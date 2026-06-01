// core_c_api.h — C ABI for the Sentinel Forge reading core.
//
// This exposes the C++ DocumentReader through a flat C interface so the C#
// WinUI app can call it via P/Invoke ([DllImport]). C ABI (not C++ name
// mangling) is what makes the C#<->C++ bridge stable and version-safe.
//
// Memory rule: any wchar_t* returned by sf_* must be freed with sf_free_string.
#pragma once

#ifdef SENTINELCORE_EXPORTS
#define SF_API __declspec(dllexport)
#else
#define SF_API __declspec(dllimport)
#endif

extern "C" {

// Load a document and return its full text as a UTF-16 string.
// Returns nullptr on failure; if errorOut is non-null it receives an error
// string (also caller-freed). Caller frees the result with sf_free_string.
SF_API const wchar_t* sf_load_document_text(const wchar_t* path, const wchar_t** errorOut);

// Detected format of a path: 0=Unknown 1=Txt 2=Pdf 3=Docx 4=Html.
SF_API int sf_detect_format(const wchar_t* path);

// Detect chapter/section headings in already-loaded text. Returns a JSON array
// string: [{"title":"...","offset":N}, ...]. Caller frees with sf_free_string.
// Works for text from ANY source (C++ core or C#-extracted PDF/DOCX).
SF_API const wchar_t* sf_detect_chapters_json(const wchar_t* text);

// Free a string previously returned by this DLL.
SF_API void sf_free_string(const wchar_t* str);

// Simple version probe so the C# side can confirm it loaded the right DLL.
SF_API int sf_core_version();

}
