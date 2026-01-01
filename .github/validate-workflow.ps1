# PowerShell script to validate GitHub Actions workflow files
# This script checks for common issues in workflow configurations

param(
    [string]$WorkflowPath = ".github/workflows/release.yml"
)

Write-Host "Validating GitHub Actions Workflow: $WorkflowPath" -ForegroundColor Cyan
Write-Host ""

$errors = @()
$warnings = @()

if (-not (Test-Path $WorkflowPath)) {
    Write-Host "ERROR: Workflow file not found: $WorkflowPath" -ForegroundColor Red
    exit 1
}

$content = Get-Content $WorkflowPath -Raw

# Check for required permissions
if ($content -notmatch "permissions:") {
    $errors += "Missing 'permissions' section - required for creating releases"
} elseif ($content -notmatch "contents:\s*write") {
    $errors += "Missing 'contents: write' permission - required for creating releases"
} else {
    Write-Host "[OK] Permissions configured correctly" -ForegroundColor Green
}

# Check for GITHUB_TOKEN
if ($content -notmatch "GITHUB_TOKEN") {
    $warnings += "GITHUB_TOKEN not explicitly set (may use default)"
} else {
    Write-Host "[OK] GITHUB_TOKEN configured" -ForegroundColor Green
}

# Check for action-gh-release
if ($content -notmatch "action-gh-release") {
    $errors += "Release action not found"
} else {
    Write-Host "[OK] Release action found" -ForegroundColor Green
    
    # Check version
    if ($content -match "action-gh-release@v1") {
        $warnings += "Using action-gh-release@v1 - consider upgrading to @v2"
    } elseif ($content -match "action-gh-release@v2") {
        Write-Host "[OK] Using latest action version (v2)" -ForegroundColor Green
    }
}

# Check for file verification step
if ($content -notmatch "Verify.*executable|Test-Path|executable exists") {
    $warnings += "No file verification step found - recommended to verify files before release"
} else {
    Write-Host "[OK] File verification step found" -ForegroundColor Green
}

# Check for tag trigger
if ($content -notmatch "tags:\s*-") {
    $warnings += "No tag trigger found - workflow may not trigger on tag pushes"
} else {
    Write-Host "[OK] Tag trigger configured" -ForegroundColor Green
}

# Summary
Write-Host ""
Write-Host "Validation Summary:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

if ($errors.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "[OK] Workflow validation passed!" -ForegroundColor Green
    exit 0
}

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "ERRORS:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  [X] $error" -ForegroundColor Red
    }
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "WARNINGS:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "  [!] $warning" -ForegroundColor Yellow
    }
}

Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "Workflow validation FAILED. Please fix the errors above." -ForegroundColor Red
    exit 1
} else {
    Write-Host "Workflow validation passed with warnings." -ForegroundColor Yellow
    exit 0
}

