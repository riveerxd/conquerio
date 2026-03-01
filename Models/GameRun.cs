using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace conquerio.Models;

public class GameRun
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(User))]
    [MaxLength(450)]
    public string UserId { get; set; } = null!;

    public int Kills { get; set; }

    public float MaxTerritoryPct { get; set; }

    public int TotalXp { get; set; }

    [MaxLength(100)]
    public string? DeathCause { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
}
