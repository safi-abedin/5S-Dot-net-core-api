using api.Data;
using api.Extensions;
using api.Helpers.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Serilog
builder.AddSerilogLogging();

// 📦 Services (ALL DI inside this)
builder.Services.AddApplicationServices(builder.Configuration);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔥 Middleware (ORDER MATTERS)
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingEnrichmentMiddleware>();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "5S API V1");
    options.RoutePrefix = "swagger";
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}

// CORS
app.UseCors("AllowAll");

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();