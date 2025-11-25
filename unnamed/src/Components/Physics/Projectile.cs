using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Components.Physics;

public struct Projectile
{
    public Lifetime Lifetime;
    public int Damage;
    public int ExplosionRadius;
    public AssetRef<AnimationClip> ExplosionAnimation;
}