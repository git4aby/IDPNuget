# Entra ID (Azure AD) Configuration Guide

This guide shows exactly where and how to configure your Entra ID (Azure AD) settings like Client ID, Client Secret, Tenant ID, etc.

## Configuration Locations

The configuration location depends on whether you're using **.NET Core** or **.NET Framework**:

---

## For .NET Core 6/8/9 Applications

### Method 1: Using appsettings.json (Recommended)

**Location**: `appsettings.json` or `appsettings.{Environment}.json`

**Configuration Section**: `WpIdpAuth`

**Example Configuration**:

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{your-tenant-id}",
    "ClientId": "your-application-client-id",
    "ClientSecret": "your-client-secret-value",
    "RedirectUri": "https://localhost:5001/signin-oidc",
    "Scope": "openid profile email",
    "ResponseType": "code id_token",
    "RequireHttps": true
  }
}
```

**Where to find these values in Azure Portal**:

1. **Authority**: 
   - Format: `https://login.microsoftonline.com/{tenant-id}`
   - Find Tenant ID: Azure Portal → Azure Active Directory → Overview → Tenant ID
   - Or use: `https://login.microsoftonline.com/common` (for multi-tenant)

2. **ClientId** (Application ID):
   - Azure Portal → App Registrations → Your App → Overview → Application (client) ID

3. **ClientSecret**:
   - Azure Portal → App Registrations → Your App → Certificates & secrets → Client secrets
   - Create a new secret if needed
   - ⚠️ **Important**: Store this securely (use User Secrets for development, Azure Key Vault for production)

4. **RedirectUri**:
   - Must match what's configured in Azure Portal
   - Azure Portal → App Registrations → Your App → Authentication → Redirect URIs
   - Common values:
     - Development: `https://localhost:5001/signin-oidc`
     - Production: `https://yourdomain.com/signin-oidc`

**Usage in Program.cs**:

```csharp
using WP.Idp.Auth.CoreAdapter;

var builder = WebApplication.CreateBuilder(args);

// Reads from appsettings.json automatically
builder.Services.AddWpIdpAuth(builder.Configuration);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

### Method 2: Using Code (Programmatic)

**Location**: `Program.cs`

**Example**:

```csharp
using WP.Idp.Auth.CoreAdapter;
using WP.Idp.Auth.SharedModels;

var builder = WebApplication.CreateBuilder(args);

// Configure directly in code
builder.Services.AddWpIdpAuth(new WpIdpOptions
{
    Authority = "https://login.microsoftonline.com/{your-tenant-id}",
    ClientId = "your-application-client-id",
    ClientSecret = "your-client-secret-value",
    RedirectUri = "https://localhost:5001/signin-oidc",
    Scope = "openid profile email",
    ResponseType = "code id_token",
    RequireHttps = true
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

### Method 3: Using Environment Variables

**Location**: Environment variables or `appsettings.{Environment}.json`

**appsettings.Development.json**:

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{dev-tenant-id}",
    "ClientId": "dev-client-id",
    "ClientSecret": "dev-client-secret",
    "RedirectUri": "https://localhost:5001/signin-oidc"
  }
}
```

**appsettings.Production.json**:

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{prod-tenant-id}",
    "ClientId": "prod-client-id",
    "ClientSecret": "prod-client-secret",
    "RedirectUri": "https://yourdomain.com/signin-oidc"
  }
}
```

**Or use User Secrets (for development)**:

```bash
dotnet user-secrets set "WpIdpAuth:ClientSecret" "your-secret-value"
```

---

## For .NET Framework 4.x Applications

### Configuration Location: Startup.cs

**Location**: `App_Start/Startup.cs` or `Startup.cs` in root

