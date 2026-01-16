using Kinde.Api.Client;
using Kinde.Api.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kinde.Api.Extensions
{
    /// <summary>
    /// Extension methods for registering Kinde services with dependency injection
    /// </summary>
    public static class KindeServiceCollectionExtensions
    {
        /// <summary>
        /// Adds KindeClient as a singleton service to the service collection.
        /// Recommended for machine-to-machine (M2M) applications using client credentials flow.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure KindeClientOptions</param>
        /// <returns>The service collection for chaining</returns>
        /// <example>
        /// <code>
        /// services.AddKindeClient(options =>
        /// {
        ///     options.Domain = "https://yourapp.kinde.com";
        ///     options.ClientId = "your-client-id";
        ///     options.ClientSecret = "your-client-secret";
        ///     options.Audience = "https://yourapp.kinde.com/api";
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddKindeClient(
            this IServiceCollection services,
            Action<KindeClientOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            // Configure options
            services.Configure(configureOptions);

            // Register KindeClient as singleton
            // Note: We create a single KindeHttpClient instance for the singleton
            // For production use with high traffic, consider using IHttpClientFactory
            services.TryAddSingleton<IKindeClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<KindeClientOptions>>().Value;
                var httpClient = new KindeHttpClient();

                var appConfig = new ApplicationConfiguration(
                    options.Domain,
                    options.ReplyUrl,
                    options.LogoutUrl,
                    options.ForceApi
                );

                return new KindeClient(appConfig, httpClient);
            });

            // Also register as concrete type for backwards compatibility
            services.TryAddSingleton(sp => (KindeClient)sp.GetRequiredService<IKindeClient>());

            return services;
        }

        /// <summary>
        /// Adds KindeClient as a scoped service to the service collection.
        /// Recommended for web applications using authorization code flow where each user session needs its own client instance.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure KindeClientOptions</param>
        /// <returns>The service collection for chaining</returns>
        /// <remarks>
        /// Note: This creates a new HttpClient for each scope. For high-traffic applications,
        /// consider implementing IHttpClientFactory pattern or using singleton lifetime if appropriate.
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddKindeClientScoped(options =>
        /// {
        ///     options.Domain = "https://yourapp.kinde.com";
        ///     options.ReplyUrl = "https://myapp.com/callback";
        ///     options.LogoutUrl = "https://myapp.com";
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddKindeClientScoped(
            this IServiceCollection services,
            Action<KindeClientOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            // Configure options
            services.Configure(configureOptions);

            // Register KindeClient as scoped (new instance per request)
            services.TryAddScoped<IKindeClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<KindeClientOptions>>().Value;
                
                // Create a new HttpClient for each scope
                // Note: For production high-traffic scenarios, consider using IHttpClientFactory
                var httpClient = new KindeHttpClient();

                var appConfig = new ApplicationConfiguration(
                    options.Domain,
                    options.ReplyUrl,
                    options.LogoutUrl,
                    options.ForceApi
                );

                return new KindeClient(appConfig, httpClient);
            });

            // Also register as concrete type for backwards compatibility
            services.TryAddScoped(sp => (KindeClient)sp.GetRequiredService<IKindeClient>());

            return services;
        }

        /// <summary>
        /// Adds KindeClient as a transient service to the service collection.
        /// Creates a new instance every time it's requested. Use this when you need complete isolation between operations.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure KindeClientOptions</param>
        /// <returns>The service collection for chaining</returns>
        /// <remarks>
        /// Note: This creates a new HttpClient for each instance. This should be used sparingly
        /// in high-traffic applications. Consider singleton or scoped lifetime when possible.
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddKindeClientTransient(options =>
        /// {
        ///     options.Domain = "https://yourapp.kinde.com";
        ///     options.ReplyUrl = "https://myapp.com/callback";
        ///     options.LogoutUrl = "https://myapp.com";
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddKindeClientTransient(
            this IServiceCollection services,
            Action<KindeClientOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            // Configure options
            services.Configure(configureOptions);

            // Register KindeClient as transient (new instance every time)
            services.TryAddTransient<IKindeClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<KindeClientOptions>>().Value;
                
                // Create a new HttpClient for each instance
                // Note: Transient instances should be used carefully to avoid socket exhaustion
                var httpClient = new KindeHttpClient();

                var appConfig = new ApplicationConfiguration(
                    options.Domain,
                    options.ReplyUrl,
                    options.LogoutUrl,
                    options.ForceApi
                );

                return new KindeClient(appConfig, httpClient);
            });

            // Also register as concrete type for backwards compatibility
            services.TryAddTransient(sp => (KindeClient)sp.GetRequiredService<IKindeClient>());

            return services;
        }
    }
}
