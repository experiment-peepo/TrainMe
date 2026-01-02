# GitHub Actions & App Configuration

This directory contains GitHub Actions workflows and GitHub App configuration files for TrainMeX.

## Files

### Workflows

- **`workflows/release.yml`** - Main release workflow that builds and publishes TrainMeX when a version tag (v*) is pushed
- **`workflows/test-release.yml`** - Test workflow to validate the build process without creating releases

### Configuration

- **`app.yml`** - GitHub App manifest for creating a GitHub App programmatically
- **`validate-workflow.ps1`** - PowerShell script to validate workflow configurations

## Testing the Workflow

### Option 1: Use the Test Workflow

The `test-release.yml` workflow can be triggered manually or runs automatically on pull requests:

```bash
# Trigger manually via GitHub CLI
gh workflow run "Test Release Workflow"
```

### Option 2: Validate Locally

Run the validation script to check your workflow configuration:

```powershell
.\github\validate-workflow.ps1
```

### Option 3: Test with a Draft Tag

Create a test tag to verify the workflow (remember to delete it after testing):

```bash
git tag v0.0.0-test
git push origin v0.0.0-test
# After verification, delete the tag:
git push origin --delete v0.0.0-test
git tag -d v0.0.0-test
```

## Creating a GitHub App

To create a GitHub App from the manifest:

1. Update `app.yml` with your repository URL and webhook URL
2. Use the GitHub API to create the app:

```bash
curl -X POST \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/app-manifests/YOUR_CODE/conversions
```

Or use the GitHub CLI:

```bash
gh api -X POST /app-manifests/YOUR_CODE/conversions
```

## Workflow Permissions

The release workflow requires `contents: write` permission to create releases. This is configured in the workflow file:

```yaml
permissions:
  contents: write
```

**Important:** If you modify the workflow, ensure this permission is always present to avoid 403 errors when creating releases.

## Troubleshooting

### 403 Forbidden Error

If you see a 403 error when creating releases:

1. Check that `permissions: contents: write` is set in the workflow
2. Verify repository settings allow workflow permissions
3. Ensure the workflow file is in the correct branch

### Workflow Not Triggering

- Verify the tag format matches the pattern: `v*` (e.g., `v1.0.0`, `v5`)
- Check that the workflow file is in the `.github/workflows/` directory
- Ensure the workflow file is committed to the repository

### Build Failures

- Run the test workflow first: `test-release.yml`
- Check that all dependencies are restored correctly
- Verify the publish path matches the release file path

## Best Practices

1. **Always test before releasing** - Use the test workflow or validation script
2. **Keep action versions updated** - We use `@v2` for `action-gh-release`
3. **Verify files exist** - The workflow includes a verification step
4. **Monitor workflow runs** - Check the Actions tab for any issues



