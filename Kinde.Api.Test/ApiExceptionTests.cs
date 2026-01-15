using Kinde.Api.Client;
using Xunit;

namespace Kinde.Api.Test
{
    /// <summary>
    /// Tests for ApiException helper properties that enable pattern matching on different error types.
    /// </summary>
    public class ApiExceptionTests
    {
        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(403)]
        [InlineData(404)]
        [InlineData(422)]
        [InlineData(429)]
        [InlineData(499)]
        public void IsClientError_ShouldReturnTrue_For4xxStatusCodes(int statusCode)
        {
            // Arrange
            var exception = new ApiException(statusCode, "Client error");

            // Act & Assert
            Assert.True(exception.IsClientError);
            Assert.False(exception.IsServerError);
        }

        [Theory]
        [InlineData(500)]
        [InlineData(501)]
        [InlineData(502)]
        [InlineData(503)]
        [InlineData(504)]
        [InlineData(599)]
        public void IsServerError_ShouldReturnTrue_For5xxStatusCodes(int statusCode)
        {
            // Arrange
            var exception = new ApiException(statusCode, "Server error");

            // Act & Assert
            Assert.True(exception.IsServerError);
            Assert.False(exception.IsClientError);
        }

        [Theory]
        [InlineData(200)]
        [InlineData(201)]
        [InlineData(204)]
        [InlineData(304)]
        public void IsClientError_AndIsServerError_ShouldReturnFalse_ForSuccessStatusCodes(int statusCode)
        {
            // Arrange
            var exception = new ApiException(statusCode, "Success");

            // Act & Assert
            Assert.False(exception.IsClientError);
            Assert.False(exception.IsServerError);
        }

        [Fact]
        public void IsBadRequest_ShouldReturnTrue_For400StatusCode()
        {
            // Arrange
            var exception = new ApiException(400, "Bad request");

            // Act & Assert
            Assert.True(exception.IsBadRequest);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void IsUnauthorized_ShouldReturnTrue_For401StatusCode()
        {
            // Arrange
            var exception = new ApiException(401, "Unauthorized");

            // Act & Assert
            Assert.True(exception.IsUnauthorized);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void IsForbidden_ShouldReturnTrue_For403StatusCode()
        {
            // Arrange
            var exception = new ApiException(403, "Forbidden");

            // Act & Assert
            Assert.True(exception.IsForbidden);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void IsNotFound_ShouldReturnTrue_For404StatusCode()
        {
            // Arrange
            var exception = new ApiException(404, "Not found");

            // Act & Assert
            Assert.True(exception.IsNotFound);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void IsConflict_ShouldReturnTrue_For409StatusCode()
        {
            // Arrange
            var exception = new ApiException(409, "Conflict");

            // Act & Assert
            Assert.True(exception.IsConflict);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void IsUnprocessableEntity_ShouldReturnTrue_For422StatusCode()
        {
            // Arrange
            var exception = new ApiException(422, "Unprocessable entity");

            // Act & Assert
            Assert.True(exception.IsUnprocessableEntity);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void IsRateLimitExceeded_ShouldReturnTrue_For429StatusCode()
        {
            // Arrange
            var exception = new ApiException(429, "Too many requests");

            // Act & Assert
            Assert.True(exception.IsRateLimitExceeded);
            Assert.True(exception.IsClientError);
        }

        [Fact]
        public void ApiException_ShouldStoreErrorCodeAndMessage()
        {
            // Arrange
            var errorCode = 404;
            var message = "Resource not found";

            // Act
            var exception = new ApiException(errorCode, message);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ApiException_ShouldStoreErrorContent()
        {
            // Arrange
            var errorCode = 400;
            var message = "Bad request";
            var errorContent = "{\"error\": \"Invalid parameter\"}";

            // Act
            var exception = new ApiException(errorCode, message, errorContent);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Equal(errorContent, exception.ErrorContent);
        }

        [Fact]
        public void ApiException_ShouldStoreHeaders()
        {
            // Arrange
            var errorCode = 429;
            var message = "Rate limit exceeded";
            var headers = new Multimap<string, string>();
            headers.Add("Retry-After", "60");
            headers.Add("X-RateLimit-Limit", "100");

            // Act
            var exception = new ApiException(errorCode, message, null, headers);

            // Assert
            Assert.Equal(errorCode, exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.NotNull(exception.Headers);
            Assert.True(exception.Headers.ContainsKey("Retry-After"));
            Assert.Equal("60", exception.Headers["Retry-After"][0]);
        }

        [Theory]
        [InlineData(400, true, false, false, false, false, false, false, false)]
        [InlineData(401, false, true, false, false, false, false, false, false)]
        [InlineData(403, false, false, true, false, false, false, false, false)]
        [InlineData(404, false, false, false, true, false, false, false, false)]
        [InlineData(409, false, false, false, false, true, false, false, false)]
        [InlineData(422, false, false, false, false, false, true, false, false)]
        [InlineData(429, false, false, false, false, false, false, true, false)]
        [InlineData(500, false, false, false, false, false, false, false, true)]
        public void HelperProperties_ShouldReturnCorrectValues(
            int statusCode,
            bool expectedBadRequest,
            bool expectedUnauthorized,
            bool expectedForbidden,
            bool expectedNotFound,
            bool expectedConflict,
            bool expectedUnprocessableEntity,
            bool expectedRateLimitExceeded,
            bool expectedServerError)
        {
            // Arrange
            var exception = new ApiException(statusCode, "Test error");

            // Act & Assert
            Assert.Equal(expectedBadRequest, exception.IsBadRequest);
            Assert.Equal(expectedUnauthorized, exception.IsUnauthorized);
            Assert.Equal(expectedForbidden, exception.IsForbidden);
            Assert.Equal(expectedNotFound, exception.IsNotFound);
            Assert.Equal(expectedConflict, exception.IsConflict);
            Assert.Equal(expectedUnprocessableEntity, exception.IsUnprocessableEntity);
            Assert.Equal(expectedRateLimitExceeded, exception.IsRateLimitExceeded);
            
            // For server error, also check IsServerError
            if (expectedServerError)
            {
                Assert.True(exception.IsServerError);
            }
        }

        [Fact]
        public void ApiException_CanBeUsedInPatternMatching()
        {
            // Arrange
            var notFoundException = new ApiException(404, "Not found");
            var unauthorizedException = new ApiException(401, "Unauthorized");
            var serverException = new ApiException(500, "Internal server error");

            // Act & Assert - Simulate pattern matching scenarios
            Assert.True(HandleException(notFoundException) == "Resource not found");
            Assert.True(HandleException(unauthorizedException) == "Authentication required");
            Assert.True(HandleException(serverException) == "Server error occurred");
        }

        // Helper method to simulate real-world pattern matching usage
        private string HandleException(ApiException ex)
        {
            if (ex.IsNotFound)
                return "Resource not found";
            if (ex.IsUnauthorized)
                return "Authentication required";
            if (ex.IsServerError)
                return "Server error occurred";
            return "Unknown error";
        }
    }
}
