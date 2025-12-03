# WP.Idp.Auth Project Structure

This document describes the structure and organization of the WP.Idp.Auth NuGet package.

## Directory Structure

```
WP.Idp.Auth/
├── Abstractions/              # Common interfaces
│   └── IIdpAuthenticator.cs   # Main authentication interface
│
├── CoreAdapter/               # .NET Core/6/8/10 implementation
│   ├── CoreIdpAuthenticator.cs      # Core implementation of IIdpAuthenticator
│   └── CoreAuthExtensions.cs        # Extension methods for IServiceCollection
│
├── FrameworkAdapter/          # .NET Framework 4.x implementation
│   ├── FrameworkIdpAuthenticator.cs      # Framework implementation of IIdpAuthenticator
│   └── FrameworkAuthExtensions.cs        # Extension methods for IAppBuilder (OWIN)
│
├── SharedModels/              # Shared DTOs and configuration
│   ├── TokenValidationResult.cs      # Token validation result model
│   └── WpIdpOptions.cs              # Configuration options
│
├── WP.Idp.Auth.csproj         # Multi-targeted project file
├── README.md                   # User documentation
├── PUBLISHING.md              # Publishing instructions
└── PROJECT_STRUCTURE.md       # This file
```

## Component Overview

### Abstractions

Contains the common interface that both Core and Framework adapters must implement:

- **IIdpAuthenticator**: Defines the contract for IDP authentication operations
  - `ValidateTokenAsync(string token)`: Validates a JWT token
  - `GetAuthorizeUrl()`: Gets the OAuth/OpenID Connect authorization URL

### CoreAdapter

Implementation for .NET Core 6, 8, and 10:

- Uses `Microsoft.Identity.Web` for authentication
- Integrates with ASP.NET Core dependency injection
- Provides `AddWpIdpAuth()` extension method for `IServiceCollection`
- Uses `JwtSecurityTokenHandler` for token validation

### FrameworkAdapter

Implementation for .NET Framework 4.0, 4.5, 4.6, and 4.7:

- Uses OWIN/Katana middleware
- Uses `Microsoft.Owin.Security.OpenIdConnect`
- Provides `UseWpIdpAuth()` extension method for `IAppBuilder`
- Uses `JwtSecurityTokenHandler` for token validation

### SharedModels

Common data transfer objects and configuration classes:

- **TokenValidationResult**: Contains token validation results and extracted claims
- **WpIdpOptions**: Configuration options for IDP authentication (Authority, ClientId, etc.)

## Conditional Compilation

The project uses conditional compilation directives to ensure only the appropriate code is compiled for each target framework:

- `#if NET6_0_OR_GREATER`: Code for .NET Core 6/8/10
- `#if !NET6_0_OR_GREATER`: Code for .NET Framework 4.x

This allows a single codebase to support multiple frameworks while using framework-specific APIs.

## Package Dependencies

### Common (All Frameworks)
- `System.IdentityModel.Tokens.Jwt` (version varies by framework)

### .NET Framework 4.x Only
- `Microsoft.Owin.Security.OpenIdConnect` 4.2.0
- `Microsoft.Owin.Security.Jwt` 4.2.0
- `Microsoft.Owin.Host.SystemWeb` 4.2.0
- `Owin` 1.0.0

### .NET Core/6/8/10 Only
- `Microsoft.Identity.Web` 3.0.0
- `Microsoft.Extensions.Configuration.Abstractions` 8.0.0
- `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.0
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0

## Target Frameworks

The package targets:
- **.NET Framework**: 4.0, 4.5, 4.6, 4.7
- **.NET Core/.NET**: 6.0, 8.0, 10.0

## Build Process

1. All source files are included in the project
2. Conditional compilation (`#if` directives) ensures only relevant code compiles for each target
3. Framework-specific package references are conditionally included
4. The resulting NuGet package contains assemblies for all target frameworks

## Extension Points

### For .NET Core Applications

```csharp
// In Program.cs or Startup.cs
builder.Services.AddWpIdpAuth(builder.Configuration);
// or
builder.Services.AddWpIdpAuth(options);
```

### For .NET Framework Applications

```csharp
// In Startup.cs
app.UseWpIdpAuth(options);
```

## Testing Considerations

When testing the package:

1. Test with a .NET Framework 4.x application
2. Test with a .NET Core 6 application
3. Test with a .NET 8 application
4. Test with a .NET 10 application (when available)
5. Verify token validation works correctly
6. Verify authorization URL generation
7. Test with both Azure AD and Auth0 (if applicable)

## Future Enhancements

Potential areas for expansion:

- Support for additional IDP providers
- Custom claim mapping
- Token caching strategies
- Refresh token handling
- Multi-tenant support improvements

