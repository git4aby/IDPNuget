# WP.Idp.Auth - Complete Technical Documentation

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture & Design](#architecture--design)
3. [Multi-Targeting Strategy](#multi-targeting-strategy)
4. [Component Breakdown](#component-breakdown)
5. [Implementation Details](#implementation-details)
6. [How It Works](#how-it-works)
7. [Usage Patterns](#usage-patterns)
8. [Dependencies & Package Management](#dependencies--package-management)
9. [Code Flow & Execution](#code-flow--execution)
10. [Extension Points & Customization](#extension-points--customization)
11. [Security Considerations](#security-considerations)
12. [Troubleshooting](#troubleshooting)

---

## Project Overview

### Purpose

**WP.Idp.Auth** is a cross-version NuGet package that provides a **unified authentication API** for Identity Provider (IDP) authentication across multiple .NET framework versions. It solves the problem of maintaining separate authentication implementations for .NET Framework and .NET Core applications by providing a single, consistent interface.

### Problem Statement

Traditionally, developers face challenges when:
- Supporting both legacy .NET Framework applications and modern .NET Core applications
- Maintaining separate authentication codebases for different frameworks
- Dealing with different authentication libraries (OWIN for Framework, Microsoft.Identity.Web for Core)
- Ensuring consistent authentication behavior across different framework versions

### Solution

This package provides:
- **Single API**: One interface (`IIdpAuthenticator`) that works across all frameworks
- **Automatic Detection**: The package automatically uses the correct implementation based on the target framework
- **Unified Configuration**: Similar configuration model for both Framework and Core applications
- **Framework-Specific Optimizations**: Uses the best authentication libraries for each framework

### Target Frameworks

| Framework | Version | Purpose |
|-----------|---------|---------|
| .NET Framework | 4.5, 4.6, 4.7 | Legacy enterprise applications |
| .NET Core | 6.0 | Modern cross-platform applications |
| .NET | 8.0, 9.0 | Latest .NET applications |

---

## Architecture & Design

### Design Principles

1. **Separation of Concerns**: Abstractions, implementations, and shared models are separated
2. **Dependency Inversion**: Code depends on abstractions, not concrete implementations
3. **Single Responsibility**: Each component has one clear purpose
4. **Open/Closed Principle**: Extensible without modification through interfaces

### Architectural Layers

```
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                       │
│  (Uses IIdpAuthenticator interface)                     │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                  Abstraction Layer                       │
│  IIdpAuthenticator (Common Interface)                     │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┴─────────────────┐
        ▼                                     ▼
┌──────────────────┐              ┌──────────────────┐
│  Core Adapter     │              │ Framework Adapter │
│  (.NET Core/6+)   │              │ (.NET Framework) │
└──────────────────┘              └──────────────────┘
        │                                     │
        └─────────────────┬─────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────┐
│              Shared Models Layer                         │
│  TokenValidationResult, WpIdpOptions                     │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│              Framework Libraries                         │
│  Microsoft.Identity.Web (Core)                           │
│  OWIN/Katana (Framework)                                 │
└─────────────────────────────────────────────────────────┘
```

### Project Structure

```
WP.Idp.Auth/
├── Abstractions/              # Common interfaces
│   └── IIdpAuthenticator.cs   # Main contract
│
├── CoreAdapter/               # .NET Core implementation
│   ├── CoreIdpAuthenticator.cs      # Core authenticator
│   └── CoreAuthExtensions.cs        # DI extension methods
│
├── FrameworkAdapter/          # .NET Framework implementation
│   ├── FrameworkIdpAuthenticator.cs  # Framework authenticator
│   └── FrameworkAuthExtensions.cs   # OWIN extension methods
│
└── SharedModels/              # Shared DTOs
    ├── TokenValidationResult.cs     # Validation result model
    └── WpIdpOptions.cs              # Configuration model
```

---

## Multi-Targeting Strategy

### How Multi-Targeting Works

The package uses **conditional compilation** to include only the relevant code for each target framework. This is achieved through:

1. **Conditional Compilation Directives**: `#if NET6_0_OR_GREATER` and `#if !NET6_0_OR_GREATER`
2. **Conditional Package References**: Different NuGet packages for different frameworks
3. **Single Codebase**: All code in one project, compiled differently per target

### Conditional Compilation Flow

```csharp
// CoreAdapter/CoreIdpAuthenticator.cs
#if NET6_0_OR_GREATER
    // This code ONLY compiles for .NET 6.0, 8.0, 9.0
    using Microsoft.Identity.Web;
    // ... Core implementation
#endif

// FrameworkAdapter/FrameworkIdpAuthenticator.cs
#if !NET6_0_OR_GREATER
    // This code ONLY compiles for .NET Framework 4.5, 4.6, 4.7
    using Microsoft.Owin.Security.OpenIdConnect;
    // ... Framework implementation
#endif
```

### Build Process

When you build the project:

1. **For each target framework** (net45, net46, net47, net6.0, net8.0, net9.0):
   - MSBuild evaluates conditional compilation directives
   - Only relevant code is compiled
   - Only relevant package references are included
   - A separate DLL is generated for each target

2. **Result**: One NuGet package containing multiple DLLs, one per target framework

3. **At Runtime**: The consuming application uses the DLL that matches its target framework

### Example Build Output

```
bin/Release/
├── net45/
│   └── WP.Idp.Auth.dll          (Framework implementation)
├── net46/
│   └── WP.Idp.Auth.dll          (Framework implementation)
├── net47/
│   └── WP.Idp.Auth.dll          (Framework implementation)
├── net6.0/
│   └── WP.Idp.Auth.dll          (Core implementation)
├── net8.0/
│   └── WP.Idp.Auth.dll          (Core implementation)
└── net9.0/
    └── WP.Idp.Auth.dll          (Core implementation)
```

---

## Component Breakdown

### 1. Abstractions Layer

#### IIdpAuthenticator Interface

**Location**: `Abstractions/IIdpAuthenticator.cs`

**Purpose**: Defines the contract that both Core and Framework implementations must follow.

```csharp
public interface IIdpAuthenticator
{
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    string GetAuthorizeUrl();
}
```

**Key Points**:
- This is the **only interface** consumers need to know about
- Both adapters implement this interface identically
- Provides a consistent API regardless of underlying framework

**Methods**:

1. **ValidateTokenAsync(string token)**
   - Validates a JWT token
   - Extracts claims (Subject, TenantId, Email, etc.)
   - Returns validation result with extracted claims
   - Throws no exceptions; returns error in result object

2. **GetAuthorizeUrl()**
   - Generates the OAuth/OpenID Connect authorization URL
   - Includes client_id, redirect_uri, scope, response_type
   - Used to initiate the authentication flow

### 2. Shared Models Layer

#### TokenValidationResult

**Location**: `SharedModels/TokenValidationResult.cs`

**Purpose**: Represents the result of token validation.

```csharp
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? Subject { get; set; }
    public string? TenantId { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, string> AdditionalClaims { get; set; }
    public string? ErrorMessage { get; set; }
}
```

**Properties**:
- `IsValid`: Indicates if token validation succeeded
- `Subject`: User identifier (sub, oid, or NameIdentifier claim)
- `TenantId`: Tenant identifier (tid or tenant_id claim)
- `Email`: User email (email, preferred_username, or Email claim)
- `AdditionalClaims`: All other claims from the token
- `ErrorMessage`: Error description if validation failed

#### WpIdpOptions

**Location**: `SharedModels/WpIdpOptions.cs`

**Purpose**: Configuration options for IDP authentication.

```csharp
public class WpIdpOptions
{
    public string Authority { get; set; }              // Required
    public string ClientId { get; set; }              // Required
    public string? ClientSecret { get; set; }         // Optional
    public string RedirectUri { get; set; }            // Required
    public string Scope { get; set; }                 // Default: "openid profile email"
    public string ResponseType { get; set; }          // Default: "code id_token"
    public bool RequireHttps { get; set; }            // Default: true
    public string? MetadataAddress { get; set; }       // Optional
    public List<string> ValidAudiences { get; set; }  // Optional
    public List<string> ValidIssuers { get; set; }     // Optional
}
```

**Configuration Sources**:
- For Core: `appsettings.json` → `WpIdpAuth` section
- For Framework: Direct instantiation in `Startup.cs`

### 3. Core Adapter Layer

#### CoreIdpAuthenticator

**Location**: `CoreAdapter/CoreIdpAuthenticator.cs`

**Purpose**: .NET Core/6/8/9 implementation of `IIdpAuthenticator`.

**Key Technologies**:
- `Microsoft.Identity.Web`: Microsoft's official authentication library
- `System.IdentityModel.Tokens.Jwt`: JWT token handling
- `JwtSecurityTokenHandler`: Token validation

**Implementation Details**:

1. **Constructor**:
   ```csharp
   public CoreIdpAuthenticator(WpIdpOptions options)
   {
       _options = options ?? throw new ArgumentNullException(nameof(options));
       _validationParameters = CreateValidationParameters();
   }
   ```
   - Validates options
   - Pre-creates token validation parameters for performance

2. **ValidateTokenAsync**:
   - Uses `JwtSecurityTokenHandler` to validate tokens
   - Extracts claims using multiple fallback strategies
   - Handles exceptions gracefully, returning error results

3. **GetAuthorizeUrl**:
   - Constructs Azure AD v2.0 authorization endpoint
   - URL-encodes all parameters
   - Supports custom scopes and response types

4. **CreateValidationParameters**:
   - Configures issuer validation
   - Configures audience validation
   - Sets clock skew tolerance (5 minutes)
   - Enables lifetime validation

#### CoreAuthExtensions

**Location**: `CoreAdapter/CoreAuthExtensions.cs`

**Purpose**: Extension methods for .NET Core dependency injection.

**Methods**:

1. **AddWpIdpAuth(IServiceCollection, IConfiguration)**
   - Reads configuration from `appsettings.json`
   - Binds to `WpIdpAuth` section
   - Validates required properties
   - Calls the options-based overload

2. **AddWpIdpAuth(IServiceCollection, WpIdpOptions)**
   - Registers `WpIdpOptions` as singleton
   - Registers `IIdpAuthenticator` → `CoreIdpAuthenticator`
   - Configures Microsoft Identity Web
   - Configures JWT Bearer authentication
   - Enables token acquisition for downstream APIs
   - Adds in-memory token cache

**Helper Methods**:
- `ExtractInstance`: Extracts base URL from authority
- `ExtractDomain`: Extracts domain from authority
- `ExtractTenantId`: Extracts tenant ID from Azure AD authority
- `ExtractCallbackPath`: Extracts callback path from redirect URI

### 4. Framework Adapter Layer

#### FrameworkIdpAuthenticator

**Location**: `FrameworkAdapter/FrameworkIdpAuthenticator.cs`

**Purpose**: .NET Framework 4.x implementation of `IIdpAuthenticator`.

**Key Technologies**:
- `Microsoft.Owin.Security.OpenIdConnect`: OWIN OpenID Connect middleware
- `System.IdentityModel.Tokens.Jwt`: JWT token handling (same as Core)
- `JwtSecurityTokenHandler`: Token validation (same as Core)

**Implementation Details**:

The implementation is **nearly identical** to `CoreIdpAuthenticator`:
- Same token validation logic
- Same claim extraction logic
- Same authorization URL generation
- Same error handling

**Why Separate?**
- Different namespace (prevents conflicts)
- Different dependency injection (OWIN vs ASP.NET Core DI)
- Framework-specific optimizations possible in future

#### FrameworkAuthExtensions

**Location**: `FrameworkAdapter/FrameworkAuthExtensions.cs`

**Purpose**: Extension methods for OWIN pipeline configuration.

**Methods**:

1. **UseWpIdpAuth(IAppBuilder, WpIdpOptions)**
   - Validates required options
   - Configures cookie authentication
   - Configures OpenID Connect authentication
   - Sets up authentication notifications
   - Configures token validation parameters

**OWIN Pipeline Configuration**:

```csharp
app.UseCookieAuthentication(...)           // Step 1: Cookie auth
   .UseOpenIdConnectAuthentication(...)     // Step 2: OIDC auth
```

**Authentication Notifications**:
- `AuthenticationFailed`: Handles authentication errors, redirects to error page
- `SecurityTokenValidated`: Hook for additional token validation

---

## Implementation Details

### Token Validation Process

Both adapters follow the same validation process:

```
1. Input: JWT Token String
   │
   ▼
2. Check if token is null/empty
   │
   ▼
3. Create JwtSecurityTokenHandler
   │
   ▼
4. Check if token can be read (CanReadToken)
   │
   ▼
5. Validate token with TokenValidationParameters
   │   ├─ Validate Issuer
   │   ├─ Validate Audience
   │   ├─ Validate Lifetime
   │   └─ Validate Signature
   │
   ▼
6. Extract ClaimsPrincipal
   │
   ▼
7. Extract Claims:
   │   ├─ Subject (sub, oid, NameIdentifier)
   │   ├─ TenantId (tid, tenant_id)
   │   ├─ Email (email, preferred_username, Email)
   │   └─ All other claims → AdditionalClaims
   │
   ▼
8. Return TokenValidationResult
```

### Claim Extraction Strategy

The package uses a **fallback strategy** for claim extraction:

```csharp
// Subject extraction (tries multiple claim types)
result.Subject = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value  // Standard .NET claim
    ?? principal.FindFirst("sub")?.Value                                  // OIDC standard
    ?? principal.FindFirst("oid")?.Value;                                // Azure AD specific

// Email extraction (tries multiple claim types)
result.Email = principal.FindFirst(ClaimTypes.Email)?.Value              // Standard .NET claim
    ?? principal.FindFirst("email")?.Value                               // OIDC standard
    ?? principal.FindFirst("preferred_username")?.Value;                  // Azure AD fallback
```

**Why Multiple Fallbacks?**
- Different IDPs use different claim names
- Azure AD uses `oid` for subject, Auth0 uses `sub`
- Ensures compatibility across providers

### Authorization URL Generation

Both adapters generate URLs in the same format:

```
{Authority}/oauth2/v2.0/authorize?
    client_id={ClientId}&
    redirect_uri={RedirectUri}&
    response_type={ResponseType}&
    scope={Scope}&
    response_mode=form_post
```

**URL Encoding**: All parameters are URL-encoded using `Uri.EscapeDataString()`

**Azure AD v2.0 Format**: Uses `/oauth2/v2.0/authorize` endpoint (supports both Azure AD and Auth0-style providers)

### Token Validation Parameters

Both adapters create similar validation parameters:

```csharp
new TokenValidationParameters
{
    ValidateIssuer = true,              // Must validate issuer
    ValidIssuers = [...],               // List of acceptable issuers
    ValidateAudience = true,            // Must validate audience
    ValidAudiences = [...],             // List of acceptable audiences
    ValidateLifetime = true,            // Must validate expiration
    ValidateIssuerSigningKey = true,    // Must validate signature
    RequireSignedTokens = true,        // Tokens must be signed
    ClockSkew = TimeSpan.FromMinutes(5) // 5-minute clock skew tolerance
}
```

**Default Values**:
- If `ValidIssuers` not specified → Uses `Authority`
- If `ValidAudiences` not specified → Uses `ClientId`

---

## How It Works

### Runtime Behavior

#### For .NET Core Applications

1. **Application Startup**:
   ```csharp
   builder.Services.AddWpIdpAuth(builder.Configuration);
   ```
   - Extension method registers services
   - Configuration is read from `appsettings.json`
   - `CoreIdpAuthenticator` is registered as `IIdpAuthenticator`

2. **Dependency Injection**:
   - When a controller/service requests `IIdpAuthenticator`
   - DI container provides `CoreIdpAuthenticator` instance
   - Instance is singleton (shared across application)

3. **Authentication Flow**:
   - User accesses protected endpoint
   - ASP.NET Core middleware redirects to IDP
   - User authenticates with IDP
   - IDP redirects back with token
   - Middleware validates token
   - User is authenticated

4. **Token Validation**:
   ```csharp
   var result = await _authenticator.ValidateTokenAsync(token);
   ```
   - Uses `JwtSecurityTokenHandler`
   - Validates against configured parameters
   - Returns `TokenValidationResult`

#### For .NET Framework Applications

1. **Application Startup**:
   ```csharp
   app.UseWpIdpAuth(new WpIdpOptions { ... });
   ```
   - Extension method configures OWIN middleware
   - Cookie authentication is configured
   - OpenID Connect authentication is configured

2. **OWIN Pipeline**:
   - OWIN middleware processes requests
   - Cookie middleware checks for authentication cookie
   - If not authenticated, redirects to OpenID Connect
   - OpenID Connect middleware handles authentication

3. **Authentication Flow**:
   - User accesses protected endpoint
   - OWIN middleware checks authentication
   - If not authenticated, redirects to IDP
   - User authenticates with IDP
   - IDP redirects back with token
   - Middleware validates token
   - User is authenticated

4. **Token Validation**:
   - Token validation happens automatically in middleware
   - Can also use `FrameworkIdpAuthenticator` directly (if registered)

### Conditional Compilation in Action

**At Compile Time**:

When building for `.NET 6.0`:
```csharp
// ✅ This code is INCLUDED
#if NET6_0_OR_GREATER
public class CoreIdpAuthenticator { ... }
#endif

// ❌ This code is EXCLUDED
#if !NET6_0_OR_GREATER
public class FrameworkIdpAuthenticator { ... }
#endif
```

When building for `.NET Framework 4.5`:
```csharp
// ❌ This code is EXCLUDED
#if NET6_0_OR_GREATER
public class CoreIdpAuthenticator { ... }
#endif

// ✅ This code is INCLUDED
#if !NET6_0_OR_GREATER
public class FrameworkIdpAuthenticator { ... }
#endif
```

**Result**: Each target framework gets only the code it needs.

---

## Usage Patterns

### Pattern 1: .NET Core with Configuration

**appsettings.json**:
```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{tenantId}",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "https://localhost:5001/signin-oidc",
    "Scope": "openid profile email"
  }
}
```

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add WP IDP Authentication
builder.Services.AddWpIdpAuth(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

**Controller**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataController : ControllerBase
{
    private readonly IIdpAuthenticator _authenticator;

    public DataController(IIdpAuthenticator authenticator)
    {
        _authenticator = authenticator;
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var token = await HttpContext.GetTokenAsync("access_token");
        var result = await _authenticator.ValidateTokenAsync(token);
        
        return Ok(new { 
            IsValid = result.IsValid,
            Email = result.Email,
            Subject = result.Subject 
        });
    }
}
```

### Pattern 2: .NET Core with Options

**Program.cs**:
```csharp
builder.Services.AddWpIdpAuth(new WpIdpOptions
{
    Authority = "https://login.microsoftonline.com/{tenantId}",
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret",
    RedirectUri = "https://localhost:5001/signin-oidc"
});
```

### Pattern 3: .NET Framework with OWIN

**Startup.cs**:
```csharp
[assembly: OwinStartup(typeof(YourApp.Startup))]

namespace YourApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWpIdpAuth(new WpIdpOptions
            {
                Authority = "https://login.microsoftonline.com/{tenantId}",
                ClientId = "your-client-id",
                ClientSecret = "your-client-secret",
                RedirectUri = "https://localhost:44300/",
                Scope = "openid profile email"
            });
        }
    }
}
```

**Controller**:
```csharp
[Authorize]
public class HomeController : Controller
{
    public ActionResult Index()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var email = claimsIdentity?.FindFirst("email")?.Value;
        
        return View();
    }
}
```

### Pattern 4: Direct Token Validation

```csharp
// In any service/controller
var authenticator = serviceProvider.GetService<IIdpAuthenticator>();
var result = await authenticator.ValidateTokenAsync(tokenString);

if (result.IsValid)
{
    var email = result.Email;
    var subject = result.Subject;
    // Use claims...
}
else
{
    var error = result.ErrorMessage;
    // Handle error...
}
```

---

## Dependencies & Package Management

### Dependency Strategy

The package uses **framework-specific dependencies** to ensure compatibility:

#### .NET Framework 4.x Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `System.IdentityModel.Tokens.Jwt` | 6.35.0 | JWT token handling |
| `Microsoft.Owin.Security.OpenIdConnect` | 4.2.0 | OpenID Connect middleware |
| `Microsoft.Owin.Security.Jwt` | 4.2.0 | JWT validation middleware |
| `Microsoft.Owin.Security.Cookies` | 4.2.0 | Cookie authentication |
| `Microsoft.Owin.Host.SystemWeb` | 4.2.0 | OWIN host for IIS |
| `Owin` | 1.0.0 | OWIN abstraction |

**Why These Versions?**
- Latest stable versions compatible with .NET Framework 4.5+
- `System.IdentityModel.Tokens.Jwt` 6.35.0 is the last version supporting .NET Framework 4.5

#### .NET 6.0 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `System.IdentityModel.Tokens.Jwt` | 6.35.0 | JWT token handling |
| `Microsoft.Identity.Web` | 2.15.1 | Microsoft's authentication library |
| `Microsoft.Extensions.Configuration.Abstractions` | 6.0.0 | Configuration support |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 6.0.0 | DI support |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 6.0.25 | JWT Bearer authentication |

**Why These Versions?**
- Compatible with .NET 6.0
- `Microsoft.Identity.Web` 2.15.1 is the latest 2.x version (stable for .NET 6)

#### .NET 8.0 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `System.IdentityModel.Tokens.Jwt` | 8.0.1 | JWT token handling |
| `Microsoft.Identity.Web` | 3.0.0 | Microsoft's authentication library |
| `Microsoft.Extensions.Configuration.Abstractions` | 8.0.0 | Configuration support |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.0 | DI support |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | JWT Bearer authentication |

**Why These Versions?**
- Latest versions for .NET 8.0
- `System.IdentityModel.Tokens.Jwt` 8.0.1 required by `Microsoft.Identity.Web` 3.0.0

#### .NET 9.0 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `System.IdentityModel.Tokens.Jwt` | 8.0.1 | JWT token handling |
| `Microsoft.Identity.Web` | 3.0.0 | Microsoft's authentication library |
| `Microsoft.Extensions.Configuration.Abstractions` | 9.0.0 | Configuration support |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 9.0.0 | DI support |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.0 | JWT Bearer authentication |

**Why These Versions?**
- Latest versions for .NET 9.0
- Aligned with .NET 9.0 runtime

### Conditional Package References

In the `.csproj` file:

```xml
<!-- Only included when building for .NET Framework -->
<ItemGroup Condition="'$(TargetFramework)' == 'net45' OR ...">
    <PackageReference Include="Microsoft.Owin.Security.OpenIdConnect" Version="4.2.0" />
</ItemGroup>

<!-- Only included when building for .NET 6.0 -->
<ItemGroup Condition="'$(TargetFramework)' == 'net6.0'">
    <PackageReference Include="Microsoft.Identity.Web" Version="2.15.1" />
</ItemGroup>
```

**How It Works**:
- MSBuild evaluates the condition at build time
- Only matching packages are included in the build
- Each target framework gets its own set of dependencies

---

## Code Flow & Execution

### .NET Core Application Flow

```
1. Application Starts
   │
   ▼
2. Program.cs: builder.Services.AddWpIdpAuth(config)
   │
   ▼
3. CoreAuthExtensions.AddWpIdpAuth()
   │   ├─ Reads configuration
   │   ├─ Validates options
   │   ├─ Registers WpIdpOptions (singleton)
   │   ├─ Registers IIdpAuthenticator → CoreIdpAuthenticator (singleton)
   │   ├─ Configures Microsoft Identity Web
   │   └─ Configures JWT Bearer authentication
   │
   ▼
4. User Requests Protected Endpoint
   │
   ▼
5. ASP.NET Core Authentication Middleware
   │   ├─ Checks for authentication
   │   ├─ If not authenticated → Redirect to IDP
   │   └─ If authenticated → Continue
   │
   ▼
6. IDP Authentication
   │   ├─ User logs in
   │   ├─ IDP issues token
   │   └─ Redirects back to application
   │
   ▼
7. Middleware Validates Token
   │   ├─ Extracts token from response
   │   ├─ Validates signature, issuer, audience, lifetime
   │   └─ Creates ClaimsPrincipal
   │
   ▼
8. User is Authenticated
   │
   ▼
9. Controller/Service Uses IIdpAuthenticator
   │   ├─ DI provides CoreIdpAuthenticator instance
   │   ├─ ValidateTokenAsync() called
   │   └─ Returns TokenValidationResult
```

### .NET Framework Application Flow

```
1. Application Starts
   │
   ▼
2. Startup.Configuration(): app.UseWpIdpAuth(options)
   │
   ▼
3. FrameworkAuthExtensions.UseWpIdpAuth()
   │   ├─ Validates options
   │   ├─ Configures Cookie Authentication
   │   └─ Configures OpenID Connect Authentication
   │
   ▼
4. User Requests Protected Endpoint
   │
   ▼
5. OWIN Middleware Pipeline
   │   ├─ Cookie Middleware checks for cookie
   │   ├─ If no cookie → OpenID Connect middleware
   │   └─ If cookie exists → Continue
   │
   ▼
6. OpenID Connect Middleware
   │   ├─ Redirects to IDP
   │   ├─ User authenticates
   │   ├─ IDP redirects back
   │   └─ Validates token
   │
   ▼
7. Token Validation
   │   ├─ Extracts token
   │   ├─ Validates with TokenValidationParameters
   │   └─ Creates ClaimsPrincipal
   │
   ▼
8. Cookie Created
   │   ├─ ClaimsPrincipal stored in cookie
   │   └─ User is authenticated
   │
   ▼
9. Subsequent Requests
   │   ├─ Cookie middleware reads cookie
   │   └─ User is automatically authenticated
```

### Token Validation Flow (Both Frameworks)

```
1. ValidateTokenAsync(token) Called
   │
   ▼
2. Check if token is null/empty
   │   └─ If yes → Return error result
   │
   ▼
3. Create JwtSecurityTokenHandler
   │
   ▼
4. Check CanReadToken(token)
   │   └─ If no → Return error result
   │
   ▼
5. Validate Token
   │   ├─ ValidateIssuer: Check if issuer matches
   │   ├─ ValidateAudience: Check if audience matches
   │   ├─ ValidateLifetime: Check if token is expired
   │   └─ ValidateIssuerSigningKey: Check signature
   │
   ▼
6. If Validation Fails
   │   └─ Return error result with exception message
   │
   ▼
7. Extract ClaimsPrincipal
   │
   ▼
8. Extract Claims
   │   ├─ Subject: Try NameIdentifier → sub → oid
   │   ├─ TenantId: Try tid → tenant_id
   │   ├─ Email: Try Email → email → preferred_username
   │   └─ All other claims → AdditionalClaims dictionary
   │
   ▼
9. Return TokenValidationResult
   │   ├─ IsValid = true
   │   ├─ Subject, TenantId, Email populated
   │   └─ AdditionalClaims populated
```

---

## Extension Points & Customization

### Customizing Token Validation

You can extend the validation by:

1. **Custom ValidIssuers/ValidAudiences**:
   ```csharp
   var options = new WpIdpOptions
   {
       Authority = "...",
       ClientId = "...",
       ValidIssuers = new List<string> { "issuer1", "issuer2" },
       ValidAudiences = new List<string> { "audience1", "audience2" }
   };
   ```

2. **Custom Metadata Address**:
   ```csharp
   options.MetadataAddress = "https://custom-metadata-endpoint";
   ```

### Custom Claim Extraction

To extract custom claims, access `AdditionalClaims`:

```csharp
var result = await _authenticator.ValidateTokenAsync(token);
if (result.IsValid)
{
    var customClaim = result.AdditionalClaims["custom_claim"];
}
```

### Extending the Interface

To add new functionality:

1. **Extend IIdpAuthenticator**:
   ```csharp
   public interface IIdpAuthenticator
   {
       Task<TokenValidationResult> ValidateTokenAsync(string token);
       string GetAuthorizeUrl();
       Task<string> RefreshTokenAsync(string refreshToken); // New method
   }
   ```

2. **Implement in Both Adapters**:
   - Add implementation to `CoreIdpAuthenticator`
   - Add implementation to `FrameworkIdpAuthenticator`

### Custom Authentication Notifications (Framework Only)

In `FrameworkAuthExtensions`, you can customize notifications:

```csharp
Notifications = new OpenIdConnectAuthenticationNotifications
{
    SecurityTokenValidated = context =>
    {
        // Add custom claims
        context.AuthenticationTicket.Identity.AddClaim(
            new Claim("custom_claim", "value"));
        return Task.FromResult(0);
    }
}
```

---

## Security Considerations

### Token Validation

✅ **Validated Properties**:
- Issuer (prevents token from wrong IDP)
- Audience (prevents token for wrong application)
- Lifetime (prevents expired tokens)
- Signature (prevents tampered tokens)

✅ **Security Features**:
- Clock skew tolerance (5 minutes) for time synchronization
- HTTPS metadata requirement (configurable)
- Signed tokens required

### Configuration Security

⚠️ **Best Practices**:
- Store `ClientSecret` in secure configuration (Azure Key Vault, User Secrets)
- Use HTTPS for all redirect URIs in production
- Validate `ValidIssuers` and `ValidAudiences` explicitly
- Use environment-specific configuration

### Known Vulnerabilities

⚠️ **Microsoft.Owin.Security.Cookies 4.2.0**:
- Has a known high severity vulnerability
- Consider updating when a patched version is available
- Monitor security advisories

### Recommendations

1. **Always use HTTPS** in production
2. **Validate all tokens** before trusting claims
3. **Use short-lived tokens** with refresh token pattern
4. **Store secrets securely** (never in source code)
5. **Regularly update dependencies** for security patches

---

## Troubleshooting

### Common Issues

#### Issue 1: "Token validation failed: Invalid issuer"

**Cause**: Token issuer doesn't match configured issuers.

**Solution**:
```csharp
options.ValidIssuers = new List<string> 
{ 
    "https://login.microsoftonline.com/{tenantId}/v2.0",
    "https://login.microsoftonline.com/{tenantId}"
};
```

#### Issue 2: "Token validation failed: Invalid audience"

**Cause**: Token audience doesn't match client ID.

**Solution**:
```csharp
options.ValidAudiences = new List<string> 
{ 
    options.ClientId,
    "api://" + options.ClientId  // If using API scope
};
```

#### Issue 3: "Token is expired"

**Cause**: Token lifetime exceeded (including clock skew).

**Solution**: Check system time synchronization, or increase clock skew:
```csharp
// In CreateValidationParameters()
ClockSkew = TimeSpan.FromMinutes(10) // Increase from 5 to 10 minutes
```

#### Issue 4: Configuration not found (.NET Core)

**Cause**: Configuration section name mismatch.

**Solution**: Ensure `appsettings.json` has:
```json
{
  "WpIdpAuth": {
    "Authority": "...",
    "ClientId": "..."
  }
}
```

#### Issue 5: OWIN not working (.NET Framework)

**Cause**: Missing `Startup.cs` or incorrect assembly attribute.

**Solution**: Ensure `Startup.cs` has:
```csharp
[assembly: OwinStartup(typeof(YourApp.Startup))]
```

### Debugging Tips

1. **Enable Logging**:
   ```csharp
   // .NET Core
   builder.Logging.AddConsole();
   
   // .NET Framework
   // Check Application Insights or Event Viewer
   ```

2. **Inspect Token**:
   ```csharp
   var handler = new JwtSecurityTokenHandler();
   var token = handler.ReadJwtToken(tokenString);
   var claims = token.Claims;
   // Inspect claims to see what's in the token
   ```

3. **Check Validation Parameters**:
   ```csharp
   // Add breakpoint in CreateValidationParameters()
   // Inspect _validationParameters values
   ```

---

## Conclusion

**WP.Idp.Auth** provides a robust, cross-version solution for IDP authentication in .NET applications. By leveraging conditional compilation and framework-specific implementations, it offers:

- ✅ **Unified API** across all .NET versions
- ✅ **Automatic framework detection**
- ✅ **Best-of-breed libraries** for each framework
- ✅ **Consistent behavior** regardless of target framework
- ✅ **Easy integration** with extension methods
- ✅ **Comprehensive token validation**
- ✅ **Flexible configuration**

The package successfully bridges the gap between legacy .NET Framework applications and modern .NET Core applications, allowing teams to use the same authentication code across their entire .NET ecosystem.

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Package Version**: 1.0.0

