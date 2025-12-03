using System.Collections.Generic;

namespace WP.Idp.Auth.SharedModels
{
    /// <summary>
    /// Result of token validation operation.
    /// </summary>
    public class TokenValidationResult
    {
        /// <summary>
        /// Indicates whether the token is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// The subject (user identifier) from the token claims.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// The tenant ID from the token claims.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// The email address from the token claims.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Additional claims from the token.
        /// </summary>
        public Dictionary<string, string> AdditionalClaims { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Error message if validation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

