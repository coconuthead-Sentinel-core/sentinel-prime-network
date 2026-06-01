# Book Reader → C# / WinUI 3 Migration Plan

**Goal:** Rewrite the Sentinel Forge Book Reader from Python/Tkinter to a modern,
native Windows 11 app using **C# + .NET 10 + WinUI 3 (Windows App SDK)**, packaged
as a signed **MSIX** installer.

**Author:** Shannon Brian Kelly (Coconut Head / The Architect)
**Started:** 2026-05-30

---

## Reality check (read this first)

- The current app already runs natively on Windows 11 — it's a PyInstaller `.exe`.
  The rewrite is about a **modern native stack and long-term maintainability**, not
  about "making it run on Windows." Language ≠ compliance.
- The Python app is **8,628 lines in a single `BookReader` class with 246 methods.**
  This is not a "book reader" — it's a **reading + study + productivity workstation.**
- This is therefore a **multi-session, phased project.** The Python app stays the
  working app until the C# version reaches feature parity, phase by phase.

---

## What the app actually does (feature inventory)

Extracted from the 246 methods in `book_reader/book_reader.py`:

| Area | Features |
|---|---|
| **Reading** | Open PDF / DOCX / HTML / TXT, chapter detection + navigation, font sizing, persisted highlights |
| **Text-to-speech** | 3 backends: Windows SAPI (pyttsx3), Piper neural voices (`tts/`), PowerShell; word-synced highlighting while reading |
| **Dictation** | Microphone speech-to-text into notes |
| **Study tools** | Highlights, bookmarks, notes, study notes, topics, glossary, journal, audit findings, prompts (+CSV export) |
| **Productivity** | Pomodoro timer, Eisenhower matrix (day picker, time blocks, "do now" panel) |
| **Workflow** | Workflow folders / file management, library manager (scan, import, zone filter/migrate) |
| **Sentinel system** | GREEN/YELLOW/RED cognitive-load zones, session start/end wizards, handoff state |
| **Persistence** | SQLite study DB + JSON session/handoff/meta files |

---

## Technology mapping (Python → C#)

| Python (now) | C# / WinUI 3 (target) |
|---|---|
| Tkinter / ttk | **WinUI 3 XAML** |
| One `BookReader` god class | **MVVM**: Models + ViewModels + Views + Services |
| `sqlite3` | **Microsoft.Data.Sqlite** |
| `pypdf` | **PdfPig** (UglyToad.PdfPig) |
| `python-docx` | **DocumentFormat.OpenXml** (Open XML SDK) |
| `BeautifulSoup` | **HtmlAgilityPack** |
| `pyttsx3` (SAPI) | **System.Speech** / Windows.Media.SpeechSynthesis |
| Piper (`piper.exe`) | Keep `piper.exe`, call via `System.Diagnostics.Process` (same as today) |
| mic dictation | **Windows.Media.SpeechRecognition** |
| `winsound` | `System.Media.SystemSounds` / `MediaPlayer` |
| `send2trash` | Shell recycle (Microsoft.VisualBasic `FileSystem.DeleteFile`) |
| PyInstaller `.spec` | **MSIX package** + code signing |

---

## Toolchain status (checked 2026-05-30)

- ✅ .NET SDK **10.0.201** installed
- ✅ Windows 11 Home build **26200**
- ✅ winget + VS Code present
- ⚠️ No full Visual Studio (optional — VS Code + CLI is enough)
- ⏳ WinUI 3 templates: install `Microsoft.WindowsAppSDK.WinUI.CSharp.Templates`

---

## Phased roadmap

Each phase ends with **something that runs**. We never spend long with a broken app.

- **Phase 0 — Setup & skeleton** *(start here)*
  Install WinUI templates, scaffold an MVVM app, get a blank window launching.
  *Milestone: the new app opens an empty window on Windows 11.*

- **Phase 1 — Open & display a document**
  File picker, load TXT/PDF/DOCX/HTML into the reading pane, font sizing.
  *Milestone: you can open a book and read it.*

- **Phase 2 — Read aloud (TTS)**
  Start with Windows SAPI (easiest), then wire the Piper subprocess, then add
  word-synced highlighting.
  *Milestone: the app reads a page aloud and highlights along.*

- **Phase 3 — Persistence layer**
  Port the SQLite study-DB schema via Microsoft.Data.Sqlite. Foundation for all
  study tools.
  *Milestone: data saves and reloads between launches.*

- **Phase 4 — Study tools**
  Highlights → bookmarks → notes → glossary → topics → journal, one at a time.

- **Phase 5 — Productivity**
  Pomodoro timer, Eisenhower matrix.

- **Phase 6 — Library, zones & wizards**
  Library manager, GREEN/YELLOW/RED zones, session start/end wizards, handoff state.

- **Phase 7 — Dictation**
  Microphone speech-to-text.

- **Phase 8 — Package & ship**
  MSIX packaging, code signing, Windows 11 visual polish, installer.
  *Milestone: an installable, signed Windows 11 app.*

---

## Project layout (target)

```
csharp/
  SentinelForge.sln
  SentinelForge/                 # WinUI 3 app (MVVM)
    Models/                      # plain data (Book, Highlight, Bookmark, ...)
    ViewModels/                  # UI logic, no XAML knowledge
    Views/                       # XAML pages/windows
    Services/                    # DocumentLoader, TtsService, StudyDb, ...
    Assets/
```
