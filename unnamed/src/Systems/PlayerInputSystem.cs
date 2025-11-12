using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Prefabs;
using unnamed.Texture;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class PlayerInputSystem(World world, Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : EntitySetSystem<(float dt, Camera2D camera, Position player, Vector2i windowSize, IAssetStore assets,
        ActionControlHandler<PlayerAction> actionHandler)>(world,
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
        (float dt, Camera2D camera, Position player, Vector2i windowSize, IAssetStore assets,
            ActionControlHandler<PlayerAction> actionHandler) args, in Entity e)
    {
        KeyboardState keyboardState = this.keyboardStateProvider();
        MouseState mouseState = this.mouseStateProvider();
        float dt = args.dt;
        ref Camera2D camera2D = ref args.camera;
        ref Position playerPosition = ref args.player;

        if (e.Has<Player>())
        {
            PlayerAction currentState;

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

            ref PlayerActionState playerState = ref e.Get<PlayerActionState>();

            ref Velocity velocity = ref e.Get<Velocity>();
            ref AlignedCharacter alignedCharacter = ref e.Get<AlignedCharacter>();
            alignedCharacter.CharacterDirection =
                mouseState.Get8WayDirectionFromPosition(args.windowSize, alignedCharacter.CharacterDirection);

            const float acceleration = 12;
            const float friction = 8;
            const float maxSpeed = 5f;

            if (direction != Vector2.Zero)
            {
                currentState = args.actionHandler.TryUpdateAction(
                    ref playerState.CurrentAction,
                    ref playerState.RemainingTime,
                    desiredAction: PlayerAction.Move,
                    out bool success
                );

                direction = direction.Normalized();
                velocity.Value = success
                    ? velocity.Value += direction * (acceleration * dt)
                    : Vector2.Zero;
            }
            else
            {
                float effectiveFriction = MathF.Max(0f, 1f - (friction * dt));
                velocity.Value *= effectiveFriction;
                if (velocity.Value.LengthSquared < 0.0001f)
                {
                    velocity.Value = Vector2.Zero;
                }

                currentState = args.actionHandler.TryUpdateAction(
                    ref playerState.CurrentAction,
                    ref playerState.RemainingTime,
                    PlayerAction.Idle,
                    out bool _
                );
            }

            float maxSquared = maxSpeed * maxSpeed;
            if (velocity.Value.LengthSquared > maxSquared)
            {
                velocity.Value = velocity.Value.Normalized() * maxSpeed;
            }

            if (mouseState.IsButtonReleased(Controls.PlayerShoot))
            {
                Vector2 mousePositionWorld =
                    Projection.ScreenToWorldCoordinates(mouseState.Position, camera2D.Viewport,
                        camera2D.ViewProjection);

                Vector2 bulletDirection =
                    -Vector2.NormalizeFast(playerPosition.ToWorldPosition() -
                                           new Vector2(mousePositionWorld.X, mousePositionWorld.Y));

                Entity unused = PrefabFactory.CreateBullet(this.world, playerPosition,
                    bulletDirection * 7.5f, (float)MathHelper.Atan2(bulletDirection.Y, bulletDirection.X), 2,
                    args.assets);

                var clip = args.assets.Get(GameAssets.Player.Attack.East);
                float visibleForSeconds = clip.Frames.Count / clip.Fps;

                currentState = args.actionHandler.TryUpdateAction(
                    ref playerState.CurrentAction,
                    ref playerState.RemainingTime,
                    PlayerAction.Shoot,
                    visibleForSeconds,
                    out bool _
                );
#if DEBUG
                Console.WriteLine($"{mousePositionWorld}");
#endif
            }

            alignedCharacter.ActionIndex = (byte)currentState;
            args.actionHandler.Sync(
                ref playerState.CurrentAction,
                ref playerState.RemainingTime,
                dt);
        }

        if (e.Has<Camera2D>())
        {
            ref Camera2D camera = ref e.Get<Camera2D>();

            camera.Zoom *= (float)Math.Pow(1.1f, mouseState.ScrollDelta.Y);
            camera.Zoom = Math.Clamp(camera.Zoom, 0.1f, 5.0f);

            if (keyboardState.IsKeyDown(Controls.RotateCamCW))
            {
                camera.Rotation += .1f;
            }

            if (keyboardState.IsKeyDown(Controls.RotateCamCCW))
            {
                camera.Rotation -= .1f;
            }
        }
    }
}