using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.UI;

namespace unnamed.Rendering.RenderContext;

public interface IDrawBuilder : ISpriteStep,
    IColorWithoutTextureStep, IColorWithTextureStep,
    IProjectionStep,
    IVerticesRelativeStep, IVerticesAbsoluteStep, IDrawStep;

public interface ISpriteStep
{
    IColorWithTextureStep WithSprite(in StaticSprite texture);
    IColorWithTextureStep WithText(in StaticTextTexture text);
    IColorWithoutTextureStep WithoutSprite();
}

public interface IColorStep
{
    IProjectionStep WithColoration(in Color4 color, float blendFactor);
    IProjectionStep WithColoration(in Vector3 color, float blendFactor);
    IProjectionStep WithColoration(in Color4? color, float blendFactor);
}

public interface IColorWithTextureStep : IColorStep
{
    IProjectionStep WithoutColoration();
    IProjectionStep WithAlpha(float alpha);
}

public interface IColorWithoutTextureStep : IColorStep;

public interface IProjectionStep
{
    IVerticesRelativeStep WithModelViewProjection(ref Matrix4 modelViewProjection);
    IVerticesRelativeStep WithPosition(in Vector2 position);

    IVerticesRelativeStep WithPosition(in float x, in float y);
    IVerticesRelativeStep WithPositionAndDistortion(in Vector2 position, in Matrix4 distortionMatrix);
    IVerticesRelativeStep WithPositionAndDistortion(in float x, in float y, in Matrix4 distortionMatrix);

    IVerticesRelativeStep WithPositionAndTransform(in Vector2 position, in Transform transform);

    IVerticesAbsoluteStep WithAbsoluteUiTransform(
        in AbsolutePosition position,
        in AbsoluteSize size,
        in UiPivot pivot);

    IVerticesAbsoluteStep WithReferenceUiTransform(
        in UiReferenceOffset referenceOffset,
        in UiReferenceSize referenceSize,
        in UiAnchor anchor,
        in UiPivot pivot,
        UiScaleMode scaleMode);
}

public interface IVerticesStep
{
    IDrawStep WithVertices(in float[] vertices);
}

public interface IVerticesRelativeStep
{
    IDrawStep WithSize(in Vector2 size, bool horizontallyCentered, bool verticallyCentered);
}

public interface IVerticesAbsoluteStep : IVerticesStep
{
    IDrawStep WithUiUnitQuad();
}

public interface IDrawStep
{
    void Draw();
}