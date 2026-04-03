using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.UI;

namespace unnamed.Rendering.RenderContext;

public interface IDrawBuilder : ISpriteStep,
    IColorWithoutTextureStep, IColorWithTextureStep,
    IProjectionStep,
    IVerticesStep, IDrawStep;

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
    IVerticesStep WithModelViewProjection(ref Matrix4 modelViewProjection);
    IVerticesStep WithPosition(in Vector2 position, Vector2 size, UiPivot pivot);

    IVerticesStep WithPosition(in float x, in float y, Vector2 size, UiPivot pivot);
    IVerticesStep WithPositionAndDistortion(in Vector2 position, in Matrix4 distortionMatrix, Vector2 size, UiPivot pivot);
    IVerticesStep WithPositionAndDistortion(in float x, in float y, in Matrix4 distortionMatrix, Vector2 size, UiPivot pivot);

    IVerticesStep WithPositionAndTransform(in Vector2 position, in Transform transform, Vector2 size, UiPivot pivot);

    IVerticesStep WithAbsoluteUiTransform(
        in AbsolutePosition position,
        in AbsoluteSize size,
        in UiPivot pivot);

    IVerticesStep WithReferenceUiTransform(
        in UiReferenceOffset referenceOffset,
        in UiReferenceSize referenceSize,
        in UiAnchor anchor,
        in UiPivot pivot,
        UiScaleMode scaleMode);
}

public interface IVerticesStep
{
    IDrawStep WithVertices(in float[] vertices);
    IDrawStep WithUnitQuad();
}

// public interface IVerticesRelativeStep
// {
//     IDrawStep WithSize(in Vector2 size, UiPivot pivot);
// }

// public interface IVerticesStep
// {
// }

public interface IDrawStep
{
    void Draw();
}