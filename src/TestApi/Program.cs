using Fuzn.FluentHttp.TestApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Test API",
        Version = "v1",
        Description = "Test API for FluentHttp library testing"
    });
});

var app = builder.Build();

// Configure Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root (http://localhost:port/)
});

// Map endpoints from organized classes
app.MapHttpMethodsEndpoints();
app.MapQueryParametersEndpoints();
app.MapHeadersEndpoints();
app.MapAuthenticationEndpoints();
app.MapCookiesEndpoints();
app.MapContentTypesEndpoints();
app.MapFileUploadEndpoints();
app.MapResponseTypesEndpoints();
app.MapStatusCodesEndpoints();
app.MapStreamingEndpoints();
app.MapUtilityEndpoints();

app.Run();

// Make Program accessible for WebApplicationFactory in tests
public partial class Program { }
