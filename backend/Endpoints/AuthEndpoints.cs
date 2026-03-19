using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using conquerio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace conquerio.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // POST /api/auth/register
        app.MapPost("/api/auth/register", async (
            RegisterRequest request,
            UserManager<AppUser> userManager) =>
        {
            var user = new AppUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return Results.BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Results.Ok(new { message = "Registration successful." });
        })
        .WithTags("Auth")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account with the specified username, email, and password.");

        // POST /api/auth/login
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            UserManager<AppUser> userManager,
            IConfiguration config) =>
        {
            var user = await userManager.FindByNameAsync(request.Username);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
                return Results.Unauthorized();

            var token = GenerateJwtToken(user, config);
            return Results.Ok(new { token });
        })
        .WithTags("Auth")
        .WithSummary("Login and get a JWT token")
        .WithDescription("Authenticates a user and returns a JWT token for subsequent requests.");

        // GET /api/auth/me
        app.MapGet("/api/auth/me", (ClaimsPrincipal principal, UserManager<AppUser> userManager) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            return Results.Ok(new
            {
                id = userId,
                username = principal.FindFirstValue(ClaimTypes.Name),
                email = principal.FindFirstValue(ClaimTypes.Email)
            });
        })
        .RequireAuthorization()
        .WithTags("Auth")
        .WithSummary("Get current user profile")
        .WithDescription("Returns information about the currently authenticated user based on the JWT token.");
    }

    private static string GenerateJwtToken(AppUser user, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
