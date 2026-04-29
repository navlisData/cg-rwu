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
using unnamed.Resources;
using unnamed.Texture;
using unnamed.Utils;
using unnamed.Utils.Health;

namespace unnamed.systems;

public sealed class PlayerInputSystem(Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : BaseSystem
{
    private static readonly Query PlayerQuery = new QueryBuilder().With<Player>().Build();

    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    public override void Run(World world)
    {
        if (!PlayerQuery.TrySingle(world, out Entity e))
        {
            return;
        }

        EntityHandle player = world.Handle(e);
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        ref Camera2D camera2D = ref world.GetResource<Camera2D>();
        ref WindowSize windowSize = ref world.GetResource<WindowSize>();
        ref AssetStore assetStore = ref world.GetResource<AssetStore>();
        ref ActionControlHandler<PlayerAction> actionHandler =
            ref world.GetResource<ActionControlHandler<PlayerAction>>();

        KeyboardState keyboardState = this.keyboardStateProvider();
        MouseState mouseState = this.mouseStateProvider();

        ref Velocity velocity = ref player.Get<Velocity>();
        ref Position playerPosition = ref player.Get<Position>();
        ref PlayerActionState playerState = ref player.Get<PlayerActionState>();
        ref AlignedCharacter alignedCharacter = ref player.Get<AlignedCharacter>();

        alignedCharacter.CharacterDirection =
            mouseState.Get8WayDirectionFromPosition(windowSize, alignedCharacter.CharacterDirection);

#if DEBUG
        if (keyboardState.IsKeyPressed(Keys.D1))
        {
            player.AddDamage(2);
        }

        if (keyboardState.IsKeyPressed(Keys.D2))
        {
            player.AddHealth(2);
        }

        if (keyboardState.IsKeyPressed(Keys.D3))
        {
            player.SetMaxHealthUnits(15);
        }
#endif

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
            currentState = actionHandler.TryUpdateAction(
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

            currentState = actionHandler.TryUpdateAction(
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


        ref EntityStats entityStats = ref player.Get<EntityStats>();
        entityStats.AttackCooldown -= dt;

        if (mouseState.IsButtonPressed(Controls.PlayerShoot))
        {
            if (entityStats.AttackCooldown > 0f)
            {
                return;
            }

            AnimationClip clip = assetStore.Get(GameAssets.Player.Attack.East);
            currentState = actionHandler.TryUpdateAction(
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

                PrefabFactory.CreateBullet(world, playerPosition,
                    new Velocity(bulletDirection, 7.5f), (float)MathHelper.Atan2(bulletDirection.Y, bulletDirection.X),
                    2);

                entityStats.AttackCooldown = entityStats.MaxAttackCooldown;

#if DEBUG
                Console.WriteLine(
                    $"Clicked at {mousePositionWorld} (Global Coords)");
#endif
            }
        }

        alignedCharacter.ActionIndex = (byte)currentState;
        actionHandler.Sync(
            ref playerState.CurrentAction,
            ref playerState.RemainingTime,
            dt);
    }
}