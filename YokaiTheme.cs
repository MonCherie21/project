using Godot;

// Wawa's shared visual language: soot-black stone, tarnished brass, faded
// parchment and eerie moss-green. Keep these square and pixel-art friendly.
public static class YokaiTheme
{
    public static readonly Color Shadow = new("090e10");
    public static readonly Color Surface = new("121b1b");
    public static readonly Color Raised = new("1d2b29");
    public static readonly Color Inset = new("172320");
    public static readonly Color Brass = new("ad9160");
    public static readonly Color BrassBright = new("e0be7a");
    public static readonly Color Moss = new("628c78");
    public static readonly Color MossBright = new("92b7a0");
    public static readonly Color Parchment = new("e4ddc9");
    public static readonly Color Muted = new("b8bdac");
    public static readonly Color Rust = new("ae685c");
    public static readonly Color FrameEdge = new("53645a");

    public static StyleBoxFlat Box(Color fill, Color edge, int width = 3) => new()
    {
        BgColor = fill,
        BorderColor = edge,
        BorderWidthLeft = width,
        BorderWidthTop = width,
        BorderWidthRight = width,
        BorderWidthBottom = width
    };

    public static StyleBoxFlat Panel(int width = 4) => new()
    {
        BgColor = Surface,
        BorderColor = Brass,
        BorderWidthLeft = width,
        BorderWidthTop = width,
        BorderWidthRight = width,
        BorderWidthBottom = width,
        ShadowColor = new Color(0, 0, 0, 0.68f),
        ShadowSize = 8
    };

    public static void StyleButton(Button button, bool important = false)
    {
        button.AddThemeStyleboxOverride("normal",
            Box(important ? new Color("2b4438") : Raised, Brass, 3));
        button.AddThemeStyleboxOverride("hover",
            Box(new Color("3b5b48"), BrassBright, 3));
        button.AddThemeStyleboxOverride("pressed",
            Box(new Color("172923"), MossBright, 3));
        button.AddThemeStyleboxOverride("disabled",
            Box(new Color("18211f"), FrameEdge, 3));
        button.AddThemeStyleboxOverride("focus", Box(Raised, BrassBright, 2));
        button.AddThemeColorOverride("font_color", Parchment);
        button.AddThemeColorOverride("font_hover_color", BrassBright);
        button.AddThemeColorOverride("font_pressed_color", Parchment);
        button.AddThemeColorOverride("font_disabled_color", Muted);
    }

    // A text 'X' looks off-center in the pixel font. Draw a symmetric mark
    // instead; it is centered on the actual button rectangle at every size.
    public static void StyleClose(Button button)
    {
        button.Text = "";
        button.TooltipText = "Close";
        button.CustomMinimumSize = new Vector2(44, 44);
        button.AddThemeStyleboxOverride("normal", Box(new Color("202622"), Brass, 3));
        button.AddThemeStyleboxOverride("hover", Box(new Color("59332d"), BrassBright, 3));
        button.AddThemeStyleboxOverride("pressed", Box(new Color("321f1e"), Rust, 3));
        button.AddThemeStyleboxOverride("focus", Box(new Color("202622"), Brass, 3));
        var icon = new PixelCloseIcon { MouseFilter = Control.MouseFilterEnum.Ignore };
        button.AddChild(icon);
        icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }
}
