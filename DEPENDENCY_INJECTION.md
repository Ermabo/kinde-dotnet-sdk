# Using Kinde SDK with Dependency Injection

This guide explains how to use the Kinde .NET SDK with ASP.NET Core's built-in dependency injection container.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Configuration](#configuration)
  - [Using appsettings.json](#using-appsettingsjson)
  - [Using Environment Variables](#using-environment-variables)
- [Service Registration](#service-registration)
  - [Singleton (Recommended for M2M)](#singleton-recommended-for-m2m)
  - [Scoped (Recommended for Web Apps)](#scoped-recommended-for-web-apps)
  - [Transient](#transient)
- [Usage Examples](#usage-examples)
  - [Machine-to-Machine (M2M) Applications](#machine-to-machine-m2m-applications)
  - [Web Applications](#web-applications)
  - [API Controllers](#api-controllers)
  - [Background Services](#background-services)
- [Choosing the Right Lifetime](#choosing-the-right-lifetime)
- [Advanced Scenarios](#advanced-scenarios)

## Overview

The Kinde SDK supports dependency injection through extension methods that register `KindeClient` with ASP.NET Core's service container. This approach provides several benefits:

- **Better testability**: Easy to mock `IKindeClient` in unit tests
- **Lifecycle management**: Framework handles creation and disposal
- **Configuration management**: Integrates with .NET configuration system
- **Best practices**: Follows .NET dependency injection patterns

## Installation

Install the Kinde SDK via NuGet:

```bash
dotnet add package Kinde.SDK
```

## Configuration

### Using appsettings.json

Add Kinde configuration to your `appsettings.json`:

```json
{
  "Kinde": {
    "Domain": "https://yourapp.kinde.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Audience": "https://yourapp.kinde.com/api",
    "ReplyUrl": "https://localhost:5001/callback",
    "LogoutUrl": "https://localhost:5001"
  }
}
```

### Using Environment Variables

You can also configure via environment variables:

```bash
export Kinde__Domain="https://yourapp.kinde.com"
export Kinde__ClientId="your-client-id"
export Kinde__ClientSecret="your-client-secret"
export Kinde__Audience="https://yourapp.kinde.com/api"
```

## Service Registration

### Singleton (Recommended for M2M)

Use singleton lifetime for machine-to-machine applications where you want a single shared instance:

```csharp
using Kinde.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register KindeClient as Singleton
builder.Services.AddKindeClient(options =>
{
    options.Domain = builder.Configuration["Kinde:Domain"]!;
    options.ClientId = builder.Configuration["Kinde:ClientId"];
    options.ClientSecret = builder.Configuration["Kinde:ClientSecret"];
    options.Audience = builder.Configuration["Kinde:Audience"];
});

var app = builder.Build();
```

**When to use Singleton:**
- Machine-to-machine (M2M) applications using client credentials
- Background services that need consistent authentication
- API-to-API communication
- Single-tenant applications

### Scoped (Recommended for Web Apps)

Use scoped lifetime for web applications where each HTTP request should have its own client instance:

```csharp
using Kinde.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register KindeClient as Scoped
builder.Services.AddKindeClientScoped(options =>
{
    options.Domain = builder.Configuration["Kinde:Domain"]!;
    options.ReplyUrl = builder.Configuration["Kinde:ReplyUrl"]!;
    options.LogoutUrl = builder.Configuration["Kinde:LogoutUrl"]!;
});

var app = builder.Build();
```

**When to use Scoped:**
- Web applications with user authentication
- Multi-user/multi-tenant applications
- When using authorization code flow
- When each user session needs isolated state

### Transient

Use transient lifetime when you need a new instance for every injection:

```csharp
using Kinde.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register KindeClient as Transient
builder.Services.AddKindeClientTransient(options =>
{
    options.Domain = builder.Configuration["Kinde:Domain"]!;
    options.ReplyUrl = builder.Configuration["Kinde:ReplyUrl"]!;
    options.LogoutUrl = builder.Configuration["Kinde:LogoutUrl"]!;
});

var app = builder.Build();
```

**When to use Transient:**
- When you need complete isolation between operations
- Short-lived operations that require independent state
- Testing scenarios where you want fresh instances

## Usage Examples

### Machine-to-Machine (M2M) Applications

For M2M applications using client credentials flow:

```csharp
// Program.cs
using Kinde.Api.Extensions;
using Kinde.Api.Client;
using Kinde.Api.Models.Configuration;
using Kinde.Api.Api;

var builder = WebApplication.CreateBuilder(args);

// Register as Singleton for M2M
builder.Services.AddKindeClient(options =>
{
    options.Domain = builder.Configuration["Kinde:Domain"]!;
    options.ClientId = builder.Configuration["Kinde:ClientId"];
    options.ClientSecret = builder.Configuration["Kinde:ClientSecret"];
    options.Audience = builder.Configuration["Kinde:Audience"];
});

var app = builder.Build();

app.MapGet("/users", async (IKindeClient kindeClient) =>
{
    // Authorize using client credentials
    await kindeClient.Authorize(new ClientCredentialsConfiguration(
        kindeClient.IdentityProviderConfiguration.Domain,
        builder.Configuration["Kinde:ClientId"]!,
        builder.Configuration["Kinde:ClientSecret"]!,
        builder.Configuration["Kinde:Audience"]!
    ));

    // Use the management API
    var usersApi = new UsersApi(kindeClient);
    var users = await usersApi.GetUsersAsync();
    
    return Results.Ok(users.Users?.Select(u => new 
    { 
        u.Id, 
        u.Email, 
        Name = $"{u.FirstName} {u.LastName}" 
    }));
});

app.Run();
```

### Web Applications

For web applications with user authentication:

```csharp
// Program.cs
using Kinde.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register as Scoped for web apps
builder.Services.AddKindeClientScoped(options =>
{
    options.Domain = builder.Configuration["Kinde:Domain"]!;
    options.ReplyUrl = builder.Configuration["Kinde:ReplyUrl"]!;
    options.LogoutUrl = builder.Configuration["Kinde:LogoutUrl"]!;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### API Controllers

Using KindeClient in API controllers:

```csharp
using Kinde.Api.Client;
using Kinde.Api.Api;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IKindeClient _kindeClient;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(IKindeClient kindeClient, ILogger<OrganizationsController> logger)
    {
        _kindeClient = kindeClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrganizations()
    {
        try
        {
            if (!_kindeClient.IsAuthenticated)
            {
                return Unauthorized("Client not authenticated");
            }

            var orgApi = new OrganizationsApi(_kindeClient);
            var organizations = await orgApi.GetOrganizationsAsync();
            
            return Ok(organizations.Organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching organizations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] string name)
    {
        try
        {
            var orgApi = new OrganizationsApi(_kindeClient);
            var newOrg = await orgApi.CreateOrganizationAsync(
                new Kinde.Api.Model.CreateOrganizationRequest(name)
            );
            
            return CreatedAtAction(nameof(GetOrganizations), new { id = newOrg.Organization.Code }, newOrg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### Background Services

Using KindeClient in background services:

```csharp
using Kinde.Api.Client;
using Kinde.Api.Models.Configuration;
using Kinde.Api.Api;

public class UserSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserSyncService> _logger;

    public UserSyncService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<UserSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a scope to resolve scoped services
                using var scope = _serviceProvider.CreateScope();
                var kindeClient = scope.ServiceProvider.GetRequiredService<IKindeClient>();

                // Authorize if needed
                if (!kindeClient.IsAuthenticated)
                {
                    await kindeClient.Authorize(new ClientCredentialsConfiguration(
                        _configuration["Kinde:Domain"]!,
                        _configuration["Kinde:ClientId"]!,
                        _configuration["Kinde:ClientSecret"]!,
                        _configuration["Kinde:Audience"]!
                    ));
                }

                // Perform sync operation
                var usersApi = new UsersApi(kindeClient);
                var users = await usersApi.GetUsersAsync();
                
                _logger.LogInformation("Synced {Count} users", users.Users?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing users");
            }

            // Wait 1 hour before next sync
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}

// Register the service in Program.cs
builder.Services.AddHostedService<UserSyncService>();
```

## Choosing the Right Lifetime

| Lifetime | Use Case | Pros | Cons |
|----------|----------|------|------|
| **Singleton** | M2M apps, API clients | Memory efficient, consistent state | Shared state across requests |
| **Scoped** | Web apps, user auth | Isolated per request, safe for multi-user | More memory usage |
| **Transient** | Complete isolation needed | Fresh instance every time | Most memory usage, overhead |

### Resource Management

The `KindeClient` class implements `IDisposable` (through `ApiClient`), which means:
- **Scoped and Transient**: The DI container automatically disposes instances at the end of their lifetime, properly cleaning up HttpClient resources
- **Singleton**: The instance lives for the application lifetime, which is appropriate for long-running services
- HttpClient instances are automatically disposed when their parent KindeClient is disposed

For typical usage scenarios, the default behavior provides proper resource management. For high-traffic production environments, consider using the singleton lifetime when possible to minimize resource allocation.

## Advanced Scenarios

### Custom HttpClient Configuration

If you need custom HttpClient configuration, you can create a custom factory. Note that `KindeClient` implements `IDisposable`, so the DI container will handle cleanup:

```csharp
// For singleton with custom configuration
builder.Services.AddSingleton<IKindeClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    
    // Create custom HttpClient (will be disposed when KindeClient is disposed)
    var httpClient = new KindeHttpClient();
    
    var appConfig = new ApplicationConfiguration(
        config["Kinde:Domain"]!,
        config["Kinde:ReplyUrl"]!,
        config["Kinde:LogoutUrl"]!
    );
    
    return new KindeClient(appConfig, httpClient);
});

// For scoped with automatic disposal
builder.Services.AddScoped<IKindeClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClient = new KindeHttpClient();
    
    var appConfig = new ApplicationConfiguration(
        config["Kinde:Domain"]!,
        config["Kinde:ReplyUrl"]!,
        config["Kinde:LogoutUrl"]!
    );
    
    // The DI container will dispose this at the end of the scope
    return new KindeClient(appConfig, httpClient);
});
```

### Using IHttpClientFactory (Advanced)

For very high-traffic scenarios where you want more control over HttpClient lifecycle, you can integrate with `IHttpClientFactory`. However, note that `KindeHttpClient` has specific configuration, so this approach requires careful consideration:

```csharp
// Register named HttpClient
builder.Services.AddHttpClient("KindeClient", client =>
{
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PostmanRuntime/7.29.2");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler 
{ 
    AllowAutoRedirect = false 
});

// Use the factory to create KindeClient
builder.Services.AddScoped<IKindeClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    
    // Get HttpClient from factory
    var httpClient = httpClientFactory.CreateClient("KindeClient");
    
    var appConfig = new ApplicationConfiguration(
        config["Kinde:Domain"]!,
        config["Kinde:ReplyUrl"]!,
        config["Kinde:LogoutUrl"]!
    );
    
    // Note: When using HttpClientFactory, the HttpClient disposal is managed by the factory
    return new KindeClient(appConfig, httpClient);
});
```

### Multiple Kinde Instances

For scenarios where you need multiple Kinde instances (e.g., multi-tenant):

```csharp
// Register with keyed services (.NET 8+)
builder.Services.AddKeyedSingleton<IKindeClient>("tenant1", (sp, key) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var appConfig = new ApplicationConfiguration(
        config["Kinde:Tenant1:Domain"]!,
        config["Kinde:Tenant1:ReplyUrl"]!,
        config["Kinde:Tenant1:LogoutUrl"]!
    );
    return new KindeClient(appConfig, new KindeHttpClient());
});

builder.Services.AddKeyedSingleton<IKindeClient>("tenant2", (sp, key) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var appConfig = new ApplicationConfiguration(
        config["Kinde:Tenant2:Domain"]!,
        config["Kinde:Tenant2:ReplyUrl"]!,
        config["Kinde:Tenant2:LogoutUrl"]!
    );
    return new KindeClient(appConfig, new KindeHttpClient());
});

// Usage in controller
public class MyController : ControllerBase
{
    private readonly IKindeClient _tenant1Client;
    private readonly IKindeClient _tenant2Client;

    public MyController(
        [FromKeyedServices("tenant1")] IKindeClient tenant1Client,
        [FromKeyedServices("tenant2")] IKindeClient tenant2Client)
    {
        _tenant1Client = tenant1Client;
        _tenant2Client = tenant2Client;
    }
}
```

### Testing with Dependency Injection

Example of mocking `IKindeClient` for unit tests:

```csharp
using Moq;
using Kinde.Api.Client;
using Kinde.Api.Models.User;

public class OrganizationsControllerTests
{
    [Fact]
    public async Task GetOrganizations_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var mockKindeClient = new Mock<IKindeClient>();
        mockKindeClient.Setup(x => x.IsAuthenticated).Returns(true);
        
        var controller = new OrganizationsController(
            mockKindeClient.Object,
            Mock.Of<ILogger<OrganizationsController>>()
        );

        // Act
        var result = await controller.GetOrganizations();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
```

## Summary

The Kinde .NET SDK provides flexible dependency injection support through extension methods:

- **`AddKindeClient()`** - Singleton lifetime for M2M applications
- **`AddKindeClientScoped()`** - Scoped lifetime for web applications
- **`AddKindeClientTransient()`** - Transient lifetime for complete isolation

Choose the appropriate lifetime based on your application's needs and follow standard .NET dependency injection patterns for best results.

For more information, see the [Kinde documentation](https://kinde.com/docs/developer-tools/dotnet-sdk/).
