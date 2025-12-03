#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WP.Idp.Auth.Abstractions;
using WP.Idp.Auth.SharedModels;

namespace WP.Idp.Auth.CoreAdapter
{
    /// <summary>
    /// .NET Core/6/8/9 implementation of IIdpAuthenticator using Microsoft.Identity.Web.
    /// </summary>
    public class CoreIdpAuthenticator : IIdpAuthenticator
    {
        private readonly WpIdpOptions _options;
        private readonly TokenValidationParameters _validationParameters;

        public CoreIdpAuthenticator(WpIdpOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _validationParameters = CreateValidationParameters();
        }

        public async Task<SharedModels.TokenValidationResult> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new SharedModels.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token is null or empty"
                };
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                
                if (!handler.CanReadToken(token))
                {
                    return new SharedModels.TokenValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token cannot be read"
                    };
                }

                var principal = handler.ValidateToken(token, _validationParameters, out SecurityToken validatedToken);
                
                var result = new SharedModels.TokenValidationResult
                {
                    IsValid = true
                };

                // Extract claims
                result.Subject = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? principal.FindFirst("sub")?.Value 
                    ?? principal.FindFirst("oid")?.Value;
                
                result.TenantId = principal.FindFirst("tid")?.Value 
                    ?? principal.FindFirst("tenant_id")?.Value;
                
                result.Email = principal.FindFirst(ClaimTypes.Email)?.Value 
                    ?? principal.FindFirst("email")?.Value 
                    ?? principal.FindFirst("preferred_username")?.Value;

                // Extract additional claims
                foreach (var claim in principal.Claims)
                {
                    if (!result.AdditionalClaims.ContainsKey(claim.Type))
                    {
                        result.AdditionalClaims[claim.Type] = claim.Value;
                    }
                }

                return await Task.FromResult(result);
            }
            catch (SecurityTokenValidationException ex)
            {
                return new SharedModels.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Token validation failed: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new SharedModels.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Error validating token: {ex.Message}"
                };
            }
        }

        public string GetAuthorizeUrl()
        {
            var baseUrl = _options.Authority.TrimEnd('/');
            var authorizeEndpoint = $"{baseUrl}/oauth2/v2.0/authorize";
            
            var queryParams = new List<string>
            {
                $"client_id={Uri.EscapeDataString(_options.ClientId)}",
                $"redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}",
                $"response_type={Uri.EscapeDataString(_options.ResponseType)}",
                $"scope={Uri.EscapeDataString(_options.Scope)}",
                "response_mode=form_post"
            };

            return $"{authorizeEndpoint}?{string.Join("&", queryParams)}";
        }

        private TokenValidationParameters CreateValidationParameters()
        {
            var validIssuers = _options.ValidIssuers.Any() 
                ? _options.ValidIssuers 
                : new List<string> { _options.Authority };

            var validAudiences = _options.ValidAudiences.Any() 
                ? _options.ValidAudiences 
                : new List<string> { _options.ClientId };

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = validIssuers,
                ValidateAudience = true,
                ValidAudiences = validAudiences,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        }
    }
}
#endif

