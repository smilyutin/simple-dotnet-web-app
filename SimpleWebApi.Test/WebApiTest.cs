namespace SimpleWebApi.Test;

using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class WebApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "WeatherForecast endpoint returns 200 OK and JSON")]
    public async Task TestWeatherForecast()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        response.EnsureSuccessStatusCode(); // 2xx
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact(DisplayName = "Swagger UI endpoint returns 200 OK and HTML")]
    public async Task TestSwagger()
    {
        // Act
        var response = await _client.GetAsync("/swagger");

        // Assert
        response.EnsureSuccessStatusCode(); // 2xx
        Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }
}