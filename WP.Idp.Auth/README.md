# WP.Idp.Auth

A cross-version NuGet package providing unified IDP (Identity Provider) authentication support for both .NET Framework 4.x and .NET Core/.NET 6/8/9 applications.

> **Note**: To add .NET 10.0 support when it becomes available, simply add `net10.0` to the `<TargetFrameworks>` in the csproj file.

## Features

- ✅ **Multi-Targeting**: Supports .NET Framework 4.5, 4.6, 4.7 and .NET Core 6, 8, 9
- ✅ **Unified API**: Single interface (`IIdpAuthenticator`) across all frameworks
- ✅ **Azure Entra ID Support**: Full support for Azure AD / Entra ID authentication
- ✅ **Auth0 Support**: Compatible with Auth0 authentication
- ✅ **Automatic Framework Detection**: Automatically uses the correct implementation based on target framework
- ✅ **Token Validation**: Built-in JWT token validation
- ✅ **Extension Methods**: Easy integration with both OWIN (Framework) and ASP.NET Core (Core)

## Installation

```bash
dotnet add package WP.Idp.Auth
```

Or via NuGet Package Manager:
```
Install-Package WP.Idp.Auth
```

## Quick Start

### For .NET Core 6/8/9 Applications

#### 1. Configure in `appsettings.json`:

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{tenantId}",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "https://localhost:5001/signin-oidc",
    "Scope": "openid profile email",
    "RequireHttps": true
  }
}
```

#### 2. Register services in `Program.cs`:

```csharp
using WP.Idp.Auth.CoreAdapter;

var builder = WebApplication.CreateBuilder(args);

// Add WP IDP Authentication
builder.Services.AddWpIdpAuth(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

#### 3. Use in controllers:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WP.Idp.Auth.Abstractions;

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

### For .NET Framework 4.x Applications

#### 1. Configure in `Startup.cs`:

```csharp
using Microsoft.Owin;
using Owin;
using WP.Idp.Auth.FrameworkAdapter;
using WP.Idp.Auth.SharedModels;

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
                Scope = "openid profile email",
                RequireHttps = true
            });
        }
    }
}
```

#### 2. Protect controllers/actions:

```csharp
using System.Web.Mvc;
using System.Web;

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

## Configuration Options

### WpIdpOptions

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Authority` | string | Yes | The authority URL (e.g., `https://login.microsoftonline.com/{tenantId}`) |
| `ClientId` | string | Yes | The client/application ID |
| `ClientSecret` | string | No | The client secret (required for some flows) |
| `RedirectUri` | string | Yes | The redirect URI after authentication |
| `Scope` | string | No | Space-separated scopes (default: `"openid profile email"`) |
| `ResponseType` | string | No | Response type (default: `"code id_token"`) |
| `RequireHttps` | bool | No | Require HTTPS metadata (default: `true`) |
| `MetadataAddress` | string | No | Override metadata address (auto-discovered if not provided) |
| `ValidAudiences` | List<string> | No | Valid audiences for token validation |
| `ValidIssuers` | List<string> | No | Valid issuers for token validation |

## API Reference

### IIdpAuthenticator Interface

```csharp
public interface IIdpAuthenticator
{
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    string GetAuthorizeUrl();
}
```

### TokenValidationResult

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

## Architecture

The package uses conditional compilation to provide framework-specific implementations:

- **Abstractions**: Common interfaces shared across all frameworks
- **CoreAdapter**: Implementation for .NET Core 6/8/10 using `Microsoft.Identity.Web`
- **FrameworkAdapter**: Implementation for .NET Framework 4.x using OWIN/Katana
- **SharedModels**: DTOs and configuration classes used by both adapters

## Building from Source

```bash
git clone <repository-url>
cd WP.Idp.Auth
dotnet restore
dotnet build
dotnet pack
```

## Publishing to NuGet

### 1. Build the package:

```bash
dotnet pack -c Release
```

This creates a `.nupkg` file in the `bin/Release` directory.

### 2. Publish to NuGet.org:

```bash
dotnet nuget push bin/Release/WP.Idp.Auth.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### 3. Or publish to a private feed:

```bash
dotnet nuget push bin/Release/WP.Idp.Auth.1.0.0.nupkg --api-key YOUR_API_KEY --source https://your-private-feed-url/v3/index.json
```

## Requirements

### .NET Framework 4.x
- Microsoft.Owin.Security.OpenIdConnect 4.2.0
- Microsoft.Owin.Security.Jwt 4.2.0
- OWIN 1.0.0

### .NET Core 6/8/10
- Microsoft.Identity.Web 3.0.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0

## License

MIT License

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Support

For issues and questions, please open an issue on the GitHub repository.