**Example Configuration**:

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
                // Entra ID Configuration
                Authority = "https://login.microsoftonline.com/{your-tenant-id}",
                ClientId = "your-application-client-id",
                ClientSecret = "your-client-secret-value",
                RedirectUri = "https://localhost:44300/",
                
                // Optional settings
                Scope = "openid profile email",
                ResponseType = "code id_token",
                RequireHttps = true,
                
                // Advanced settings (optional)
                ValidIssuers = new List<string>
                {
                    "https://login.microsoftonline.com/{your-tenant-id}/v2.0",
                    "https://sts.windows.net/{your-tenant-id}/"
                },
                ValidAudiences = new List<string>
                {
                    "your-application-client-id",
                    "api://your-application-client-id"
                }
            });
        }
    }
}
```

### Alternative: Using Web.config (for some settings)

**Location**: `Web.config`

You can store non-sensitive values in `Web.config` and read them in `Startup.cs`:

**Web.config**:

```xml
<appSettings>
  <add key="WpIdpAuth:Authority" value="https://login.microsoftonline.com/{tenant-id}" />
  <add key="WpIdpAuth:ClientId" value="your-client-id" />
  <add key="WpIdpAuth:RedirectUri" value="https://localhost:44300/" />
</appSettings>
```

**Startup.cs**:

```csharp
public void Configuration(IAppBuilder app)
{
    var authority = ConfigurationManager.AppSettings["WpIdpAuth:Authority"];
    var clientId = ConfigurationManager.AppSettings["WpIdpAuth:ClientId"];
    var redirectUri = ConfigurationManager.AppSettings["WpIdpAuth:RedirectUri"];
    var clientSecret = ConfigurationManager.AppSettings["WpIdpAuth:ClientSecret"]; // ⚠️ Not recommended for secrets
    
    app.UseWpIdpAuth(new WpIdpOptions
    {
        Authority = authority,
        ClientId = clientId,
        ClientSecret = clientSecret,
        RedirectUri = redirectUri
    });
}
```

⚠️ **Security Note**: Never store `ClientSecret` in `Web.config` in production. Use secure configuration methods.

---

## Complete Configuration Properties

### Required Properties

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `Authority` | string | Entra ID authority URL | `https://login.microsoftonline.com/{tenant-id}` |
| `ClientId` | string | Application (client) ID from Azure Portal | `12345678-1234-1234-1234-123456789abc` |
| `RedirectUri` | string | Redirect URI after authentication | `https://localhost:5001/signin-oidc` |

### Optional Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ClientSecret` | string? | null | Client secret (required for some flows) |
| `Scope` | string | `"openid profile email"` | Space-separated scopes |
| `ResponseType` | string | `"code id_token"` | OAuth response type |
| `RequireHttps` | bool | `true` | Require HTTPS for metadata |
| `MetadataAddress` | string? | null | Override metadata endpoint |
| `ValidIssuers` | List<string> | `[Authority]` | Valid token issuers |
| `ValidAudiences` | List<string> | `[ClientId]` | Valid token audiences |

---

## Azure Portal Setup Checklist

Before configuring, ensure you've set up your app in Azure Portal:

### 1. Create App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Click **New registration**
4. Enter:
   - **Name**: Your application name
   - **Supported account types**: Choose appropriate option
   - **Redirect URI**: Add your redirect URI (e.g., `https://localhost:5001/signin-oidc`)

### 2. Get Client ID

1. After registration, go to **Overview**
2. Copy the **Application (client) ID** → This is your `ClientId`

### 3. Get Tenant ID

1. In **Overview**, copy the **Directory (tenant) ID** → Use in `Authority` URL

