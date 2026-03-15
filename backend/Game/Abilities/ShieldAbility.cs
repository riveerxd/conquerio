namespace conquerio.Game.Abilities;

public class ShieldAbility : PlayerAbility
{
    private PlayerState player;

    protected override int DurationSeconds => 1;

    protected override int CooldownSeconds => 90;

    public override string Tag => "SHIELD";

    public ShieldAbility(GameRoom gameRoom, PlayerState player) : base(gameRoom)
    {
        this.player = player;
    }

    protected override void Finish()
    {
        player.Invulnerable = false;
    }

    protected override void Start()
    {
        player.Invulnerable = true;
    }

    protected override void Update()
    {
    }
}
