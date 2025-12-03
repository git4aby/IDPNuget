#if !NET6_0_OR_GREATER
using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using WP.Idp.Auth.Abstractions;
using WP.Idp.Auth.FrameworkAdapter;
using WP.Idp.Auth.SharedModels;

namespace WP.Idp.Auth.FrameworkAdapter
{
    /// <summary>
    /// Extension methods for configuring WP IDP authentication in .NET Framework 4.x applications using OWIN.
    /// </summary>
    public static class FrameworkAuthExtensions
    {
        /// <summary>
        /// Configures WP IDP authentication middleware for OWIN pipeline.
        /// </summary>
        /// <param name="app">The OWIN application builder.</param>
        /// <param name="options">The IDP authentication options.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IAppBuilder UseWpIdpAuth(this IAppBuilder app, WpIdpOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Validate required options
            if (string.IsNullOrWhiteSpace(options.Authority))
                throw new InvalidOperationException("Authority is required.");
            if (string.IsNullOrWhiteSpace(options.ClientId))
                throw new InvalidOperationException("ClientId is required.");
            if (string.IsNullOrWhiteSpace(options.RedirectUri))
                throw new InvalidOperationException("RedirectUri is required.");

            // Configure cookie authentication (for sign-in)
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                CookieName = "WpIdpAuth.Cookie",
                ExpireTimeSpan = TimeSpan.FromHours(8),
                SlidingExpiration = true
            });

            // Configure OpenID Connect authentication
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = options.ClientId,
                Authority = options.Authority,
                RedirectUri = options.RedirectUri,
                PostLogoutRedirectUri = options.RedirectUri,
                ResponseType = options.ResponseType,
                Scope = options.Scope,
                RequireHttpsMetadata = options.RequireHttps,
                MetadataAddress = options.MetadataAddress,
                
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect($"/Error?message={Uri.EscapeDataString(context.Exception.Message)}");
                        return System.Threading.Tasks.Task.FromResult(0);
                    },
                    SecurityTokenValidated = context =>
                    {
                        // Additional token validation can be performed here
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                },
                
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = options.ValidIssuers != null && options.ValidIssuers.Count > 0 
                        ? options.ValidIssuers 
                        : new[] { options.Authority },
                    ValidateAudience = true,
                    ValidAudiences = options.ValidAudiences != null && options.ValidAudiences.Count > 0 
                        ? options.ValidAudiences 
                        : new[] { options.ClientId },
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                }
            });

            // Register authenticator in OWIN context (if needed)
            // Note: In OWIN, you typically access authentication through the context
            // This is a simplified registration for dependency injection scenarios

            return app;
        }
    }
}
#endif

