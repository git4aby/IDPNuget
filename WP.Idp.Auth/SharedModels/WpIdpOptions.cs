using System.Collections.Generic;

namespace WP.Idp.Auth.SharedModels
{
    /// <summary>
    /// Configuration options for WP IDP authentication.
    /// </summary>
    public class WpIdpOptions
    {
        /// <summary>
        /// The authority URL (e.g., https://login.microsoftonline.com/{tenantId} or Auth0 domain).
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// The client ID (application ID).
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// The client secret (optional, required for some flows).
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// The redirect URI after authentication.
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// The scope(s) to request (space-separated).
        /// </summary>
        public string Scope { get; set; } = "openid profile email";

        /// <summary>
        /// The response type (default: "code id_token").
        /// </summary>
        public string ResponseType { get; set; } = "code id_token";

        /// <summary>
        /// Whether to require HTTPS (default: true).
        /// </summary>
        public bool RequireHttps { get; set; } = true;

        /// <summary>
        /// The metadata address (optional, auto-discovered if not provided).
        /// </summary>
        public string? MetadataAddress { get; set; }

        /// <summary>
        /// Valid audiences for token validation.
        /// </summary>
        public List<string> ValidAudiences { get; set; } = new List<string>();

        /// <summary>
        /// Valid issuers for token validation.
        /// </summary>
        public List<string> ValidIssuers { get; set; } = new List<string>();
    }
}

