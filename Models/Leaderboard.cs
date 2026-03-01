using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace conquerio.Models;

public class Leaderboard
{
    [Key]
    [ForeignKey(nameof(User))]
    [MaxLength(450)]
    public string UserId { get; set; } = null!;

    public int Elo { get; set; } = 1000;

    public float BestPct { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
}
