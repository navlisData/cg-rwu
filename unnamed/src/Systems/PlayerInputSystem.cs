using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.systems;

public sealed class PlayerInputSystem(World world, Func<KeyboardState> keyboardProvider) : EntitySetSystem<float>(world, world.Query()
    .With<Player>()
    .With<Velocity>()
    .Build()
)
{
    private readonly Func<KeyboardState> keyboardStateProvider = keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    protected override void Update(float dt, in Entity e)
    {
        var keyboardState = keyboardStateProvider();

        Vector2 direction = Vector2.Zero;
        if (keyboardState.IsKeyDown(Keys.Left))  direction.X -= 1;
        if (keyboardState.IsKeyDown(Keys.Right)) direction.X += 1;
        if (keyboardState.IsKeyDown(Keys.Up))    direction.Y += 1;
        if (keyboardState.IsKeyDown(Keys.Down))  direction.Y -= 1;

        ref var velocity = ref e.Get<Velocity>();

        const float acceleration = 12;
        const float friction = 8;
        const float maxSpeed = 5f;

        if (direction != Vector2.Zero)
        {
            direction = direction.Normalized();
            velocity.Value += direction * (acceleration * dt);
        }
        else
        {
            float effectiveFriction = MathF.Max(0f, 1f - friction * dt);
            velocity.Value *= effectiveFriction;
            if (velocity.Value.LengthSquared < 0.0001f) velocity.Value = Vector2.Zero;
        }

        float maxSquared = maxSpeed * maxSpeed;
        if (velocity.Value.LengthSquared > maxSquared)
            velocity.Value = velocity.Value.Normalized() * maxSpeed;
    }
}