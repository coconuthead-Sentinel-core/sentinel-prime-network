# Sentinel Forge Book Reader — C++/WinRT + WinUI rewrite

Native Windows rewrite of the Book Reader in **C++/WinRT + WinUI 3 (Windows App
SDK)**. This sits alongside the earlier `csharp/` attempt; the C++ version is the
chosen direction (decided 2026-05-30).

> The Python app (`book_reader/`) stays the working app until the C++ version
> reaches feature parity, phase by phase. Each phase ends with something runnable.

## Why staged

The Python app is one 8,628-line class — reading, 3 TTS backends, SQLite study
tools, Pomodoro, Eisenhower, library/zones, dictation. A full C++ rewrite is a
multi-session effort. We build the framework-independent **core** first (plain
C++, unit-testable), then wrap it in the WinUI UI layer.

## Layout

```
cpp/
  SentinelForge/
    core/                 # plain C++ — no WinRT. The reusable engine.
      Document.h          # the open-document data model
      DocumentReader.h/.cpp  # loads TXT (done) / PDF/DOCX/HTML (stubs)
      test_main.cpp       # console harness — proves the core runs with no UI
      build_core.bat      # compiles + runs the core test via MSVC
    app/                  # (next) C++/WinRT WinUI 3 app that hosts the core
```

## Status

| Piece | State |
|---|---|
| Reading core — TXT load | ✅ written (`core/`) |
| Reading core — PDF/DOCX/HTML | ⏳ stubbed, return "not implemented yet" |
| Console test harness | ✅ written — `build_core.bat` builds + runs it |
| **C++ toolset (MSVC + Windows SDK)** | ⏳ **installing** — see prerequisite below |
| C++/WinRT WinUI app shell | ⛔ next, after the toolset lands |

## Prerequisite — the one gate

Visual Studio 2026 Community is installed, **but only with the .NET/C# workload.**
Compiling C++ needs the **"Desktop development with C++"** workload + the
**Windows App SDK C++** components. An install was kicked off via the VS Installer
(`setup.exe modify --add Microsoft.VisualStudio.Workload.NativeDesktop --add
Microsoft.VisualStudio.ComponentGroup.WindowsAppSDK.Cpp`). It requires approving a
**UAC elevation prompt** and is a multi-GB download.

Once `...\VC\Tools\MSVC\` exists, run:

```
cd cpp\SentinelForge\core
build_core.bat
```

You should see the sample document loaded and printed — the reading core working
in C++ before any UI exists. That is the Phase 1 milestone, core-first.
