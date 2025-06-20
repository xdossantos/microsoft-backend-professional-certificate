using Microsoft.EntityFrameworkCore;
using Serilog;
using AcmeGrocer.Data;
using AcmeGrocer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/acme-grocer-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<CustomerContext>(options =>
    options.UseInMemoryDatabase("CustomerDb")); // Using in-memory database for demo

// Add custom services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerScoringService, CustomerScoringService>();
builder.Services.AddScoped<IBinarySerializationService, BinarySerializationService>();
builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AcmeGrocer Customer API", Version = "v1" });
});

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AcmeGrocer Customer API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI as the root
    });
}

app.UseCors();

// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<CustomerContext>();
        await context.Database.EnsureCreatedAsync();
        
        var seedingService = scope.ServiceProvider.GetRequiredService<IDataSeedingService>();
        await seedingService.SeedCustomersFromJsonAsync();
        
        Log.Information("Application started successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Application failed to start");
        throw;
    }
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

Log.Information("AcmeGrocer Customer API is running...");

app.Run();