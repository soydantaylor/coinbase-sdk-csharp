# Troubleshooting GitHub Package Registry

## Common Issues and Solutions

### Issue 1: Package Published but Not Visible

**Symptoms:**
- `dotnet nuget push` reports success
- Package doesn't appear in GitHub Package Registry
- No errors during publishing

**Possible Causes & Solutions:**

#### 1. **Repository Association Issue**
The package might not be properly associated with your repository.

**Solution:**
```bash
# Re-publish with explicit repository metadata
./publish-to-github-improved.sh soydantaylor YOUR_TOKEN coinbase-sdk-csharp
```

#### 2. **Package is Private by Default (Intended Behavior)**
GitHub Packages are private by default, which is perfect for your use case.

**Check Your Private Packages:**
1. Go to: https://github.com/soydantaylor?tab=packages
2. Look for "Coinbase.SDK" in your private packages list
3. Only you and authorized users can see and install this package

**This is the desired behavior for a private package.**

#### 3. **Repository Doesn't Exist**
The package needs to be associated with an existing repository.

**Solution:**
1. Create repository: https://github.com/new
2. Name it: `coinbase-sdk-csharp`
3. Push your code to the repository
4. Re-publish the package

#### 4. **Token Permissions**
Your GitHub token might not have the correct permissions.

**Required Permissions:**
- `write:packages`
- `read:packages`
- `repo` (if repository is private)

**Fix:**
1. Go to: https://github.com/settings/tokens
2. Edit your token
3. Ensure the above permissions are checked
4. Regenerate if needed

### Issue 2: Authentication Problems

**Symptoms:**
- "No API Key was provided" warnings
- Authentication failures

**Solution:**
Use the improved script that sets the API key properly:
```bash
./publish-to-github-improved.sh soydantaylor YOUR_TOKEN
```

### Issue 3: Package Already Exists Error

**Symptoms:**
- Error about package version already existing
- Cannot overwrite existing package

**Solution:**
GitHub Packages doesn't allow overwriting. You need to:
1. Increment version in `Coinbase.SDK.csproj`
2. Re-publish with new version

### Verification Commands

#### Check if Package Exists (requires `jq`):
```bash
curl -H "Authorization: token YOUR_TOKEN" \
     -H "Accept: application/vnd.github.v3+json" \
     "https://api.github.com/users/soydantaylor/packages/nuget/Coinbase.SDK" | jq '.'
```

#### List All Your Packages:
```bash
curl -H "Authorization: token YOUR_TOKEN" \
     -H "Accept: application/vnd.github.v3+json" \
     "https://api.github.com/users/soydantaylor/packages" | jq '.[].name'
```

### Manual Verification Steps

1. **Check GitHub Profile:**
   - Go to: https://github.com/soydantaylor
   - Click "Packages" tab
   - Look for "Coinbase.SDK"

2. **Check Repository Packages:**
   - Go to: https://github.com/soydantaylor/coinbase-sdk-csharp
   - Look for "Packages" section in right sidebar

3. **Direct Package URL:**
   - Try: https://github.com/soydantaylor/coinbase-sdk-csharp/pkgs/nuget/Coinbase.SDK

### Next Steps if Still Not Working

1. **Create the Repository First:**
   ```bash
   # Create a new repository on GitHub named 'coinbase-sdk-csharp'
   # Then push your code:
   git remote add origin https://github.com/soydantaylor/coinbase-sdk-csharp.git
   git branch -M main
   git push -u origin main
   ```

2. **Use GitHub CLI (if available):**
   ```bash
   gh repo create coinbase-sdk-csharp --public
   git remote add origin https://github.com/soydantaylor/coinbase-sdk-csharp.git
   git push -u origin main
   ```

3. **Try Publishing Again:**
   ```bash
   ./publish-to-github-improved.sh soydantaylor YOUR_TOKEN coinbase-sdk-csharp
   ```

### Alternative: Publish to Specific Repository

If you want to associate the package with a different repository:

```bash
./publish-to-github-improved.sh soydantaylor YOUR_TOKEN CoinbaseTest
```

This will associate the package with your existing `CoinbaseTest` repository.

### Debug Information

If you're still having issues, run this to get debug info:
```bash
./check-github-package.sh
```

This will show you:
- Direct links to check
- API responses
- Troubleshooting steps
- Visibility settings