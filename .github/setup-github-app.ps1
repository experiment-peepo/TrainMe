# PowerShell script to help create a GitHub App from the manifest
# This script guides you through the GitHub App creation process

param(
    [switch]$Interactive = $true
)

Write-Host "GitHub App Setup for TrainMeX" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

# Check if GitHub CLI is installed
$ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghInstalled) {
    Write-Host "GitHub CLI (gh) is not installed." -ForegroundColor Yellow
    Write-Host "Install it from: https://cli.github.com/" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternatively, you can create the app manually:" -ForegroundColor Yellow
    Write-Host "1. Go to: https://github.com/settings/apps/new" -ForegroundColor Cyan
    Write-Host "2. Fill in the app details from .github/app.yml" -ForegroundColor Cyan
    Write-Host "3. Set permissions: Contents (Write), Metadata (Read)" -ForegroundColor Cyan
    Write-Host "4. Subscribe to events: Release, Push" -ForegroundColor Cyan
    Write-Host ""
    exit 0
}

# Check if user is authenticated
Write-Host "Checking GitHub authentication..." -ForegroundColor Cyan
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "You are not authenticated with GitHub CLI." -ForegroundColor Yellow
    Write-Host "Run: gh auth login" -ForegroundColor Cyan
    exit 1
}

Write-Host "[OK] Authenticated with GitHub" -ForegroundColor Green
Write-Host ""

# Display manifest information
Write-Host "App Manifest Configuration:" -ForegroundColor Cyan
Write-Host "---------------------------" -ForegroundColor Cyan
$manifest = Get-Content ".github/app.yml" -Raw
Write-Host $manifest
Write-Host ""

# Option 1: Create app via manifest (requires web interface)
Write-Host "To create a GitHub App from this manifest:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Option 1: Using GitHub Web Interface (Recommended)" -ForegroundColor Yellow
Write-Host "1. Go to: https://github.com/settings/apps/new?manifest=1" -ForegroundColor Cyan
Write-Host "2. Copy the contents of .github/app.yml" -ForegroundColor Cyan
Write-Host "3. Paste into the manifest field" -ForegroundColor Cyan
Write-Host "4. Click 'Create GitHub App'" -ForegroundColor Cyan
Write-Host ""

# Option 2: Manual creation
Write-Host "Option 2: Manual Creation" -ForegroundColor Yellow
Write-Host "1. Go to: https://github.com/settings/apps/new" -ForegroundColor Cyan
Write-Host "2. App name: TrainMeX" -ForegroundColor Cyan
Write-Host "3. Homepage URL: https://github.com/experiment-peepo/TrainMe" -ForegroundColor Cyan
Write-Host "4. Webhook: Leave empty or set to your webhook URL" -ForegroundColor Cyan
Write-Host "5. Permissions:" -ForegroundColor Cyan
Write-Host "   - Contents: Write" -ForegroundColor Cyan
Write-Host "   - Metadata: Read" -ForegroundColor Cyan
Write-Host "6. Subscribe to events: Release, Push" -ForegroundColor Cyan
Write-Host "7. Where can this GitHub App be installed: Any account" -ForegroundColor Cyan
Write-Host ""

# Option 3: API method (if they have a code)
if ($Interactive) {
    Write-Host "Option 3: Using GitHub API (if you have a manifest code)" -ForegroundColor Yellow
    $manifestCode = Read-Host "Enter manifest code (or press Enter to skip)"
    
    if ($manifestCode) {
        Write-Host ""
        Write-Host "Creating GitHub App..." -ForegroundColor Cyan
        $result = gh api -X POST "/app-manifests/$manifestCode/conversions" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] GitHub App created successfully!" -ForegroundColor Green
            Write-Host $result
        } else {
            Write-Host "[ERROR] Failed to create app: $result" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "After creating the app:" -ForegroundColor Cyan
Write-Host "1. Install the app on your repository" -ForegroundColor Yellow
Write-Host "2. Generate a private key if needed" -ForegroundColor Yellow
Write-Host "3. The workflow will use GITHUB_TOKEN automatically" -ForegroundColor Yellow
Write-Host ""



