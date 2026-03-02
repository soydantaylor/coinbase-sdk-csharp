#!/bin/bash
# Simple script to publish Coinbase SDK to GitHub Package Registry
# Usage: ./publish-simple.sh YOUR_GITHUB_USERNAME YOUR_GITHUB_TOKEN

set -e  # Exit on any error

# Check arguments
if [ $# -lt 2 ]; then
    echo "Usage: $0 <github-username> <github-token>"
    echo "Example: $0 soydantaylor ghp_xxxxxxxxxxxx"
    exit 1
fi

GITHUB_USERNAME=$1
GITHUB_TOKEN=$2

echo "🚀 Publishing Coinbase SDK to GitHub Package Registry..."

# Clean and build
echo "🔨 Building solution..."
dotnet clean --configuration Release
dotnet restore
dotnet build --configuration Release --no-restore

# Test
echo "🧪 Running tests..."
dotnet test --configuration Release --no-build --verbosity minimal

# Pack
echo "📦 Creating package..."
rm -rf ./artifacts
mkdir -p ./artifacts
dotnet pack Coinbase.SDK/Coinbase.SDK.csproj --configuration Release --no-build --output ./artifacts

# Configure source
GITHUB_SOURCE="https://nuget.pkg.github.com/$GITHUB_USERNAME/index.json"
echo "⚙️  Configuring source: $GITHUB_SOURCE"

dotnet nuget remove source github 2>/dev/null || true
dotnet nuget add source "$GITHUB_SOURCE" --name github --username "$GITHUB_USERNAME" --password "$GITHUB_TOKEN" --store-password-in-clear-text

# Publish
echo "📤 Publishing package..."
for package in ./artifacts/*.nupkg; do
    if [ -f "$package" ]; then
        echo "Publishing $(basename "$package")..."
        dotnet nuget push "$package" --source github --api-key "$GITHUB_TOKEN" --skip-duplicate
        if [ $? -eq 0 ]; then
            echo "✅ Successfully published $(basename "$package")"
        else
            echo "⚠️  Push completed (may already exist)"
        fi
    fi
done

echo ""
echo "🎉 Done! Check your private packages at:"
echo "   https://github.com/$GITHUB_USERNAME?tab=packages"