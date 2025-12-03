#if NET6_0_OR_GREATER
using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using WP.Idp.Auth.Abstractions;
using WP.Idp.Auth.SharedModels;

namespace WP.Idp.Auth.CoreAdapter
{
    /// <summary>
    /// Extension methods for configuring WP IDP authentication in .NET Core/6/8/9 applications.
    /// </summary>
    public static class CoreAuthExtensions
    {
        /// <summary>
        /// Adds WP IDP authentication services to the service collection using configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration containing IDP settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddWpIdpAuth(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Bind configuration section
            var options = new WpIdpOptions();
            configuration.GetSection("WpIdpAuth").Bind(options);
            
            // Validate required options
            if (string.IsNullOrWhiteSpace(options.Authority))
                throw new InvalidOperationException("WpIdpAuth:Authority is required in configuration.");
            if (string.IsNullOrWhiteSpace(options.ClientId))
                throw new InvalidOperationException("WpIdpAuth:ClientId is required in configuration.");

            return services.AddWpIdpAuth(options);
        }

        /// <summary>
        /// Adds WP IDP authentication services to the service collection using options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The IDP authentication options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddWpIdpAuth(this IServiceCollection services, WpIdpOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Register options
            services.AddSingleton(options);

            // Register authenticator
            services.AddSingleton<IIdpAuthenticator, CoreIdpAuthenticator>();

            // Configure Microsoft Identity Web authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(msOptions =>
                {
                    msOptions.Instance = ExtractInstance(options.Authority);
                    msOptions.Domain = ExtractDomain(options.Authority);
                    msOptions.TenantId = ExtractTenantId(options.Authority);
                    msOptions.ClientId = options.ClientId;
                    msOptions.ClientSecret = options.ClientSecret;
                    msOptions.CallbackPath = new PathString(ExtractCallbackPath(options.RedirectUri));
                })
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            // Configure JWT Bearer options
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
            {
                jwtOptions.Authority = options.Authority;
                jwtOptions.Audience = options.ClientId;
                jwtOptions.RequireHttpsMetadata = options.RequireHttps;
                
                if (!string.IsNullOrWhiteSpace(options.MetadataAddress))
                {
                    jwtOptions.MetadataAddress = options.MetadataAddress;
                }

                if (options.ValidAudiences.Any())
                {
                    jwtOptions.TokenValidationParameters.ValidAudiences = options.ValidAudiences;
                }

                if (options.ValidIssuers.Any())
                {
                    jwtOptions.TokenValidationParameters.ValidIssuers = options.ValidIssuers;
                }
            });

            return services;
        }

        private static string? ExtractInstance(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
                return null;

            try
            {
                var uri = new Uri(authority);
                return $"{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return null;
            }
        }

        private static string? ExtractDomain(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
                return null;

            try
            {
                var uri = new Uri(authority);
                return uri.Host;
            }
            catch
            {
                return null;
            }
        }

        private static string? ExtractTenantId(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
                return null;

            // Try to extract tenant ID from Azure AD authority format
            // e.g., https://login.microsoftonline.com/{tenantId}
            var parts = authority.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && parts[parts.Length - 1] != "v2.0")
            {
                return parts[parts.Length - 1];
            }

            return "common"; // Default to common tenant
        }

        private static string ExtractCallbackPath(string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(redirectUri))
                return "/signin-oidc";

            try
            {
                var uri = new Uri(redirectUri);
                return uri.AbsolutePath;
            }
            catch
            {
                return "/signin-oidc";
            }
        }
    }
}
#endif

