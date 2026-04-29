namespace unnamed.Components.General;

public struct EntityStats
{
    public int Hitpoints { get; private set; }
    public int MaxHealthUnits { get; private set; }
    public int AbsoluteMaxHealthUnits { get; private set; }
    public float AttackCooldown { get; set; } = 0f;
    public float MaxAttackCooldown { get; set; } = 1f;
    public int ScoreReward { get; set; } = 10;

    /// <summary>
    ///     Creates entity stats with consistent initial values.
    /// </summary>
    /// <param name="initHealthUnits">Initial hitpoints and current max health.</param>
    /// <param name="absMaxHealthUnits">Absolute upper bound for max health.</param>
    /// <param name="scoreReward">The reward for killing this entity.</param>
    public EntityStats(int initHealthUnits, int absMaxHealthUnits, int scoreReward = 5)
    {
        this.AbsoluteMaxHealthUnits = Math.Max(0, absMaxHealthUnits);

        this.MaxHealthUnits = Math.Clamp(initHealthUnits, 0, this.AbsoluteMaxHealthUnits);
        this.Hitpoints = this.MaxHealthUnits;
        this.ScoreReward = scoreReward;
    }

    public void SetHitpoints(int value) => this.Hitpoints = value;

    public void SetMaxHealthUnits(int value) => this.MaxHealthUnits = value;

    public void SetAbsoluteMaxHealthUnits(int value) => this.AbsoluteMaxHealthUnits = value;
}