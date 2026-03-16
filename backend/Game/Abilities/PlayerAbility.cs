namespace conquerio.Game.Abilities;

public abstract class PlayerAbility
{
    public bool IsReady { get => !isActivated && cooldownTicksRemaining <= 0; }
    public bool IsActivated { get => isActivated; }

    public int CooldownTicksRemaining { get => cooldownTicksRemaining; }
    public int DurationTicksRemaining { get => durationTicksRemaining; }

    private bool isActivated = false;
    private int cooldownTicksRemaining = 0;
    private int durationTicksRemaining = 0;

    private GameRoom gameRoom;

    public abstract string Tag { get; }
    protected abstract int DurationSeconds { get; }
    protected abstract int CooldownSeconds { get; }

    public PlayerAbility(GameRoom gameRoom)
    {
        this.gameRoom = gameRoom;
    }

    public void Activate()
    {
        if (!IsReady)
            return;
        Start();
        isActivated = true;
        durationTicksRemaining = gameRoom.TickRate * DurationSeconds;
    }

    public void Tick()
    {
        cooldownTicksRemaining = Math.Max(0, cooldownTicksRemaining - 1);
        if (durationTicksRemaining > 0)
        {
            if (--durationTicksRemaining <= 0)
            {
                Finish();
                isActivated = false;
                cooldownTicksRemaining = CooldownSeconds * gameRoom.TickRate;
            }
            else Update();
        }

    }

    protected abstract void Update();
    protected abstract void Start();
    protected abstract void Finish();

}
