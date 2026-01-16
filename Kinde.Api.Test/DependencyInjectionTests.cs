using Kinde.Api.Client;
using Kinde.Api.Extensions;
using Kinde.Api.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kinde.Api.Test
{
    [Collection("Sequential")]
    public class DependencyInjectionTests
    {
        private const string TestDomain = "https://test.kinde.com";
        private const string TestReplyUrl = "https://localhost:5001/callback";
        private const string TestLogoutUrl = "https://localhost:5001";

        [Fact]
        public void AddKindeClient_RegistersSingletonServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var kindeClient1 = serviceProvider.GetRequiredService<IKindeClient>();
            var kindeClient2 = serviceProvider.GetRequiredService<IKindeClient>();

            Assert.NotNull(kindeClient1);
            Assert.NotNull(kindeClient2);
            Assert.Same(kindeClient1, kindeClient2); // Should be the same instance (singleton)
        }

        [Fact]
        public void AddKindeClient_ConfiguresOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            const string clientId = "test-client-id";
            const string clientSecret = "test-client-secret";
            const string audience = "https://test.kinde.com/api";

            // Act
            services.AddKindeClient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.Audience = audience;
                options.ForceApi = true;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var options = serviceProvider.GetRequiredService<IOptions<KindeClientOptions>>().Value;
            Assert.Equal(TestDomain, options.Domain);
            Assert.Equal(TestReplyUrl, options.ReplyUrl);
            Assert.Equal(TestLogoutUrl, options.LogoutUrl);
            Assert.Equal(clientId, options.ClientId);
            Assert.Equal(clientSecret, options.ClientSecret);
            Assert.Equal(audience, options.Audience);
            Assert.True(options.ForceApi);
        }

        [Fact]
        public void AddKindeClient_CreatesKindeClientWithCorrectConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var kindeClient = serviceProvider.GetRequiredService<IKindeClient>();
            Assert.NotNull(kindeClient.IdentityProviderConfiguration);
            Assert.Equal(TestDomain, kindeClient.IdentityProviderConfiguration.Domain);
            Assert.Equal(TestReplyUrl, kindeClient.IdentityProviderConfiguration.ReplyUrl);
            Assert.Equal(TestLogoutUrl, kindeClient.IdentityProviderConfiguration.LogoutUrl);
        }

        [Fact]
        public void AddKindeClient_RegistersBothInterfaceAndConcreteType()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var interfaceClient = serviceProvider.GetRequiredService<IKindeClient>();
            var concreteClient = serviceProvider.GetRequiredService<KindeClient>();

            Assert.NotNull(interfaceClient);
            Assert.NotNull(concreteClient);
            Assert.Same(interfaceClient, concreteClient);
        }

        [Fact]
        public void AddKindeClientScoped_RegistersScopedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClientScoped(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            using (var scope1 = serviceProvider.CreateScope())
            using (var scope2 = serviceProvider.CreateScope())
            {
                var kindeClient1a = scope1.ServiceProvider.GetRequiredService<IKindeClient>();
                var kindeClient1b = scope1.ServiceProvider.GetRequiredService<IKindeClient>();
                var kindeClient2 = scope2.ServiceProvider.GetRequiredService<IKindeClient>();

                Assert.NotNull(kindeClient1a);
                Assert.NotNull(kindeClient1b);
                Assert.NotNull(kindeClient2);

                // Within the same scope, should be the same instance
                Assert.Same(kindeClient1a, kindeClient1b);

                // Different scopes should have different instances
                Assert.NotSame(kindeClient1a, kindeClient2);
            }
        }

        [Fact]
        public void AddKindeClientTransient_RegistersTransientServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClientTransient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var kindeClient1 = serviceProvider.GetRequiredService<IKindeClient>();
            var kindeClient2 = serviceProvider.GetRequiredService<IKindeClient>();

            Assert.NotNull(kindeClient1);
            Assert.NotNull(kindeClient2);

            // Transient should create new instances
            Assert.NotSame(kindeClient1, kindeClient2);
        }

        [Fact]
        public void AddKindeClient_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Arrange
            ServiceCollection? services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services!.AddKindeClient(options => { }));
        }

        [Fact]
        public void AddKindeClient_ThrowsArgumentNullException_WhenConfigureOptionsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddKindeClient(null!));
        }

        [Fact]
        public void AddKindeClientScoped_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Arrange
            ServiceCollection? services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services!.AddKindeClientScoped(options => { }));
        }

        [Fact]
        public void AddKindeClientScoped_ThrowsArgumentNullException_WhenConfigureOptionsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddKindeClientScoped(null!));
        }

        [Fact]
        public void AddKindeClientTransient_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Arrange
            ServiceCollection? services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services!.AddKindeClientTransient(options => { }));
        }

        [Fact]
        public void AddKindeClientTransient_ThrowsArgumentNullException_WhenConfigureOptionsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddKindeClientTransient(null!));
        }

        [Fact]
        public void KindeClient_ImplementsIKindeClientInterface()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();
            var kindeClient = serviceProvider.GetRequiredService<IKindeClient>();

            // Assert
            Assert.IsAssignableFrom<IKindeClient>(kindeClient);
            Assert.IsType<KindeClient>(kindeClient);
        }

        [Fact]
        public void KindeClientOptions_DefaultValues()
        {
            // Arrange & Act
            var options = new KindeClientOptions();

            // Assert
            Assert.Equal(string.Empty, options.Domain);
            Assert.Equal(string.Empty, options.ReplyUrl);
            Assert.Equal(string.Empty, options.LogoutUrl);
            Assert.False(options.ForceApi);
            Assert.Null(options.ClientId);
            Assert.Null(options.ClientSecret);
            Assert.Null(options.Audience);
        }

        [Fact]
        public void KindeClient_AuthProperty_IsNotNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKindeClient(options =>
            {
                options.Domain = TestDomain;
                options.ReplyUrl = TestReplyUrl;
                options.LogoutUrl = TestLogoutUrl;
            });

            var serviceProvider = services.BuildServiceProvider();
            var kindeClient = serviceProvider.GetRequiredService<IKindeClient>();

            // Assert
            Assert.NotNull(kindeClient.Auth);
        }
    }
}
