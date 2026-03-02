# Publishing to GitHub Package Registry

This document explains how to publish the Coinbase SDK to GitHub Package Registry instead of NuGet.org.

## Prerequisites

1. **GitHub Personal Access Token**: Create a token with `write:packages` and `read:packages` permissions
   - Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate new token with `write:packages`, `read:packages`, and `repo` scopes

2. **Update Configuration**: Replace `YOUR_GITHUB_USERNAME` in the following files with your actual GitHub username:
   - `NuGet.Config`
   - `Coinbase.SDK/Coinbase.SDK.csproj`

## Publishing Methods

### Method 1: Automated Publishing with GitHub Actions

The repository includes a GitHub Actions workflow (`.github/workflows/publish-package.yml`) that automatically publishes packages when you create a version tag.

**Steps:**
1. Update your code and commit changes
2. Create and push a version tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. The GitHub Action will automatically build, test, and publish the package

### Method 2: Manual Publishing with PowerShell (Windows)

```powershell
.\publish-to-github.ps1 -GitHubUsername "your-username" -GitHubToken "your-token"
```

### Method 3: Manual Publishing with Bash (Linux/macOS)

```bash
./publish-to-github.sh your-username your-token
```

### Method 4: Manual Publishing with .NET CLI

```bash
# Configure the GitHub source
dotnet nuget add source https://nuget.pkg.github.com/YOUR_USERNAME/index.json \
  --name github \
  --username YOUR_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text

# Build and pack
dotnet build --configuration Release
dotnet pack Coinbase.SDK/Coinbase.SDK.csproj --configuration Release --output ./artifacts

# Push to GitHub Packages
dotnet nuget push ./artifacts/*.nupkg --source github --skip-duplicate
```

## Installing from GitHub Package Registry

### For End Users

1. **Create or update NuGet.Config** in your project root:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="github" value="https://nuget.pkg.github.com/YOUR_USERNAME/index.json" />
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
     </packageSources>
     <packageSourceCredentials>
       <github>
         <add key="Username" value="YOUR_USERNAME" />
         <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
       </github>
     </packageSourceCredentials>
   </configuration>
   ```

2. **Install the package**:
   ```bash
   dotnet add package Coinbase.SDK --source github
   ```

### Alternative: Using Package Manager Console

```powershell
Install-Package Coinbase.SDK -Source github
```

## Configuration Files Explained

### NuGet.Config
- Configures package sources and authentication
- Points to your GitHub Package Registry
- Includes credentials for accessing private packages

### GitHub Actions Workflow
- Automatically triggers on version tags (v*.*.*)
- Builds, tests, and publishes the package
- Uses `GITHUB_TOKEN` for authentication (automatically provided)

### Project File Updates
- Updated repository URLs to point to your GitHub repository
- Added GitHub-specific metadata for better package discovery

## Security Best Practices

1. **Never commit tokens to source control**
2. **Use environment variables or secure storage for tokens**
3. **Regularly rotate your GitHub tokens**
4. **Use the minimum required token permissions**

## Troubleshooting

### Common Issues

1. **Authentication Failed**
   - Verify your GitHub token has `write:packages` permission
   - Check that the username in NuGet.Config matches your GitHub username

2. **Package Already Exists**
   - GitHub Packages doesn't allow overwriting existing versions
   - Increment the version number in the project file

3. **Source Not Found**
   - Ensure the GitHub source is properly configured
   - Verify the repository URL format

### Verification

After publishing, you can verify the package was uploaded by:
1. Going to your GitHub repository
2. Clicking on "Packages" in the right sidebar
3. Finding your published package

## Version Management

The package version is controlled in `Coinbase.SDK.csproj`:
```xml
<PackageVersion>1.0.0</PackageVersion>
```

Update this version before publishing new releases.