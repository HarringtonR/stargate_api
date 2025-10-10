using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for load balancer (required for EB)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.

// Register the logging action filter
builder.Services.AddScoped<LoggingActionFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LoggingActionFilter>();
});

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "https://main.dqh6niin9ecfm.amplifyapp.com"  // Add your Amplify URL
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

// Register logging service
builder.Services.AddScoped<IProcessLoggingService, ProcessLoggingService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.AddRequestPreProcessor<UpdatePersonPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Configure forwarded headers (must be early in pipeline)
app.UseForwardedHeaders();

// Ensure database is created and seeded in production
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StargateContext>();
    try
    {
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        // Log the exception but don't fail the startup
        app.Logger.LogError(ex, "An error occurred while creating the database");
    }
}

// Configure the HTTP request pipeline.

// Add health check endpoint (for ELB health checks)
app.MapHealthChecks("/health");

// Simple health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "StargateAPI",
    environment = app.Environment.EnvironmentName
}));

// Enable CORS first (before other middleware)
app.UseCors("AllowAngularApp");

// Only use development middleware in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Log startup information
app.Logger.LogInformation("StargateAPI started successfully");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Listening on: {Urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "Default URLs");
app.Logger.LogInformation("Available endpoints: /, /health, /api(...)");

app.Run();



