# Publish script for TrainMe - Standalone Portable Executable
# This script builds a self-contained, single-file executable

Write-Host "Building TrainMe as standalone portable executable..." -ForegroundColor Green

$projectPath = "TrainMe\TrainMe.csproj"
$outputPath = "publish"

# Clean previous publish
if (Test-Path $outputPath) {
    Write-Host "Cleaning previous publish directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $outputPath
}

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Green
dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $outputPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Standalone executable location: $PWD\$outputPath\TrainMe.exe" -ForegroundColor Cyan
    Write-Host "`nThe executable is portable and can be run from any location." -ForegroundColor Yellow
} else {
    Write-Host "`nBuild failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}


