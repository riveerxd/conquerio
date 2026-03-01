using Microsoft.AspNetCore.Identity;

namespace conquerio.Models;

public class AppUser : IdentityUser
{
    // Username and Email are inherited from IdentityUser
    // PasswordHash is managed by Identity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public PlayerStats? PlayerStats { get; set; }
    public Leaderboard? Leaderboard { get; set; }
    public ICollection<GameRun> GameRuns { get; set; } = [];
}
