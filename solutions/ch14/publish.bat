@echo off
REM Publish Spirefall for Windows (x64) and stage the FNA native libs.
REM [unverified] — paths assume the ch01 sibling layout; adjust to yours.
REM Run from the Spirefall\ directory.

set CONFIG=Release
set RUNTIME=win-x64
set OUT=publish\win-x64

dotnet publish -c %CONFIG% -r %RUNTIME% --self-contained false -o %OUT%
if errorlevel 1 exit /b 1

REM FNA native libraries (SDL2, etc.) must sit next to the exe.
REM Get fnalibs from https://github.com/FNA-XNA/fnalibs (check the current
REM inventory against FNA's docs — the set changes over time).
if exist ..\fnalibs\x64\*.dll copy /y ..\fnalibs\x64\*.dll %OUT%\

echo.
echo Published to %OUT%. Zip that folder to distribute.
