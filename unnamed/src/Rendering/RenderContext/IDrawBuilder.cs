using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.UI;

namespace unnamed.Rendering.RenderContext;

public interface IDrawBuilder : ISpriteStep,
    IColorWithoutTextureStep, IColorWithTextureStep,
    IProjectionUiStep, IProjectionGameStep,
    IDrawStep;

public interface IProjectionStep;

public interface IProjectionUiStep : IProjectionStep
{
    ISpriteStep WithReferencePosition(in UiReferenceOffset referenceOffset, in UiReferenceSize referenceSize,
        in Vector2 pivot, in UiAnchor anchor, UiScaleMode scaleMode);

    ISpriteStep WithAbsolutePosition(
        in AbsolutePosition position,
        in AbsoluteSize size,
        in Vector2 pivot);
}

public interface IProjectionGameStep : IProjectionStep
{
    ISpriteStep WithModelViewProjection(ref Matrix4 modelViewProjection);
    ISpriteStep WithPosition(in Vector2 position, Vector2 size, Vector2 pivot);

    ISpriteStep WithPosition(in float x, in float y, Vector2 size, Vector2 pivot);

    ISpriteStep WithPositionAndDistortion(in Vector2 position, in Matrix4 distortionMatrix, Vector2 size,
        Vector2 pivot);

    ISpriteStep WithPositionAndDistortion(in float x, in float y, in Matrix4 distortionMatrix, Vector2 size,
        Vector2 pivot);

    ISpriteStep WithPositionAndTransform(in Vector2 position, in Transform transform, Vector2 size, Vector2 pivot);
}

public interface ISpriteStep
{
    IColorWithTextureStep WithSprite(in StaticSprite texture);
    IColorWithTextureStep WithSprite(in StaticTextTexture text);
    IColorWithoutTextureStep WithoutSprite();
}

public interface IColorStep
{
    IDrawStep WithColoration(in Color4 color, float blendFactor);
    IDrawStep WithColoration(in Vector3 color, float blendFactor);
    IDrawStep WithColoration(in Color4? color, float blendFactor);
}

public interface IColorWithTextureStep : IColorStep
{
    IDrawStep WithoutColoration();
    IDrawStep WithAlpha(float alpha);
}

public interface IColorWithoutTextureStep : IColorStep;

public interface IDrawStep
{
    void Draw();
}