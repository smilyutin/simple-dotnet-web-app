namespace SimpleWebApi.Test;

using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

/// <summary>
/// Integration tests for SimpleWebApi using the in-memory test server
/// provided by WebApplicationFactory.
/// </summary>
public class WebApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Constructor: initializes the WebApplicationFactory and creates an HttpClient.
    /// HttpClient will send requests to the in-memory ASP.NET Core pipeline
    /// without needing a real HTTP server.
    /// </summary>
    public WebApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Test that GET /weatherforecast returns:
    /// - HTTP 200 OK
    /// - Content-Type: application/json
    /// This validates that the WeatherForecast endpoint is healthy
    /// and returning structured JSON as expected.
    /// </summary>
    [Fact(DisplayName = "WeatherForecast endpoint returns 200 OK and JSON")]
    public async Task TestWeatherForecast()
    {
        // Act: send GET request to /weatherforecast
        var response = await _client.GetAsync("/weatherforecast");

        // Assert: check status code and response content type
        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }

    /// <summary>
    /// Test that GET /swagger returns:
    /// - HTTP 200 OK
    /// - Content-Type: text/html
    /// This ensures Swagger UI is available for API documentation.
    /// </summary>
    [Fact(DisplayName = "Swagger UI endpoint returns 200 OK and HTML")]
    public async Task TestSwagger()
    {
        // Act: send GET request to /swagger
        var response = await _client.GetAsync("/swagger");

        // Assert: check status code and response content type
        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }
}