@echo off
setlocal

REM Check for admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: This script must be run as Administrator.
    echo Right-click the file and select "Run as administrator".
    pause
    exit /b 1
)

set "SRC=%~dp0GroupClashesByDistance\bin\x64\Release"
set "DEST=C:\Program Files\Autodesk\Navisworks Manage 2027\Plugins\GroupClashesByDistance"
set "RES=%~dp0GroupClashesByDistance\Resources"

echo Deploying GroupClashesByDistance plugin to Navisworks 2027...
echo.

if not exist "%DEST%" (
    mkdir "%DEST%"
    if errorlevel 1 ( echo FAILED to create plugin folder. & pause & exit /b 1 )
)

if not exist "%DEST%\Resources" (
    mkdir "%DEST%\Resources"
    if errorlevel 1 ( echo FAILED to create Resources folder. & pause & exit /b 1 )
)

echo Copying DLL...
xcopy /Y /R /D "%SRC%\GroupClashesByDistance.dll" "C:\Program Files\Autodesk\Navisworks Manage 2027\Plugins\"
if errorlevel 1 ( echo FAILED to copy DLL. & pause & exit /b 1 )

echo Copying icons...
xcopy /Y /R /D /E /I "%RES%" "%DEST%\Resources"
if errorlevel 1 ( echo FAILED to copy resources. & pause & exit /b 1 )

echo.
echo Plugin deployed successfully to:
echo %DEST%
echo.
pause
endlocal
