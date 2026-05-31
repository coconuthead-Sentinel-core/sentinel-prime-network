// test_main.cpp — tiny console harness to prove the reading core works
// before any UI exists. Build with build_core.bat, then run:
//     SentinelCoreTest.exe "C:\path\to\some.txt"
// With no argument it writes a sample file and reads it back.
#include "DocumentReader.h"

#include <cstdio>
#include <fstream>
#include <io.h>
#include <fcntl.h>

using namespace sentinel;

int wmain(int argc, wchar_t** argv) {
    // Let the console print Unicode text.
    _setmode(_fileno(stdout), _O_U16TEXT);

    std::filesystem::path target;
    if (argc >= 2) {
        target = argv[1];
    } else {
        target = std::filesystem::temp_directory_path() / "sentinel_sample.txt";
        std::ofstream(target, std::ios::binary)
            << "Sentinel Forge\n\nThe reading core works.\n"
            << "This text was loaded by the C++ DocumentReader.\n";
        std::wprintf(L"(no file given — created sample: %ls)\n\n", target.c_str());
    }

    DocumentReader reader;
    std::wstring err;
    auto doc = reader.load(target, &err);
    if (!doc) {
        std::wprintf(L"FAILED to load: %ls\n", err.c_str());
        return 1;
    }

    std::wprintf(L"Title : %ls\n", doc->title.c_str());
    std::wprintf(L"Format: %d\n", static_cast<int>(doc->format));
    std::wprintf(L"Chars : %zu\n", doc->text.size());
    std::wprintf(L"--- content ---\n%ls\n", doc->text.c_str());
    return 0;
}
