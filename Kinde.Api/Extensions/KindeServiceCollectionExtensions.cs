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

            // Register HttpClient with proper lifetime management
            services.TryAddSingleton<HttpClient>(sp =>
            {
                return new KindeHttpClient();
            });

            // Register KindeClient as singleton
            services.TryAddSingleton<IKindeClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<KindeClientOptions>>().Value;
                var httpClient = sp.GetRequiredService<HttpClient>();

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
