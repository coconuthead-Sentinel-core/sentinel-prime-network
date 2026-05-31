// core_c_api.cpp — implementation of the C ABI over the C++ DocumentReader.
#include "core_c_api.h"
#include "DocumentReader.h"

#include <cstring>
#include <string>

using namespace sentinel;

namespace {
// Allocate a caller-ownable copy of a wstring on the heap.
const wchar_t* dupString(const std::wstring& s) {
    wchar_t* out = new (std::nothrow) wchar_t[s.size() + 1];
    if (!out) return nullptr;
    std::wmemcpy(out, s.c_str(), s.size() + 1);
    return out;
}
}

extern "C" {

SF_API const wchar_t* sf_load_document_text(const wchar_t* path, const wchar_t** errorOut) {
    if (errorOut) *errorOut = nullptr;
    if (!path) {
        if (errorOut) *errorOut = dupString(L"Null path.");
        return nullptr;
    }
    DocumentReader reader;
    std::wstring err;
    auto doc = reader.load(std::filesystem::path(path), &err);
    if (!doc) {
        if (errorOut) *errorOut = dupString(err.empty() ? L"Unknown error." : err);
        return nullptr;
    }
    return dupString(doc->text);
}

SF_API int sf_detect_format(const wchar_t* path) {
    if (!path) return 0;
    return static_cast<int>(detectFormat(std::filesystem::path(path)));
}

SF_API const wchar_t* sf_detect_chapters_json(const wchar_t* text) {
    std::wstring json = L"[";
    if (text) {
        auto chapters = detectChapters(std::wstring(text));
        auto escape = [](const std::wstring& s) {
            std::wstring out;
            for (wchar_t c : s) {
                if (c == L'"' || c == L'\\') { out.push_back(L'\\'); out.push_back(c); }
                else if (c == L'\n' || c == L'\r' || c == L'\t') out.push_back(L' ');
                else out.push_back(c);
            }
            return out;
        };
        for (std::size_t i = 0; i < chapters.size(); ++i) {
            if (i) json += L",";
            json += L"{\"title\":\"" + escape(chapters[i].title)
                  + L"\",\"offset\":" + std::to_wstring(chapters[i].startOffset) + L"}";
        }
    }
    json += L"]";
    return dupString(json);
}

SF_API void sf_free_string(const wchar_t* str) {
    delete[] str;
}

SF_API int sf_core_version() {
    return 1;
}

}
