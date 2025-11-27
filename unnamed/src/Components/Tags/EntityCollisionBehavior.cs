namespace unnamed.Components.Tags;

[Flags]
public enum EntityCollisionBehavior
{
    None = 0,
    DestroySelf = 1 << 0,
    Explode = 1 << 1
}

public static class EntityCollisionBehaviorExtension
{
    public static bool DestroySelf(this EntityCollisionBehavior tile)
    {
        return (tile & EntityCollisionBehavior.DestroySelf) == EntityCollisionBehavior.DestroySelf;
    }

    public static bool Explode(this EntityCollisionBehavior tile)
    {
        return (tile & EntityCollisionBehavior.Explode) == EntityCollisionBehavior.Explode;
    }
}