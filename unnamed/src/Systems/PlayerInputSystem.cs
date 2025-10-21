using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.systems;

public sealed class PlayerInputSystem(World world, Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : EntitySetSystem<float>(world,
        world.Query()
            .With<ReceivesPlayerInput>()
            .Build()
    )
{
    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(float dt, in Entity e)
    {
        KeyboardState keyboardState = this.keyboardStateProvider();
        MouseState mouseState = this.mouseStateProvider();

        if (e.Has<Velocity>())
        {
            Vector2 direction = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                direction.X -= 1;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                direction.X += 1;
            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                direction.Y += 1;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {
                direction.Y -= 1;
            }

            ref Velocity velocity = ref e.Get<Velocity>();

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
                float effectiveFriction = MathF.Max(0f, 1f - (friction * dt));
                velocity.Value *= effectiveFriction;
                if (velocity.Value.LengthSquared < 0.0001f)
                {
                    velocity.Value = Vector2.Zero;
                }
            }

            float maxSquared = maxSpeed * maxSpeed;
            if (velocity.Value.LengthSquared > maxSquared)
            {
                velocity.Value = velocity.Value.Normalized() * maxSpeed;
            }
        }

        if (e.Has<Camera2D>())
        {
            ref Camera2D camera = ref e.Get<Camera2D>();

            camera.Zoom *= (float)Math.Pow(1.1f, mouseState.ScrollDelta.Y);
            camera.Zoom = Math.Clamp(camera.Zoom, 0.1f, 5.0f);

            if (keyboardState.IsKeyDown(Keys.Q))
            {
                camera.Rotation += .1f;
            }

            if (keyboardState.IsKeyDown(Keys.E))
            {
                camera.Rotation -= .1f;
            }
        }
    }
}