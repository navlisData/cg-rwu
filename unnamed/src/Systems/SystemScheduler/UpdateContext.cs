using unnamed.Components.Rendering;

namespace unnamed.Systems.SystemScheduler;

public readonly record struct UpdateContext(float dt, Camera2D Camera);