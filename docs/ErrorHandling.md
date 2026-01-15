# Error Handling Guide

This guide explains how to handle errors when using the Kinde .NET SDK.

## Overview

When an API call fails, the Kinde SDK throws an `ApiException` (`Kinde.Api.Client.ApiException`) containing detailed information about the error. This exception includes:

- **ErrorCode**: The HTTP status code (e.g., 404, 401, 500)
- **Message**: A descriptive error message
- **ErrorContent**: The raw response body (often contains additional error details)
- **Headers**: HTTP response headers

## Understanding ApiException

The `ApiException` class provides helper properties that make it easy to identify and handle different types of errors using C#'s pattern matching capabilities.

### Available Helper Properties

| Property | Description | HTTP Status Code |
|----------|-------------|------------------|
| `IsClientError` | True for any 4xx error | 400-499 |
| `IsServerError` | True for any 5xx error | 500-599 |
| `IsBadRequest` | Invalid request parameters | 400 |
| `IsUnauthorized` | Authentication required or token invalid/expired | 401 |
| `IsForbidden` | Authenticated but lacking permissions | 403 |
| `IsNotFound` | Resource doesn't exist | 404 |
| `IsConflict` | Resource state conflict | 409 |
| `IsUnprocessableEntity` | Validation failed | 422 |
| `IsRateLimitExceeded` | Too many requests | 429 |

## Basic Error Handling

### Simple Try-Catch

```csharp
using Kinde.Api.Api;
using Kinde.Api.Client;

try
{
    var user = await usersApi.GetUserAsync(userId);
    Console.WriteLine($"User: {user.Email}");
}
catch (ApiException ex)
{
    Console.WriteLine($"API Error: {ex.ErrorCode} - {ex.Message}");
    if (ex.ErrorContent != null)
    {
        Console.WriteLine($"Details: {ex.ErrorContent}");
    }
}
```

## Pattern Matching with Exception Filters

C#'s exception filters (`when` clause) work perfectly with the helper properties:

### Handling Specific Status Codes

```csharp
try
{
    var user = await usersApi.GetUserAsync(userId);
}
catch (ApiException ex) when (ex.IsNotFound)
{
    Console.WriteLine("User not found. Please check the user ID.");
}
catch (ApiException ex) when (ex.IsUnauthorized)
{
    Console.WriteLine("Authentication failed. Please check your access token.");
}
catch (ApiException ex) when (ex.IsForbidden)
{
    Console.WriteLine("You don't have permission to access this resource.");
}
catch (ApiException ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Differentiating Client vs Server Errors

```csharp
try
{
    var result = await organizationsApi.CreateOrganizationAsync(request);
}
catch (ApiException ex) when (ex.IsClientError)
{
    // 4xx errors - usually fixable by the client
    Console.WriteLine($"Invalid request: {ex.Message}");
    Console.WriteLine("Please check your request parameters.");
}
catch (ApiException ex) when (ex.IsServerError)
{
    // 5xx errors - server-side issues
    Console.WriteLine($"Server error: {ex.Message}");
    Console.WriteLine("Please try again later or contact support.");
}
```

## Advanced Patterns

### Handling Rate Limiting with Retry

```csharp
int maxRetries = 3;
int retryCount = 0;

while (retryCount < maxRetries)
{
    try
    {
        var users = await usersApi.GetUsersAsync();
        return users;
    }
    catch (ApiException ex) when (ex.IsRateLimitExceeded)
    {
        retryCount++;
        if (retryCount >= maxRetries)
        {
            Console.WriteLine("Rate limit exceeded. Maximum retries reached.");
            throw;
        }

        // Check for Retry-After header
        if (ex.Headers != null && ex.Headers.ContainsKey("Retry-After"))
        {
            var retryAfter = ex.Headers["Retry-After"].FirstOrDefault();
            if (int.TryParse(retryAfter, out int seconds))
            {
                Console.WriteLine($"Rate limited. Waiting {seconds} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                continue;
            }
        }

        // Default backoff
        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
        Console.WriteLine($"Rate limited. Waiting {delay.TotalSeconds} seconds...");
        await Task.Delay(delay);
    }
}
```

### Extracting Error Details

The `ErrorContent` property contains the response body, which often includes additional error information:

```csharp
using System.Text.Json;
using Kinde.Api.Client;

try
{
    var user = await usersApi.CreateUserAsync(request);
}
catch (ApiException ex) when (ex.IsBadRequest || ex.IsUnprocessableEntity)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    
    // ErrorContent is typically a string with JSON
    if (ex.ErrorContent is string jsonContent)
    {
        try
        {
            // Using System.Text.Json for modern .NET
            var errorDetails = JsonSerializer.Deserialize<ErrorResponse>(jsonContent);
            foreach (var error in errorDetails.Errors)
            {
                Console.WriteLine($"  - {error.Field}: {error.Message}");
            }
        }
        catch
        {
            // Fallback if parsing fails
            Console.WriteLine($"  Details: {ex.ErrorContent}");
        }
    }
}

// Example error response model
class ErrorResponse
{
    public List<FieldError> Errors { get; set; }
}

