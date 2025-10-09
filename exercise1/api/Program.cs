using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the logging action filter
builder.Services.AddScoped<LoggingActionFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LoggingActionFilter>();
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
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

// Configure the HTTP request pipeline.

// Enable CORS first (before other middleware)
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


