namespace conquerio.Game.Abilities;

public class BoostAbility : PlayerAbility
{
    private PlayerState player;

    protected override int DurationSeconds => 3;

    protected override int CooldownSeconds => 10;

    public override string Tag => "BOOST";

    public BoostAbility(GameRoom gameRoom, PlayerState player) : base(gameRoom)
    {
        this.player = player;
    }

    protected override void Finish()
    {
        player.SpeedMultiplier = 1;
    }

    protected override void Start()
    {
        player.SpeedMultiplier = 2;
    }

    protected override void Update()
    {
        
    }
}
