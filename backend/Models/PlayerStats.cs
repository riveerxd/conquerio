using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace conquerio.Models;

public class PlayerStats
{
    [Key]
    [ForeignKey(nameof(User))]
    [MaxLength(450)]
    public string UserId { get; set; } = null!;

    public int Elo { get; set; } = 1000;
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }

    /// <summary>Best territory percentage ever achieved (0.0 – 100.0)</summary>
    public float BestTerritoryPct { get; set; }

    public int TotalGames { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
}
