using Kinde.Api.Enums;
using Kinde.Api.Models.Configuration;
using Kinde.Api.Models.Tokens;
using Kinde.Api.Models.User;
using Kinde.Api.Model;

namespace Kinde.Api.Client
{
    /// <summary>
    /// Interface for Kinde Client to enable dependency injection
    /// </summary>
    public interface IKindeClient
    {
        /// <summary>
        /// Returns the profile for the current user	
        /// </summary>
        KindeSSOUser User { get; }

        /// <summary>
        /// Returns the current authorization state of user
        /// </summary>
        AuthorizationStates AuthorizationState { get; }

        /// <summary>
        /// Returns the raw full Oauth token after logged from Kinde
        /// </summary>
        OauthToken Token { get; }

        /// <summary>
        /// To check user authenticated or not	
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Returns the current identity provider configuration
        /// </summary>
        IApplicationConfiguration IdentityProviderConfiguration { get; set; }

        /// <summary>
        /// Returns the authentication helper for permissions, roles, feature flags, and entitlements
        /// </summary>
        Kinde.Api.Auth.Auth Auth { get; }

        /// <summary>
        /// Constructs redirect url and sends user to Kinde to sign in	
        /// </summary>
        /// <param name="authorizationConfiguration">authorization configurations.</param>
        Task Authorize(IAuthorizationConfiguration authorizationConfiguration);

        /// <summary>
        /// Constructs redirect url and sends user to Kinde to sign up	
        /// </summary>
        /// <param name="authorizationConfiguration">authorization configurations.</param>
        Task Register(IAuthorizationConfiguration authorizationConfiguration);

        /// <summary>
        /// Constructs redirect url for Kinde user to sign in	
        /// </summary>
        /// <param name="state">The session id for instance to persist users authentication.</param>
        Task<string> GetRedirectionUrl(string state);

        /// <summary>
        /// Returns the raw Access token from URL after logged from Kinde		
        /// </summary>
        Task<string> GetToken();

        /// <summary>
        /// Logs the user out of Kinde
        /// </summary>
        Task<string> Logout();

        /// <summary>
        /// Trying to get a new token using refresh_token	
        /// </summary>
        Task Renew();

        /// <summary>
        /// It returns user's information after successful authentication
        /// </summary>
        /// <remarks>
        /// Contains the id, given_name, family_name, email and picture of the currently logged in user. 
        /// </remarks>
        /// <returns>KindeUserDetail</returns>
        KindeUserDetail GetUserDetails();

        /// <summary>
        /// Get a claim from a token.
        /// </summary>
        /// <param name="key">The name of the claim.</param>
        /// <param name="tokenType">The token type to check: "access_token", "id_token"</param>
        /// <returns>KindeClaim</returns>
        KindeClaim? GetClaim(string key, string tokenType = "access_token");

        /// <summary>
        /// Returns all permissions for the current user for the organization they are logged into	
        /// </summary>
        /// <returns>OrganizationPermissionsCollection</returns>
        OrganizationPermissionsCollection? GetPermissions();

        /// <summary>
        /// Given a permission value, returns if it is granted or not (checks if permission key exists in the permissions claim array)
        /// And relevant org code (checking against claim org_code)
        /// </summary>
        /// <param name="key">The permission key.</param>
        /// <returns>OrganizationPermissionsCollection</returns>
        OrganizationPermission? GetPermission(string key);

        /// <summary>
        /// Get details for the organization your user is logged into	
        /// </summary>
        /// <returns>The response is a orgCode</returns>
        string? GetOrganization();

        /// <summary>
        /// Gets an array of all organizations the user has access to	
        /// </summary>
        /// <returns>The response is all orgCodes</returns>
        string[]? GetOrganizations();

        /// <summary>
        /// Get a flag from the feature_flags claim of the access_token.
        /// </summary>
        /// <param name="code">The name of the flag.</param>
        /// <param name="defaultValue">A fall back value if the flag isn't found.</param>
        /// <param name="flagType">The data type of the flag: "s", "b", "i"</param>
        /// <returns>FeatureFlag</returns>
        FeatureFlag GetFlag(string code, FeatureFlagValue? defaultValue = null, string? flagType = null);

        /// <summary>
        /// Get a boolean flag from the feature_flags claim of the access_token.
        /// </summary>
        /// <param name="code">The name of the flag.</param>
        /// <param name="defaultValue">A fall back value if the flag isn't found.</param>
        /// <returns>flag as bool value</returns>
        bool? GetBooleanFlag(string code, bool? defaultValue = null);

        /// <summary>
        /// Get a string flag from the feature_flags claim of the access_token.
        /// </summary>
        /// <param name="code">The name of the flag.</param>
        /// <param name="defaultValue">A fall back value if the flag isn't found.</param>
        /// <returns>flag as string value</returns>
        string GetStringFlag(string code, string? defaultValue = null);

        /// <summary>
        /// Get a int flag from the feature_flags claim of the access_token.
        /// </summary>
        /// <param name="code">The name of the flag.</param>
        /// <param name="defaultValue">A fall back value if the flag isn't found.</param>
        /// <returns>flag as int value</returns>
        int? GetIntegerFlag(string code, int? defaultValue = null);

        /// <summary>
        /// Generates a URL to the user profile portal
        /// </summary>
        /// <param name="options">Configuration options</param>
        /// <returns>Object containing the URL to redirect to</returns>
        Task<GetPortalLink> GenerateProfileUrl(GenerateProfileUrlOptions options);
    }
}
