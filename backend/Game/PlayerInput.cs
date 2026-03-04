namespace conquerio.Game;

public class PlayerInput
{
    public required string PlayerId { get; set; }
    public Direction? Direction { get; set; }
    public PlayerAbility? Ability { get; set; }
}
