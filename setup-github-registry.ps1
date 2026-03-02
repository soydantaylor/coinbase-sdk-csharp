# PowerShell script to help configure GitHub Package Registry
# Usage: .\setup-github-registry.ps1

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken
)

Write-Host "Setting up GitHub Package Registry configuration..." -ForegroundColor Green

# Update NuGet.Config
$nugetConfigPath = "NuGet.Config"
if (Test-Path $nugetConfigPath) {
    Write-Host "Updating NuGet.Config with your credentials..." -ForegroundColor Yellow
    
    $content = Get-Content $nugetConfigPath -Raw
    $content = $content -replace "YOUR_GITHUB_USERNAME", $GitHubUsername
    $content = $content -replace "YOUR_GITHUB_TOKEN", $GitHubToken
    
    Set-Content $nugetConfigPath $content
    Write-Host "✅ NuGet.Config updated successfully" -ForegroundColor Green
} else {
    Write-Host "❌ NuGet.Config not found!" -ForegroundColor Red
}

# Update project file
$projectPath = "Coinbase.SDK/Coinbase.SDK.csproj"
if (Test-Path $projectPath) {
    Write-Host "Updating project file with your repository URL..." -ForegroundColor Yellow
    
    $content = Get-Content $projectPath -Raw
    $content = $content -replace "YOUR_GITHUB_USERNAME", $GitHubUsername
    
    Set-Content $projectPath $content
    Write-Host "✅ Project file updated successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Project file not found!" -ForegroundColor Red
}

# Update documentation
$docsPath = "GITHUB_PACKAGES.md"
if (Test-Path $docsPath) {
    Write-Host "Updating documentation..." -ForegroundColor Yellow
    
    $content = Get-Content $docsPath -Raw
    $content = $content -replace "YOUR_USERNAME", $GitHubUsername
    
    Set-Content $docsPath $content
    Write-Host "✅ Documentation updated successfully" -ForegroundColor Green
}

Write-Host "`n🎉 Setup completed!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the updated files" -ForegroundColor White
Write-Host "2. Run .\publish-to-github.ps1 -GitHubUsername '$GitHubUsername' -GitHubToken 'your-token' to publish" -ForegroundColor White
Write-Host "3. See GITHUB_PACKAGES.md for detailed instructions" -ForegroundColor White