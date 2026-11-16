using Godot;

// The 16-bit X is geometry, not a font glyph, so its two diagonals are
// optically and mathematically centered in every menu's exit button.
public partial class PixelCloseIcon : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += QueueRedraw;
    }

    public override void _Draw()
    {
        Vector2 center = Size / 2f;
        Vector2 topLeft = center + new Vector2(-10, -10);
        Vector2 topRight = center + new Vector2(10, -10);
        Vector2 bottomLeft = center + new Vector2(-10, 10);
        Vector2 bottomRight = center + new Vector2(10, 10);

        DrawLine(topLeft, bottomRight, YokaiTheme.Shadow, 10, false);
        DrawLine(topRight, bottomLeft, YokaiTheme.Shadow, 10, false);
        DrawLine(topLeft, bottomRight, YokaiTheme.Parchment, 5, false);
        DrawLine(topRight, bottomLeft, YokaiTheme.Parchment, 5, false);
    }
}
