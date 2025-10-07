using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register services needed for Swagger/OpenAPI support
// - AddEndpointsApiExplorer: enables endpoint discovery
// - AddSwaggerGen: generates OpenAPI/Swagger docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI only in Development environment
// (avoids exposing internal docs in Production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware: redirect HTTP → HTTPS automatically
app.UseHttpsRedirection();

// Example data source: list of weather summaries
string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Define a minimal API endpoint at GET /weatherforecast
// - Generates 5 random forecast records
// - Returns JSON
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),    // future date
            Random.Shared.Next(-20, 55),                           // random temperature (°C)
            summaries[Random.Shared.Next(summaries.Length)]        // random summary
        )
    );
    return Results.Json(forecast);
})
.WithName("GetWeatherForecast")   // Swagger-friendly name
.WithOpenApi();                   // Auto-generate OpenAPI docs

// Run the application
// Important: keep Program partial class for test projects (xUnit + WebApplicationFactory)
app.Run();
public partial class Program { }

// Record type: immutable DTO for WeatherForecast
// Includes a derived property TemperatureF (calculated from °C)
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}