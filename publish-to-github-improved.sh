#!/bin/bash
# Improved script to publish Coinbase SDK to GitHub Package Registry
# Usage: ./publish-to-github-improved.sh YOUR_GITHUB_USERNAME YOUR_GITHUB_TOKEN [REPOSITORY_NAME]

set -e  # Exit on any error

# Check arguments
if [ $# -lt 2 ]; then
    echo "Usage: $0 <github-username> <github-token> [repository-name]"
    echo "Example: $0 soydantaylor ghp_xxxxxxxxxxxx coinbase-sdk-csharp"
    exit 1
fi

GITHUB_USERNAME=$1
GITHUB_TOKEN=$2
REPOSITORY_NAME=${3:-"coinbase-sdk-csharp"}
PACKAGE_VERSION=${4:-"1.0.0"}

echo "🚀 Publishing Coinbase SDK to GitHub Package Registry..."
echo "📋 Configuration:"
echo "   Username: $GITHUB_USERNAME"
echo "   Repository: $REPOSITORY_NAME"
echo "   Package Version: $PACKAGE_VERSION"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean --configuration Release

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build the solution
echo "🔨 Building solution..."
dotnet build --configuration Release --no-restore

# Run tests
echo "🧪 Running tests..."
if ! dotnet test --configuration Release --no-build --verbosity normal; then
    echo "❌ Tests failed! Aborting publish."
    exit 1
fi

# Create output directory
OUTPUT_DIR="./artifacts"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Pack the NuGet package with proper repository information
echo "📦 Packing NuGet package with repository metadata..."
dotnet pack Coinbase.SDK/Coinbase.SDK.csproj \
    --configuration Release \
    --no-build \
    --output "$OUTPUT_DIR" \
    -p:RepositoryUrl="https://github.com/$GITHUB_USERNAME/$REPOSITORY_NAME" \
    -p:PackageProjectUrl="https://github.com/$GITHUB_USERNAME/$REPOSITORY_NAME" \
    -p:RepositoryType=git \
    -p:RepositoryBranch=main

# Configure NuGet source with proper authentication
GITHUB_SOURCE="https://nuget.pkg.github.com/$GITHUB_USERNAME/index.json"
echo "⚙️  Configuring GitHub NuGet source: $GITHUB_SOURCE"

# Remove existing source if it exists
dotnet nuget remove source github 2>/dev/null || true

# Add GitHub source with proper authentication
dotnet nuget add source "$GITHUB_SOURCE" \
    --name github \
    --username "$GITHUB_USERNAME" \
    --password "$GITHUB_TOKEN" \
    --store-password-in-clear-text

# Publish to GitHub Packages with explicit API key
echo "🚀 Publishing to GitHub Package Registry..."
for package in "$OUTPUT_DIR"/*.nupkg; do
    if [ -f "$package" ]; then
        echo "📤 Publishing $(basename "$package")..."
        if dotnet nuget push "$package" \
            --source github \
            --api-key "$GITHUB_TOKEN" \
            --skip-duplicate; then
            echo "✅ Successfully published $(basename "$package")"
        else
            echo "❌ Failed to publish $(basename "$package")"
            echo "🔍 Checking if package already exists..."
        fi
    fi
done

echo ""
echo "🎉 Publish process completed!"
echo ""
echo "🔍 Verification steps:"
echo "1. Check your GitHub profile: https://github.com/$GITHUB_USERNAME?tab=packages"
echo "2. Check repository packages: https://github.com/$GITHUB_USERNAME/$REPOSITORY_NAME/packages"
echo "3. Direct package link: https://github.com/$GITHUB_USERNAME/$REPOSITORY_NAME/pkgs/nuget/Coinbase.SDK"
echo ""
echo "💡 If you don't see the package:"
echo "1. Packages are private by default (which is what you want)"
echo "2. Check your private packages: https://github.com/$GITHUB_USERNAME?tab=packages"
echo "3. Make sure the repository exists: https://github.com/$GITHUB_USERNAME/$REPOSITORY_NAME"
echo "4. Verify you have write:packages permission on your token"
echo ""
echo "📦 To install the package:"
echo "   dotnet add package Coinbase.SDK --source github"