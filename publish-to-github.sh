#!/bin/bash
# Bash script to publish Coinbase SDK to GitHub Package Registry
# Usage: ./publish-to-github.sh YOUR_GITHUB_USERNAME YOUR_GITHUB_TOKEN

set -e  # Exit on any error

# Check arguments
if [ $# -lt 2 ]; then
    echo "Usage: $0 <github-username> <github-token> [package-version]"
    echo "Example: $0 myusername ghp_xxxxxxxxxxxx 1.0.0"
    exit 1
fi

GITHUB_USERNAME=$1
GITHUB_TOKEN=$2
PACKAGE_VERSION=${3:-"1.0.0"}

echo "🚀 Publishing Coinbase SDK to GitHub Package Registry..."

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean --configuration Release

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore Coinbase.SDK.sln

# Build the solution
echo "🔨 Building solution..."
dotnet build Coinbase.SDK.sln --configuration Release --no-restore

# Run tests
echo "🧪 Running tests..."
if ! dotnet test Coinbase.SDK.sln --configuration Release --no-build --verbosity normal; then
    echo "❌ Tests failed! Aborting publish."
    exit 1
fi

# Create output directory
OUTPUT_DIR="./artifacts"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Pack the NuGet package
echo "📦 Packing NuGet package..."
dotnet pack Coinbase.SDK/Coinbase.SDK.csproj --configuration Release --no-build --output "$OUTPUT_DIR"

# Configure NuGet source
GITHUB_SOURCE="https://nuget.pkg.github.com/$GITHUB_USERNAME/index.json"
echo "⚙️  Configuring GitHub NuGet source: $GITHUB_SOURCE"

# Remove existing source if it exists
dotnet nuget remove source github 2>/dev/null || true

# Add GitHub source
dotnet nuget add source "$GITHUB_SOURCE" --name github --username "$GITHUB_USERNAME" --password "$GITHUB_TOKEN" --store-password-in-clear-text

# Publish to GitHub Packages
echo "🚀 Publishing to GitHub Package Registry..."
for package in "$OUTPUT_DIR"/*.nupkg; do
    if [ -f "$package" ]; then
        echo "📤 Publishing $(basename "$package")..."
        if dotnet nuget push "$package" --source github --skip-duplicate; then
            echo "✅ Successfully published $(basename "$package")"
        else
            echo "❌ Failed to publish $(basename "$package")"
        fi
    fi
done

echo "🎉 Publish process completed!"
echo "💡 You can now install the package using:"
echo "   dotnet add package Coinbase.SDK --source github"