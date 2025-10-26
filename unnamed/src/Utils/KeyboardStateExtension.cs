using OpenTK.Windowing.GraphicsLibraryFramework;

namespace unnamed.Utils;

public static class KeyboardStateExtensions
{
    /// <summary>
    /// Computes an 8-way <see cref="CharacterDirection"/> from the current
    /// <see cref="OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState"/> using the
    /// configured movement keys (e.g., Controls.MoveUp/Right/Down/Left).
    /// Opposing inputs are neutralized per axis; when no effective input remains,
    /// the method returns <see cref="CharacterDirection.Down"/>.
    /// </summary>
    /// <param name="keyboardState">
    /// The keyboard snapshot to evaluate for directional input.
    /// </param>
    /// <param name="currentDirection">
    /// The current direction of the character.
    /// </param>
    /// <returns>
    /// The resolved 8-way character direction (cardinal or diagonal). If no keys are pressed,
    /// currentDirection is returned.
    /// </returns>
    /// <remarks>
    /// Axis semantics: positive X = right, positive Y = down.
    /// Opposite keys on the same axis cancel each other (e.g., Left + Right -> 0 on X).
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override void OnUpdateFrame(FrameEventArgs args)
    /// {
    ///     var direction = KeyboardState.GetDirection(CharacterDirection.Down);
    ///     // Apply movement or animation based on 'direction'
    /// }
    /// </code>
    /// </example>
    public static CharacterDirection Get8WayDirectionFromControls(this KeyboardState keyboardState, CharacterDirection currentDirection)
    {
        bool isUpPressed = keyboardState.IsKeyDown(Controls.MoveUp);
        bool isRightPressed = keyboardState.IsKeyDown(Controls.MoveRight);
        bool isDownPressed = keyboardState.IsKeyDown(Controls.MoveDown);
        bool isLeftPressed = keyboardState.IsKeyDown(Controls.MoveLeft);

        // Axes: positive X = right, positive Y = down
        int horizontalAxis = (isRightPressed ? 1 : 0) - (isLeftPressed ? 1 : 0);
        int verticalAxis = (isDownPressed  ? 1 : 0) - (isUpPressed   ? 1 : 0);

        // Neutral: no directional input
        if (horizontalAxis == 0 && verticalAxis == 0)
            return currentDirection;

        // Diagonals
        if (verticalAxis < 0 && horizontalAxis > 0) return CharacterDirection.UpRight;
        if (verticalAxis < 0 && horizontalAxis < 0) return CharacterDirection.UpLeft;
        if (verticalAxis > 0 && horizontalAxis > 0) return CharacterDirection.DownRight;
        if (verticalAxis > 0 && horizontalAxis < 0) return CharacterDirection.DownLeft;

        // Cardinals
        if (verticalAxis < 0) return CharacterDirection.Up;
        if (verticalAxis > 0) return CharacterDirection.Down;
        if (horizontalAxis > 0) return CharacterDirection.Right;
        return CharacterDirection.Left;
    }
}