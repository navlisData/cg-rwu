using System.Diagnostics;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Prefabs;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class PlayerInputSystem(World world, Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : EntitySetSystem<(float dt, Camera2D camera, Position player, Vector2i windowSize, IAssetStore assets)>(world,
        world.Query()
            .With<ReceivesPlayerInput>()
            .Build()
    )
{
    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(
        (float dt, Camera2D camera, Position player, Vector2i windowSize, IAssetStore assets) args, in Entity e)
    {
        KeyboardState keyboardState = this.keyboardStateProvider();
        MouseState mouseState = this.mouseStateProvider();
        float dt = args.dt;
        ref ReceivesPlayerInput playerInput = ref e.Get<ReceivesPlayerInput>();
        ref Camera2D camera2D = ref args.camera;
        ref Position playerPosition = ref args.player;

        if ((playerInput & ReceivesPlayerInput.MovementControls) == ReceivesPlayerInput.MovementControls)
        {
            Debug.Assert(e.Has<Velocity>());

            Vector2 direction = Vector2.Zero;
            if (keyboardState.IsKeyDown(Controls.MoveLeft))
            {
                direction.X -= 1;
            }

            if (keyboardState.IsKeyDown(Controls.MoveRight))
            {
                direction.X += 1;
            }

            if (keyboardState.IsKeyDown(Controls.MoveUp))
            {
                direction.Y += 1;
            }

            if (keyboardState.IsKeyDown(Controls.MoveDown))
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

        if ((playerInput & ReceivesPlayerInput.AlignByMouse) == ReceivesPlayerInput.AlignByMouse)
        {
            Debug.Assert(e.Has<AlignedCharacter>());
            Debug.Assert(e.Has<Velocity>());

            ref AlignedCharacter alignedCharacter = ref e.Get<AlignedCharacter>();
            ref Velocity velocity = ref e.Get<Velocity>();
            alignedCharacter.CharacterDirection =
                mouseState.Get8WayDirectionFromPosition(args.windowSize, alignedCharacter.CharacterDirection);

            if (velocity.Value == Vector2.Zero)
            {
                alignedCharacter.ActionIndex = (int)PlayerAction.Idle;
            }
            else
            {
                alignedCharacter.ActionIndex = (int)PlayerAction.Move;
            }
        }

        if ((playerInput & ReceivesPlayerInput.MouseControls) == ReceivesPlayerInput.MouseControls)
        {
            if (mouseState.IsButtonReleased(Controls.PlayerShoot))
            {
                Vector2 mousePositionWorld =
                    Projection.ScreenToWorldCoordinates(mouseState.Position, camera2D.Viewport,
                        camera2D.ViewProjection);

                Vector2 bulletDirection =
                    -Vector2.NormalizeFast(playerPosition.ToWorldPosition() -
                                           new Vector2(mousePositionWorld.X, mousePositionWorld.Y));

                PrefabFactory.CreateBullet(this.world, playerPosition,
                    bulletDirection * 7.5f, (float)MathHelper.Atan2(bulletDirection.Y, bulletDirection.X), 2,
                    args.assets);

                if ((playerInput & ReceivesPlayerInput.AlignByMouse) == ReceivesPlayerInput.AlignByMouse)
                {
                    e.Get<AlignedCharacter>().ActionIndex = (int)PlayerAction.Shoot;
                }
#if DEBUG
                Console.WriteLine(
                    $"Clicked at {mousePositionWorld} (Global Coords)");
#endif
            }
        }

        if ((playerInput & ReceivesPlayerInput.CameraControls) == ReceivesPlayerInput.CameraControls)
        {
            Debug.Assert(e.Has<Camera2D>());
            ref Camera2D camera = ref e.Get<Camera2D>();

            camera.Zoom *= (float)Math.Pow(1.1f, mouseState.ScrollDelta.Y);
            camera.Zoom = Math.Clamp(camera.Zoom, 0.01f, 5.0f);

            if (keyboardState.IsKeyDown(Controls.RotateCamCW))
            {
                camera.Rotation += .1f;
            }

            if (keyboardState.IsKeyDown(Controls.RotateCamCCW))
            {
                camera.Rotation -= .1f;
            }
        }

        if ((playerInput & ReceivesPlayerInput.PositionByMouse) == ReceivesPlayerInput.PositionByMouse)
        {
            Debug.Assert(e.Has<Position>());
            Debug.Assert(e.Has<Transform>());
            ref Position pos = ref e.Get<Position>();
            ref Transform transform = ref e.Get<Transform>();

            try
            {
                Vector2 mousePositionWorld =
                    Projection.ScreenToWorldCoordinates(mouseState.Position, camera2D.Viewport,
                        camera2D.ViewProjection);

                pos = new Position(Vector2i.Zero, Vector2i.Zero, mousePositionWorld);
                transform.Scale = 1 / camera2D.Zoom;
            }
            catch
            {
                // ignored, this will fail if the Window is not initialized yet
            }
        }
    }
}