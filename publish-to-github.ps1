# PowerShell script to publish Coinbase SDK to GitHub Package Registry
# Usage: .\publish-to-github.ps1 -GitHubUsername "your-username" -GitHubToken "your-token"

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken,
    
    [Parameter(Mandatory=$false)]
    [string]$PackageVersion = "1.0.0"
)

Write-Host "Publishing Coinbase SDK to GitHub Package Registry..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean --configuration Release

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
$testResult = dotnet test --configuration Release --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed! Aborting publish." -ForegroundColor Red
    exit 1
}

# Create output directory
$outputDir = "./artifacts"
if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

# Pack the NuGet package
Write-Host "Packing NuGet package..." -ForegroundColor Yellow
dotnet pack Coinbase.SDK/Coinbase.SDK.csproj --configuration Release --no-build --output $outputDir

# Configure NuGet source
$githubSource = "https://nuget.pkg.github.com/$GitHubUsername/index.json"
Write-Host "Configuring GitHub NuGet source: $githubSource" -ForegroundColor Yellow

# Remove existing source if it exists
dotnet nuget remove source github 2>$null

# Add GitHub source
dotnet nuget add source $githubSource --name github --username $GitHubUsername --password $GitHubToken --store-password-in-clear-text

# Publish to GitHub Packages
Write-Host "Publishing to GitHub Package Registry..." -ForegroundColor Yellow
$packageFiles = Get-ChildItem -Path $outputDir -Filter "*.nupkg"

foreach ($package in $packageFiles) {
    Write-Host "Publishing $($package.Name)..." -ForegroundColor Cyan
    dotnet nuget push $package.FullName --source github --skip-duplicate
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully published $($package.Name)" -ForegroundColor Green
    } else {
        Write-Host "Failed to publish $($package.Name)" -ForegroundColor Red
    }
}

Write-Host "Publish process completed!" -ForegroundColor Green
Write-Host "You can now install the package using:" -ForegroundColor Cyan
Write-Host "dotnet add package Coinbase.SDK --source github" -ForegroundColor White