class FieldError
{
    public string Field { get; set; }
    public string Message { get; set; }
}
```

### Switch Expression Pattern (C# 8.0+)

```csharp
try
{
    var result = await api.SomeOperationAsync();
}
catch (ApiException ex)
{
    var action = ex.ErrorCode switch
    {
        404 => "Resource not found",
        401 => "Please authenticate",
        403 => "Access denied",
        429 => "Rate limit exceeded, slow down",
        >= 500 and < 600 => "Server error, try again later",
        _ => $"Error {ex.ErrorCode}: {ex.Message}"
    };
    
    Console.WriteLine(action);
}
```

### Using IsClientError and IsServerError

```csharp
try
{
    var data = await api.FetchDataAsync();
    ProcessData(data);
}
catch (ApiException ex)
{
    if (ex.IsClientError)
    {
        // Log and report to the user
        _logger.LogWarning($"Client error {ex.ErrorCode}: {ex.Message}");
        ShowUserFriendlyError(ex);
    }
    else if (ex.IsServerError)
    {
        // Log for investigation and retry
        _logger.LogError($"Server error {ex.ErrorCode}: {ex.Message}");
        await ScheduleRetry();
    }
    else
    {
        // Unexpected status code
        _logger.LogError($"Unexpected status code {ex.ErrorCode}: {ex.Message}");
        throw;
    }
}
```

## Best Practices

1. **Always handle ApiException**: Wrap API calls in try-catch blocks to gracefully handle failures.

2. **Use specific patterns first**: Catch specific error types (404, 401, etc.) before catching general client/server errors.

3. **Check ErrorContent for details**: The error response body often contains helpful debugging information.

4. **Implement retry logic for transient failures**: Especially for 429 (rate limiting) and 5xx errors.

5. **Log error details**: Include the ErrorCode, Message, and ErrorContent in your logs for troubleshooting.

6. **Provide user-friendly messages**: Don't expose raw error details to end users; translate them into actionable messages.

7. **Handle authentication errors gracefully**: For 401 errors, redirect to login or refresh the token.

## Common Error Scenarios

### 400 Bad Request
Indicates invalid request parameters. Check your input data and ensure it matches the API's requirements.

```csharp
catch (ApiException ex) when (ex.IsBadRequest)
{
    Console.WriteLine("Invalid request parameters. Please verify your input.");
    // Log ex.ErrorContent for specific field validation errors
}
```

### 401 Unauthorized
Your access token is missing, invalid, or expired. Obtain a new token.

```csharp
catch (ApiException ex) when (ex.IsUnauthorized)
{
    Console.WriteLine("Authentication required. Please log in again.");
    await RefreshTokenAsync();
}
```

### 403 Forbidden
You're authenticated but don't have permission for this resource or action.

```csharp
catch (ApiException ex) when (ex.IsForbidden)
{
    Console.WriteLine("You don't have permission to perform this action.");
    // Check user roles and permissions
}
```

### 404 Not Found
The requested resource doesn't exist.

```csharp
catch (ApiException ex) when (ex.IsNotFound)
{
    Console.WriteLine("Resource not found. It may have been deleted.");
    return null; // or handle appropriately
}
```

### 409 Conflict
The request conflicts with the current state of the resource (e.g., duplicate email).

```csharp
catch (ApiException ex) when (ex.IsConflict)
{
    Console.WriteLine("Resource already exists or state conflict.");
    // Prompt user to use different values
}
```

### 422 Unprocessable Entity
Request is well-formed but contains semantic errors (validation failure).

```csharp
catch (ApiException ex) when (ex.IsUnprocessableEntity)
{
    Console.WriteLine("Validation failed. Please check your input.");
    // Parse ErrorContent for specific validation errors
}
```

### 429 Too Many Requests
You've exceeded the rate limit. Implement exponential backoff.

```csharp
catch (ApiException ex) when (ex.IsRateLimitExceeded)
{
    var retryAfter = GetRetryAfterSeconds(ex.Headers);
    await Task.Delay(TimeSpan.FromSeconds(retryAfter));
    // Retry the request
}
```

### 5xx Server Errors
The server encountered an error. These are typically transient and retrying may succeed.

```csharp
catch (ApiException ex) when (ex.IsServerError)
{
    Console.WriteLine("Server error. Please try again later.");
    await Task.Delay(TimeSpan.FromSeconds(5));
    // Retry with exponential backoff
}
```

## Testing Error Handling

When writing unit tests, you can create ApiException instances to test your error handling:

```csharp
using Kinde.Api.Client;

[Test]
public void HandleNotFoundError()
{
    var exception = new ApiException(404, "User not found");
    
    Assert.IsTrue(exception.IsNotFound);
    Assert.IsTrue(exception.IsClientError);
    Assert.IsFalse(exception.IsServerError);
}

[Test]
public void HandleRateLimitError()
{
    var headers = new Multimap<string, string>();
    headers.Add("Retry-After", "60");
    
    var exception = new ApiException(429, "Rate limit exceeded", null, headers);
    
    Assert.IsTrue(exception.IsRateLimitExceeded);
    Assert.IsTrue(exception.IsClientError);
}
```

## Further Resources

- [Kinde API Documentation](https://kinde.com/docs/developer-tools/kinde-api/)
- [HTTP Status Codes Reference](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
- [.NET Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

## Support

If you encounter persistent errors or need help:
- Check the [Kinde Documentation](https://kinde.com/docs/)
- Visit the [Kinde Community](https://thekindecommunity.slack.com)
- Contact support@kinde.com