### 4. Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Enter description and expiration
4. Click **Add**
5. **⚠️ IMPORTANT**: Copy the secret value immediately (you won't see it again)
6. This is your `ClientSecret`

### 5. Configure Redirect URIs

1. Go to **Authentication**
2. Under **Redirect URIs**, add:
   - Development: `https://localhost:5001/signin-oidc`
   - Production: `https://yourdomain.com/signin-oidc`
3. Under **Implicit grant and hybrid flows**, check:
   - ✅ ID tokens (used for implicit and hybrid flows)

### 6. Configure API Permissions (if needed)

1. Go to **API permissions**
2. Add required permissions (e.g., Microsoft Graph)
3. Grant admin consent if needed

---

## Example Configurations

### Example 1: Single-Tenant Application

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/12345678-1234-1234-1234-123456789abc",
    "ClientId": "87654321-4321-4321-4321-cba987654321",
    "ClientSecret": "your-secret-value",
    "RedirectUri": "https://localhost:5001/signin-oidc"
  }
}
```

### Example 2: Multi-Tenant Application

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/common",
    "ClientId": "87654321-4321-4321-4321-cba987654321",
    "ClientSecret": "your-secret-value",
    "RedirectUri": "https://localhost:5001/signin-oidc"
  }
}
```

### Example 3: With Custom Scopes

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{tenant-id}",
    "ClientId": "your-client-id",
    "ClientSecret": "your-secret",
    "RedirectUri": "https://localhost:5001/signin-oidc",
    "Scope": "openid profile email User.Read"
  }
}
```

### Example 4: With Custom Validation

```json
{
  "WpIdpAuth": {
    "Authority": "https://login.microsoftonline.com/{tenant-id}",
    "ClientId": "your-client-id",
    "ClientSecret": "your-secret",
    "RedirectUri": "https://localhost:5001/signin-oidc",
    "ValidIssuers": [
      "https://login.microsoftonline.com/{tenant-id}/v2.0",
      "https://sts.windows.net/{tenant-id}/"
    ],
    "ValidAudiences": [
      "your-client-id",
      "api://your-client-id"
    ]
  }
}
```

---

## Security Best Practices

### ✅ DO:

1. **Store secrets securely**:
   - Development: Use User Secrets (`dotnet user-secrets`)
   - Production: Use Azure Key Vault or secure configuration

2. **Use environment-specific configuration**:
   - `appsettings.Development.json` for dev
   - `appsettings.Production.json` for prod

3. **Use HTTPS in production**:
   - Set `RequireHttps: true`
   - Use HTTPS redirect URIs

4. **Validate issuers and audiences**:
   - Explicitly set `ValidIssuers` and `ValidAudiences`

### ❌ DON'T:

1. **Never commit secrets to source control**:
   - Don't put `ClientSecret` in `appsettings.json` that's committed
   - Use `.gitignore` for sensitive config files

2. **Don't use hardcoded values in production**:
   - Always use configuration files or secure storage

3. **Don't use `common` tenant in production** (unless multi-tenant is required):
   - Use specific tenant ID for better security

---

## Troubleshooting

### Issue: "Authority is required in configuration"

**Solution**: Ensure `WpIdpAuth:Authority` is set in your configuration file.

### Issue: "ClientId is required in configuration"

**Solution**: Ensure `WpIdpAuth:ClientId` is set in your configuration file.

### Issue: Redirect URI mismatch

**Solution**: Ensure the `RedirectUri` in your configuration exactly matches what's configured in Azure Portal (case-sensitive).

### Issue: Invalid client secret

**Solution**: 
- Verify the secret hasn't expired
- Create a new secret in Azure Portal if needed
- Ensure you copied the secret value (not the secret ID)

---

## Quick Reference

### .NET Core Configuration Location
- **File**: `appsettings.json` or `appsettings.{Environment}.json`
- **Section**: `WpIdpAuth`
- **Usage**: `builder.Services.AddWpIdpAuth(builder.Configuration);`

### .NET Framework Configuration Location
- **File**: `Startup.cs`
- **Method**: `Configuration(IAppBuilder app)`
- **Usage**: `app.UseWpIdpAuth(new WpIdpOptions { ... });`

---

**Need Help?** Check the main README.md or TECHNICAL_DOCUMENTATION.md for more details.

