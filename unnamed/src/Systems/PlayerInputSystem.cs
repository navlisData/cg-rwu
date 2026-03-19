using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Querying;
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
using unnamed.Utils.Health;

namespace unnamed.systems;

public sealed class PlayerInputSystem(World world, Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : EntitySetSystem<(float dt, Camera2D camera, Vector2i windowSize, IAssetStore assets,
        ActionControlHandler<PlayerAction> actionHandler)>(world,
        new QueryBuilder()
            .With<ReceivesPlayerInput>()
            .With<AlignedCharacter>()
            .With<Position>()
            .With<Velocity>()
            .Build()
    )
{
    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(
        (float dt, Camera2D camera, Vector2i windowSize, IAssetStore assets, ActionControlHandler<PlayerAction>
            actionHandler) args, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        KeyboardState keyboardState = this.keyboardStateProvider();
        MouseState mouseState = this.mouseStateProvider();
        float dt = args.dt;

        ref Velocity velocity = ref handle.Get<Velocity>();
        ref Camera2D camera2D = ref args.camera;
        ref Position playerPosition = ref handle.Get<Position>();
        ref PlayerActionState playerState = ref handle.Get<PlayerActionState>();
        ref AlignedCharacter alignedCharacter = ref handle.Get<AlignedCharacter>();

        alignedCharacter.CharacterDirection =
            mouseState.Get8WayDirectionFromPosition(args.windowSize, alignedCharacter.CharacterDirection);

        if (handle.Has<Player>())
        {
            if (keyboardState.IsKeyPressed(Keys.D1))
            {
                handle.AddDamage(2);
            }

            if (keyboardState.IsKeyPressed(Keys.D2))
            {
                handle.AddHealth(2);
            }

            if (keyboardState.IsKeyPressed(Keys.D3))
            {
                handle.SetMaxHealthUnits(15);
            }
        }

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

        const float acceleration = 12;
        const float friction = 8;
        const float maxSpeed = 5f;

        if (direction != Vector2.Zero)
        {
            currentState = args.actionHandler.TryUpdateAction(
                ref playerState.CurrentAction,
                ref playerState.RemainingTime,
                PlayerAction.Move,
                out bool success
            );

            velocity.Direction = direction;
            velocity.Speed = success
                ? velocity.Speed += acceleration * dt
                : 0f;
        }
        else
        {
            float effectiveFriction = MathF.Max(0f, 1f - (friction * dt));
            velocity.Speed *= effectiveFriction;
            if (velocity.Speed < 0.001f)
            {
                velocity.Speed = 0f;
            }

            currentState = args.actionHandler.TryUpdateAction(
                ref playerState.CurrentAction,
                ref playerState.RemainingTime,
                PlayerAction.Idle,
                out bool _
            );
        }

        if (velocity.Speed > maxSpeed)
        {
            velocity.Speed = maxSpeed;
        }


        ref EntityStats entityStats = ref handle.Get<EntityStats>();
        entityStats.AttackCooldown -= args.dt;

        if (mouseState.IsButtonPressed(Controls.PlayerShoot))
        {
            if (entityStats.AttackCooldown > 0f)
            {
                return;
            }

            AnimationClip clip = args.assets.Get(GameAssets.Player.Attack.East);
            currentState = args.actionHandler.TryUpdateAction(
                ref playerState.CurrentAction,
                ref playerState.RemainingTime,
                PlayerAction.Shoot,
                clip.AnimationDuration(),
                out bool success
            );

            if (success)
            {
                Vector2 mousePositionWorld =
                    Projection.ScreenToWorldCoordinates(mouseState.Position, camera2D.Viewport,
                        camera2D.ViewProjection);

                Vector2 bulletDirection =
                    -Vector2.NormalizeFast(playerPosition.ToWorldPosition() -
                                           new Vector2(mousePositionWorld.X, mousePositionWorld.Y));

                PrefabFactory.CreateBullet(this.world, playerPosition,
                    new Velocity(bulletDirection, 7.5f), (float)MathHelper.Atan2(bulletDirection.Y, bulletDirection.X),
                    2,
                    args.assets);

                entityStats.AttackCooldown = entityStats.MaxAttackCooldown;


#if DEBUG
                Console.WriteLine(
                    $"Clicked at {mousePositionWorld} (Global Coords)");
#endif
            }
        }

        alignedCharacter.ActionIndex = (byte)currentState;
        args.actionHandler.Sync(
            ref playerState.CurrentAction,
            ref playerState.RemainingTime,
            dt);
    }
}