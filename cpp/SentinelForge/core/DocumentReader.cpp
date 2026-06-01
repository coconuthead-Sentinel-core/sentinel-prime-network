// DocumentReader.cpp — implementation. See DocumentReader.h.
#include "DocumentReader.h"

#include <algorithm>
#include <cctype>
#include <fstream>
#include <sstream>

namespace sentinel {

namespace {

// Lower-case a (narrow) extension string for comparison.
std::string toLower(std::string s) {
    std::transform(s.begin(), s.end(), s.begin(),
                   [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
    return s;
}

// Read an entire file as raw bytes.
std::optional<std::string> readAllBytes(const std::filesystem::path& path) {
    std::ifstream in(path, std::ios::binary);
    if (!in) return std::nullopt;
    std::ostringstream ss;
    ss << in.rdbuf();
    return ss.str();
}

// Minimal UTF-8 -> UTF-16 (wstring) conversion good enough for text display.
// Falls back to a byte-wise copy for invalid sequences so we never throw.
std::wstring utf8ToWide(const std::string& bytes) {
    std::wstring out;
    out.reserve(bytes.size());
    std::size_t i = 0;
    const std::size_t n = bytes.size();
    // Skip a leading UTF-8 byte-order mark (EF BB BF) if present.
    if (n >= 3 && static_cast<unsigned char>(bytes[0]) == 0xEF
               && static_cast<unsigned char>(bytes[1]) == 0xBB
               && static_cast<unsigned char>(bytes[2]) == 0xBF) {
        i = 3;
    }
    while (i < n) {
        unsigned char c = static_cast<unsigned char>(bytes[i]);
        char32_t cp;
        std::size_t len;
        if (c < 0x80)            { cp = c;            len = 1; }
        else if ((c >> 5) == 0x6){ cp = c & 0x1F;     len = 2; }
        else if ((c >> 4) == 0xE){ cp = c & 0x0F;     len = 3; }
        else if ((c >> 3) == 0x1E){cp = c & 0x07;     len = 4; }
        else                     { out.push_back(static_cast<wchar_t>(c)); ++i; continue; }

        if (i + len > n) { out.push_back(static_cast<wchar_t>(c)); ++i; continue; }
        bool ok = true;
        for (std::size_t k = 1; k < len; ++k) {
            unsigned char cc = static_cast<unsigned char>(bytes[i + k]);
            if ((cc >> 6) != 0x2) { ok = false; break; }
            cp = (cp << 6) | (cc & 0x3F);
        }
        if (!ok) { out.push_back(static_cast<wchar_t>(c)); ++i; continue; }

        // Encode code point as UTF-16 (wchar_t is 16-bit on Windows).
        if (cp <= 0xFFFF) {
            out.push_back(static_cast<wchar_t>(cp));
        } else {
            cp -= 0x10000;
            out.push_back(static_cast<wchar_t>(0xD800 + (cp >> 10)));
            out.push_back(static_cast<wchar_t>(0xDC00 + (cp & 0x3FF)));
        }
        i += len;
    }
    return out;
}

} // namespace

DocumentFormat detectFormat(const std::filesystem::path& path) {
    const std::string ext = toLower(path.extension().string());
    if (ext == ".txt" || ext == ".md" || ext == ".log") return DocumentFormat::Txt;
    if (ext == ".pdf")                   return DocumentFormat::Pdf;
    if (ext == ".docx")                  return DocumentFormat::Docx;
    if (ext == ".html" || ext == ".htm") return DocumentFormat::Html;
    return DocumentFormat::Unknown;
}

std::vector<Chapter> detectChapters(const std::wstring& text) {
    std::vector<Chapter> chapters;
    const std::size_t n = text.size();

    auto lowerTrim = [](const std::wstring& s) {
        std::size_t b = s.find_first_not_of(L" \t\r");
        std::size_t e = s.find_last_not_of(L" \t\r");
        std::wstring t = (b == std::wstring::npos) ? L"" : s.substr(b, e - b + 1);
        std::wstring lw = t;
        std::transform(lw.begin(), lw.end(), lw.begin(),
                       [](wchar_t c){ return static_cast<wchar_t>(::towlower(c)); });
        return std::make_pair(t, lw);
    };

    auto startsWith = [](const std::wstring& s, const wchar_t* p) {
        return s.rfind(p, 0) == 0;
    };

    std::size_t lineStart = 0;
    while (lineStart <= n) {
        std::size_t lineEnd = text.find(L'\n', lineStart);
        if (lineEnd == std::wstring::npos) lineEnd = n;

        auto [trimmed, lw] = lowerTrim(text.substr(lineStart, lineEnd - lineStart));
        if (!trimmed.empty()) {
            bool isHeading = false;
            std::wstring title = trimmed;

            if (trimmed[0] == L'#') {
                // Markdown heading: strip leading #'s and a space.
                std::size_t p = trimmed.find_first_not_of(L'#');
                if (p != std::wstring::npos) {
                    title = trimmed.substr(p);
                    std::size_t b = title.find_first_not_of(L" \t");
                    title = (b == std::wstring::npos) ? L"" : title.substr(b);
                    isHeading = !title.empty();
                }
            } else if (trimmed.size() <= 80 &&
                       (startsWith(lw, L"chapter") || startsWith(lw, L"part ") ||
                        startsWith(lw, L"prologue") || startsWith(lw, L"epilogue") ||
                        startsWith(lw, L"introduction") || startsWith(lw, L"section ") ||
                        startsWith(lw, L"book ") || startsWith(lw, L"appendix"))) {
                isHeading = true;
            }

            if (isHeading && !title.empty())
                chapters.push_back(Chapter{title, lineStart});
        }

        if (lineEnd == n) break;
        lineStart = lineEnd + 1;
    }
    return chapters;
}

std::optional<Document> DocumentReader::load(const std::filesystem::path& path,
                                             std::wstring* errorOut) {
    if (!std::filesystem::exists(path)) {
        if (errorOut) *errorOut = L"File does not exist.";
        return std::nullopt;
    }

    Document doc;
    doc.path   = path;
    doc.format = detectFormat(path);
    doc.title  = path.stem().wstring();

    bool ok = false;
    switch (doc.format) {
        case DocumentFormat::Txt:  ok = loadTxt(path, doc, errorOut);  break;
        case DocumentFormat::Pdf:  ok = loadPdf(path, doc, errorOut);  break;
        case DocumentFormat::Docx: ok = loadDocx(path, doc, errorOut); break;
        case DocumentFormat::Html: ok = loadHtml(path, doc, errorOut); break;
        default:
            if (errorOut) *errorOut = L"Unsupported file type.";
            return std::nullopt;
    }

    if (!ok) return std::nullopt;
    return doc;
}

bool DocumentReader::loadTxt(const std::filesystem::path& path, Document& doc,
                             std::wstring* err) {
    auto bytes = readAllBytes(path);
    if (!bytes) {
        if (err) *err = L"Could not read the file.";
        return false;
    }
    doc.text = utf8ToWide(*bytes);
    return true;
}

// --- Not yet implemented: filled in as each parser library is wired up. ---
bool DocumentReader::loadPdf(const std::filesystem::path&, Document&, std::wstring* err) {
    if (err) *err = L"PDF support is not implemented yet (coming in a later step).";
    return false;
}
bool DocumentReader::loadDocx(const std::filesystem::path&, Document&, std::wstring* err) {
    if (err) *err = L"DOCX support is not implemented yet (coming in a later step).";
    return false;
}
bool DocumentReader::loadHtml(const std::filesystem::path& path, Document& doc, std::wstring* err) {
    auto bytes = readAllBytes(path);
    if (!bytes) {
        if (err) *err = L"Could not read the file.";
        return false;
    }
    std::string html = *bytes;
    const std::string lower = toLower(html);

    // 1) Remove <script>...</script> and <style>...</style> blocks entirely.
    auto stripBlock = [&](const std::string& tag) {
        const std::string open = "<" + tag;
        const std::string close = "</" + tag + ">";
        std::size_t pos = 0;
        std::string lo = toLower(html);
        while ((pos = lo.find(open, pos)) != std::string::npos) {
            std::size_t end = lo.find(close, pos);
            if (end == std::string::npos) { html.erase(pos); break; }
            end += close.size();
            html.erase(pos, end - pos);
            lo = toLower(html);
        }
    };
    stripBlock("script");
    stripBlock("style");
    stripBlock("title");

    // 2) Walk the markup: turn block-level tags into newlines, drop other tags.
    std::string out;
    out.reserve(html.size());
    for (std::size_t i = 0; i < html.size();) {
        if (html[i] == '<') {
            std::size_t end = html.find('>', i);
            if (end == std::string::npos) break;
            std::string tag = toLower(html.substr(i + 1, end - i - 1));
            // Block-level / line-breaking tags -> newline.
            const char* breakers[] = {"br", "/p", "p", "/div", "div", "/h1", "/h2",
                                      "/h3", "/h4", "/h5", "/h6", "/li", "li", "/tr",
                                      "tr", "/section", "hr"};
            for (const char* b : breakers) {
                if (tag == b || tag.rfind(std::string(b) + " ", 0) == 0) { out.push_back('\n'); break; }
            }
            i = end + 1;
        } else {
            out.push_back(html[i++]);
        }
    }

    // 3) Decode common HTML entities (named + numeric).
    std::string decoded;
    decoded.reserve(out.size());
    for (std::size_t i = 0; i < out.size();) {
        if (out[i] == '&') {
            std::size_t semi = out.find(';', i);
            if (semi != std::string::npos && semi - i <= 10) {
                std::string ent = out.substr(i + 1, semi - i - 1);
                std::string el = toLower(ent);
                if (el == "amp") decoded.push_back('&');
                else if (el == "lt") decoded.push_back('<');
                else if (el == "gt") decoded.push_back('>');
                else if (el == "quot") decoded.push_back('"');
                else if (el == "apos" || el == "#39") decoded.push_back('\'');
                else if (el == "nbsp") decoded.push_back(' ');
                else if (!ent.empty() && ent[0] == '#') {
                    int code = 0;
                    try { code = std::stoi(ent.substr(1)); } catch (...) { code = 0; }
                    if (code > 0 && code < 128) decoded.push_back(static_cast<char>(code));
                    else decoded += ent; // leave higher code points as-is-ish
                } else {
                    decoded.push_back('&'); decoded += ent; decoded.push_back(';');
                }
                i = semi + 1;
                continue;
            }
        }
        decoded.push_back(out[i++]);
    }

    // 4) Collapse runs of blank lines / trailing spaces for readability.
    std::string clean;
    clean.reserve(decoded.size());
    int blankRun = 0;
    std::string line;
    auto flush = [&](const std::string& ln) {
        // trim trailing spaces
        std::size_t e = ln.find_last_not_of(" \t\r");
        std::string t = (e == std::string::npos) ? "" : ln.substr(0, e + 1);
        if (t.empty()) { if (++blankRun <= 1) clean += "\n"; }
        else { blankRun = 0; clean += t; clean += "\n"; }
    };
    for (char c : decoded) {
        if (c == '\n') { flush(line); line.clear(); }
        else if (c != '\r') line.push_back(c);
    }
    flush(line);

    doc.text = utf8ToWide(clean);
    return true;
}

} // namespace sentinel
