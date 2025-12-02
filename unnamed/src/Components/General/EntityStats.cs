namespace unnamed.Components.General;

public struct EntityStats
{
    public int Hitpoints { get; private set; }
    public int MaxHealthUnits { get; private set; }
    public int AbsoluteMaxHealthUnits { get; private set; }

    /// <summary>
    ///     Creates entity stats with consistent initial values.
    /// </summary>
    /// <param name="initHealthUnits">Initial hitpoints and current max health.</param>
    /// <param name="absMaxHealthUnits">Absolute upper bound for max health.</param>
    public EntityStats(int initHealthUnits, int absMaxHealthUnits)
    {
        this.AbsoluteMaxHealthUnits = Math.Max(0, absMaxHealthUnits);

        this.MaxHealthUnits = Math.Clamp(initHealthUnits, 0, this.AbsoluteMaxHealthUnits);
        this.Hitpoints = this.MaxHealthUnits;
    }

    public void SetHitpoints(int value) => this.Hitpoints = value;

    public void SetMaxHealthUnits(int value) => this.MaxHealthUnits = value;

    public void SetAbsoluteMaxHealthUnits(int value) => this.AbsoluteMaxHealthUnits = value;
}