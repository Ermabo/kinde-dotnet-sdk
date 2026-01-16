namespace Kinde.Api.Models.Configuration
{
    /// <summary>
    /// Configuration options for KindeClient when using dependency injection
    /// </summary>
    public class KindeClientOptions
    {
        /// <summary>
        /// The Kinde domain for your application (e.g., "https://yourapp.kinde.com")
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// The reply/callback URL for OAuth flows
        /// </summary>
        public string ReplyUrl { get; set; } = string.Empty;

        /// <summary>
        /// The logout redirect URL
        /// </summary>
        public string LogoutUrl { get; set; } = string.Empty;

        /// <summary>
        /// Force API mode for the Auth helper
        /// </summary>
        public bool ForceApi { get; set; } = false;

        /// <summary>
        /// Client ID for machine-to-machine authentication (optional)
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client secret for machine-to-machine authentication (optional)
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Audience/API identifier for machine-to-machine authentication (optional)
        /// </summary>
        public string? Audience { get; set; }
    }
}
