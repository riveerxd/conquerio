using System.Text;
using conquerio.Data;
using conquerio.Endpoints;
using conquerio.Game;
using conquerio.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/conquerio-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Properties.ContainsKey("Metric"))
        .WriteTo.File("Logs/conquerio-metrics-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

builder.Host.UseSerilog();

// EF Core + MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Identity Core
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

// HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Game services
builder.Services.AddSingleton<GameRoomManager>();
builder.Services.AddHostedService<GameTickHostedService>();

var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Enable HSTS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseResponseCompression();
app.UseWebSockets();

// Content-Security-Policy (CSP)
var productionCsp = "default-src 'self'; script-src 'self'; style-src 'self'; connect-src 'self' wss: https:; img-src 'self' data:; font-src 'self' data:; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
var developmentCsp = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' blob:; style-src 'self' 'unsafe-inline'; connect-src 'self' wss: ws: http: https:; img-src 'self' data: blob:; font-src 'self' data:; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
var cspToUse = app.Environment.IsDevelopment() ? developmentCsp : productionCsp;

app.Use(async (context, next) =>
{
    // Add CSP header if not already present
    if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
    {
        context.Response.Headers.Append("Content-Security-Policy", cspToUse);
    }

    await next(context);
});

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapGameEndpoints();
app.MapWebSocketEndpoints();
app.MapHealthEndpoints();

app.Run();
