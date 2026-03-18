using conquerio.Data;
using Microsoft.EntityFrameworkCore;

namespace conquerio.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", async (AppDbContext db) =>
        {
            try
            {
                await db.Database.CanConnectAsync();
                return Results.Ok(new { status = "healthy" });
            }
            catch
            {
                return Results.StatusCode(503);
            }
        })
        .WithTags("Health")
        .WithSummary("Check system health")
        .WithDescription("Checks the API and database connectivity to ensure the system is operational.");
    }
}
