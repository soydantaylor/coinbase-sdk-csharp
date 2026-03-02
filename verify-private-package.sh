#!/bin/bash
# Script to verify your private GitHub package exists

GITHUB_USERNAME="soydantaylor"
PACKAGE_NAME="Coinbase.SDK"

echo "🔍 Verifying private GitHub package: $PACKAGE_NAME"
echo ""

if [ -z "$GITHUB_TOKEN" ]; then
    echo "❌ GITHUB_TOKEN environment variable not set"
    echo "💡 Set it with: export GITHUB_TOKEN=your_token_here"
    exit 1
fi

echo "📡 Checking if package exists in your private registry..."

# Check if package exists using GitHub API
RESPONSE=$(curl -s -H "Authorization: token $GITHUB_TOKEN" \
     -H "Accept: application/vnd.github.v3+json" \
     "https://api.github.com/users/$GITHUB_USERNAME/packages/nuget/$PACKAGE_NAME")

if echo "$RESPONSE" | grep -q '"name"'; then
    echo "✅ Package found in your private registry!"
    echo ""
    echo "📦 Package details:"
    echo "$RESPONSE" | jq -r '.name, .package_type, .visibility, .created_at' 2>/dev/null || echo "Install 'jq' for formatted output"
    echo ""
    echo "🔗 Package URL: https://github.com/$GITHUB_USERNAME?tab=packages"
    echo ""
    echo "📥 To install this private package:"
    echo "1. Ensure you have a NuGet.Config with GitHub source configured"
    echo "2. Run: dotnet add package $PACKAGE_NAME --source github"
else
    echo "❌ Package not found or not accessible"
    echo ""
    echo "🔍 Possible reasons:"
    echo "1. Package hasn't been published yet"
    echo "2. Token doesn't have 'read:packages' permission"
    echo "3. Package name is different"
    echo ""
    echo "🔗 Check manually: https://github.com/$GITHUB_USERNAME?tab=packages"
fi

echo ""
echo "🔒 Remember: This is a private package, only you and authorized users can access it."