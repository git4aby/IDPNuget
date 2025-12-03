using System.Threading.Tasks;
using WP.Idp.Auth.SharedModels;

namespace WP.Idp.Auth.Abstractions
{
    /// <summary>
    /// Interface for IDP authentication operations that must be implemented by both Core and Framework adapters.
    /// </summary>
    public interface IIdpAuthenticator
    {
        /// <summary>
        /// Validates an IDP token asynchronously.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>A TokenValidationResult containing validation status and claims.</returns>
        Task<TokenValidationResult> ValidateTokenAsync(string token);

        /// <summary>
        /// Gets the authorization URL for initiating the OAuth/OpenID Connect flow.
        /// </summary>
        /// <returns>The authorization URL.</returns>
        string GetAuthorizeUrl();
    }
}

