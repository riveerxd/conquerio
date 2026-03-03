using conquerio.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace conquerio.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlayerStats> PlayerStats => Set<PlayerStats>();
    public DbSet<Leaderboard> Leaderboard => Set<Leaderboard>();
    public DbSet<GameRun> GameRuns => Set<GameRun>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<PlayerStats>()
            .HasOne(ps => ps.User)
            .WithOne(u => u.PlayerStats)
            .HasForeignKey<PlayerStats>(ps => ps.UserId);

        builder.Entity<Leaderboard>()
            .HasOne(lb => lb.User)
            .WithOne(u => u.Leaderboard)
            .HasForeignKey<Leaderboard>(lb => lb.UserId);

        builder.Entity<GameRun>()
            .HasOne(gr => gr.User)
            .WithMany(u => u.GameRuns)
            .HasForeignKey(gr => gr.UserId);
    }
}
