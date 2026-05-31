@echo off
REM build_core.bat — compile and run the C++ reading-core test.
REM Requires the "Desktop development with C++" workload (installing now).
REM Run this from a normal terminal; it finds the MSVC compiler via vswhere.

setlocal
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set "VSPATH=%%i"

if "%VSPATH%"=="" (
  echo [!] C++ tools not found yet. Wait for the "Desktop development with C++"
  echo     workload to finish installing, then re-run this script.
  exit /b 1
)

call "%VSPATH%\VC\Auxiliary\Build\vcvars64.bat" >nul
echo Compiling reading core with MSVC...
cl /nologo /std:c++20 /EHsc /W4 /Fe:SentinelCoreTest.exe DocumentReader.cpp test_main.cpp
if errorlevel 1 (
  echo [!] Build failed.
  exit /b 1
)

echo.
echo === running ===
SentinelCoreTest.exe %*
endlocal
