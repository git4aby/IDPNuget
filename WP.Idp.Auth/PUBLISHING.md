# Publishing WP.Idp.Auth to NuGet

This guide explains how to build and publish the `WP.Idp.Auth` NuGet package.

## Prerequisites

1. .NET SDK 8.0 or later (for building .NET 10.0 target)
2. NuGet CLI tool (optional, for manual publishing)
3. NuGet API key from [nuget.org](https://www.nuget.org/) (if publishing to NuGet.org)

## Building the Package

### 1. Restore Dependencies

```bash
dotnet restore WP.Idp.Auth/WP.Idp.Auth.csproj
```

### 2. Build the Project

```bash
dotnet build WP.Idp.Auth/WP.Idp.Auth.csproj -c Release
```

This will build the project for all target frameworks:
- net40
- net45
- net46
- net47
- net6.0
- net8.0
- net10.0

### 3. Run Tests (if applicable)

```bash
dotnet test
```

### 4. Create the NuGet Package

```bash
dotnet pack WP.Idp.Auth/WP.Idp.Auth.csproj -c Release
```

This creates a `.nupkg` file in `WP.Idp.Auth/bin/Release/` directory.

The package will be named: `WP.Idp.Auth.1.0.0.nupkg` (version from csproj)

## Updating the Version

To update the package version, edit `WP.Idp.Auth.csproj`:

```xml
<PropertyGroup>
  <Version>1.0.1</Version>  <!-- Update this -->
</PropertyGroup>
```

Or use the `-p:Version` parameter:

```bash
dotnet pack WP.Idp.Auth/WP.Idp.Auth.csproj -c Release -p:Version=1.0.1
```

## Publishing to NuGet.org

### Option 1: Using dotnet CLI

```bash
dotnet nuget push WP.Idp.Auth/bin/Release/WP.Idp.Auth.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Option 2: Using NuGet CLI

```bash
nuget push WP.Idp.Auth/bin/Release/WP.Idp.Auth.1.0.0.nupkg \
  YOUR_API_KEY \
  -Source https://api.nuget.org/v3/index.json
```

### Getting Your API Key

1. Sign in to [nuget.org](https://www.nuget.org/)
2. Go to your account settings
3. Navigate to "API Keys"
4. Create a new API key
5. Copy the key (you won't be able to see it again)

## Publishing to a Private Feed

### Azure Artifacts

```bash
dotnet nuget push WP.Idp.Auth/bin/Release/WP.Idp.Auth.1.0.0.nupkg \
  --api-key YOUR_AZURE_DEVOPS_PAT \
  --source https://pkgs.dev.azure.com/YOUR_ORG/_packaging/YOUR_FEED/nuget/v3/index.json
```

### GitHub Packages

```bash
dotnet nuget push WP.Idp.Auth/bin/Release/WP.Idp.Auth.1.0.0.nupkg \
  --api-key YOUR_GITHUB_TOKEN \
  --source https://nuget.pkg.github.com/YOUR_USERNAME/index.json
```

### Local/Network Feed

```bash
dotnet nuget push WP.Idp.Auth/bin/Release/WP.Idp.Auth.1.0.0.nupkg \
  --source C:\NuGetPackages
```

## Verifying the Package

Before publishing, you can verify the package contents:

```bash
# Extract and inspect package contents
dotnet nuget locals all --list
```

Or use a tool like [NuGet Package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer) to inspect the `.nupkg` file.

## Package Contents Verification

The package should contain:

- ✅ Abstractions (IIdpAuthenticator)
- ✅ SharedModels (TokenValidationResult, WpIdpOptions)
- ✅ CoreAdapter (for .NET 6/8/10)
- ✅ FrameworkAdapter (for .NET Framework 4.x)
- ✅ All required dependencies

## Troubleshooting

### Build Errors

If you encounter build errors:

1. Ensure all target frameworks are supported by your SDK
2. Check that all package references are compatible
3. Verify conditional compilation directives (`#if NET6_0_OR_GREATER`)

### Publishing Errors

Common issues:

- **403 Forbidden**: Invalid or expired API key
- **409 Conflict**: Package version already exists
- **400 Bad Request**: Package metadata is invalid

### Version Conflicts

If a version already exists on NuGet.org, you must increment the version number.

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build and Publish

on:
  release:
    types: [created]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build -c Release
      
      - name: Pack
        run: dotnet pack -c Release
      
      - name: Publish to NuGet
        run: dotnet nuget push **/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

## Best Practices

1. **Versioning**: Follow [Semantic Versioning](https://semver.org/)
2. **Release Notes**: Include release notes in package description
3. **Testing**: Test the package in both Framework and Core projects before publishing
4. **Documentation**: Keep README.md updated with usage examples
5. **Dependencies**: Minimize dependencies to reduce package size

## Support

For issues or questions about publishing, please open an issue on the repository.

