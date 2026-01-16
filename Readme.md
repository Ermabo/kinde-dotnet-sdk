# Kinde .NET SDK

The Kinde SDK for .NET.

You can also use the .NET starter kit [here](https://github.com/kinde-starter-kits/dotnet-starter-kit).

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](https://makeapullrequest.com) [![Kinde Docs](https://img.shields.io/badge/Kinde-Docs-eee?style=flat-square)](https://kinde.com/docs/developer-tools) [![Kinde Community](https://img.shields.io/badge/Kinde-Community-eee?style=flat-square)](https://thekindecommunity.slack.com)

## Documentation

For details on integrating this SDK into your project, head over to the [Kinde docs](https://kinde.com/docs/) and see the [.NET SDK](https://kinde.com/docs/developer-tools/dotnet-sdk/) doc 👍🏼.

### Using with Dependency Injection

The Kinde SDK supports ASP.NET Core dependency injection. See the [Dependency Injection Guide](DEPENDENCY_INJECTION.md) for comprehensive examples on:

- Registering KindeClient as a service (Singleton, Scoped, or Transient)
- Configuration with appsettings.json
- Usage in controllers and services
- Machine-to-machine (M2M) applications
- Web applications with user authentication
- Testing with mocks

**Quick Example:**

```csharp
// Program.cs
using Kinde.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// For M2M applications - Singleton
builder.Services.AddKindeClient(options =>
{
    options.Domain = "https://yourapp.kinde.com";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.Audience = "https://yourapp.kinde.com/api";
});

// OR for web applications - Scoped
builder.Services.AddKindeClientScoped(options =>
{
    options.Domain = "https://yourapp.kinde.com";
    options.ReplyUrl = "https://myapp.com/callback";
    options.LogoutUrl = "https://myapp.com";
});

var app = builder.Build();

// Use in endpoints or controllers via constructor injection
app.MapGet("/users", async (IKindeClient kindeClient) =>
{
    // Your code here
});
```

## Publishing

The core team handles publishing.

## Contributing

Please refer to Kinde’s [contributing guidelines](https://github.com/kinde-oss/.github/blob/489e2ca9c3307c2b2e098a885e22f2239116394a/CONTRIBUTING.md).

## License

By contributing to Kinde, you agree that your contributions will be licensed under its MIT License.
