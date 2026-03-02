#!/bin/bash
# Script to check GitHub Package Registry status and visibility

GITHUB_USERNAME="soydantaylor"
PACKAGE_NAME="Coinbase.SDK"

echo "🔍 Checking GitHub Package Registry for $PACKAGE_NAME..."

# Check if package exists using GitHub API
echo "📡 Checking package existence..."
curl -H "Authorization: token $GITHUB_TOKEN" \
     -H "Accept: application/vnd.github.v3+json" \
     "https://api.github.com/users/$GITHUB_USERNAME/packages/nuget/$PACKAGE_NAME" \
     2>/dev/null | jq '.' || echo "Package not found or API error"

echo ""
echo "🔗 Direct links to check:"
echo "1. GitHub Packages page: https://github.com/$GITHUB_USERNAME?tab=packages"
echo "2. Repository packages: https://github.com/$GITHUB_USERNAME/coinbase-sdk-csharp/packages"
echo "3. Package URL: https://github.com/$GITHUB_USERNAME/coinbase-sdk-csharp/pkgs/nuget/$PACKAGE_NAME"

echo ""
echo "💡 Troubleshooting steps:"
echo "1. Check if the repository exists: https://github.com/$GITHUB_USERNAME/coinbase-sdk-csharp"
echo "2. Verify package visibility (packages are private by default)"
echo "3. Check if you have the correct permissions"
echo "4. Look for the package in your GitHub profile under 'Packages' tab"

echo ""
echo "🔒 Package is private by default (as intended)"
echo "📋 To verify your private package:"
echo "1. Go to: https://github.com/$GITHUB_USERNAME?tab=packages"
echo "2. Look for 'Coinbase.SDK' in your private packages list"
echo "3. Only you (and collaborators) can see and install this package"