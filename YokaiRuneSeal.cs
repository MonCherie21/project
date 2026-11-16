using Godot;

// Small carved charm emblem used across Wawa's interface. It is geometry,
// not a generic icon asset or emoji, so it stays crisp in pixel-art menus.
public partial class YokaiRuneSeal : Control
{
    public Color Glow { get; set; } = YokaiTheme.MossBright;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += QueueRedraw;
    }

    public override void _Draw()
    {
        Vector2 c = Size / 2;
        DrawRect(new Rect2(c - new Vector2(21, 21), new Vector2(42, 42)), YokaiTheme.Shadow);
        DrawRect(new Rect2(c - new Vector2(19, 19), new Vector2(38, 38)), YokaiTheme.Brass);
        DrawRect(new Rect2(c - new Vector2(16, 16), new Vector2(32, 32)), YokaiTheme.Inset);
        Vector2 t = c + new Vector2(0, -15);
        Vector2 r = c + new Vector2(13, 0);
        Vector2 b = c + new Vector2(0, 15);
        Vector2 l = c + new Vector2(-13, 0);
        DrawLine(t, r, YokaiTheme.Brass, 3, false);
        DrawLine(r, b, YokaiTheme.Brass, 3, false);
        DrawLine(b, l, YokaiTheme.Brass, 3, false);
        DrawLine(l, t, YokaiTheme.Brass, 3, false);
        DrawRect(new Rect2(c - new Vector2(3, 9), new Vector2(6, 18)), Glow);
        DrawRect(new Rect2(c - new Vector2(9, 3), new Vector2(18, 6)), Glow);
        DrawRect(new Rect2(c - new Vector2(3, 3), new Vector2(6, 6)), YokaiTheme.Parchment);
    }
}
