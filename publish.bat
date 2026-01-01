@echo off
REM Publish script for TrainMe - Standalone Portable Executable
REM This script builds a self-contained, single-file executable

echo Building TrainMe as standalone portable executable...

set PROJECT_PATH=TrainMe\TrainMe.csproj
set OUTPUT_PATH=publish

REM Clean previous publish
if exist "%OUTPUT_PATH%" (
    echo Cleaning previous publish directory...
    rmdir /s /q "%OUTPUT_PATH%"
)

REM Publish the application
echo Publishing application...
dotnet publish %PROJECT_PATH% ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o %OUTPUT_PATH%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo Standalone executable location: %CD%\%OUTPUT_PATH%\TrainMe.exe
    echo.
    echo The executable is portable and can be run from any location.
) else (
    echo.
    echo Build failed with exit code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)


