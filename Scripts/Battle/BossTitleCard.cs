using Godot;

// A short, skippable retro title reveal for the seventh and final district.
public partial class BossTitleCard : CanvasLayer
{
    private bool _closing;

    public static void Show(Node parent)
    {
        var card = new BossTitleCard
        {
            Name = "FinalBossReveal",
            Layer = 190,
            ProcessMode = ProcessModeEnum.Always
        };
        parent.AddChild(card);
    }

    public override void _Ready()
    {
        Font font = GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf");
        var backdrop = new ColorRect
        {
            Color = new Color(0.025f, 0.011f, 0.018f, 0.98f),
            Size = new Vector2(1280, 720),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        AddChild(backdrop);

        var upper = Label("FINAL DISTRICT  /  SEVENTH SEAL",
            font, 16, YokaiTheme.Rust, new Vector2(170, 105), new Vector2(940, 42));
        upper.HorizontalAlignment = HorizontalAlignment.Center;
        backdrop.AddChild(upper);

        var boss = new TextureRect
        {
            Texture = GD.Load<Texture2D>(YokaiRoster.BattleSprite(6)),
            Position = new Vector2(470, 184),
            Size = new Vector2(340, 295),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Modulate = new Color(1f, 0.63f, 0.63f, 0)
        };
        backdrop.AddChild(boss);

        var title = Label("ORCANOS", font, 39, YokaiTheme.BrassBright,
            new Vector2(180, 477), new Vector2(920, 68));
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_outline_color", Colors.Black);
        title.AddThemeConstantOverride("outline_size", 8);
        title.Modulate = new Color(1, 1, 1, 0);
        backdrop.AddChild(title);

        var subtitle = Label("THE LAST CURSE OF WAWA", font, 12, YokaiTheme.Parchment,
            new Vector2(250, 552), new Vector2(780, 33));
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        subtitle.Modulate = new Color(1, 1, 1, 0);
        backdrop.AddChild(subtitle);

        var begin = new Button
        {
            Text = "FACE THE DARKNESS",
            Position = new Vector2(462, 624),
            Size = new Vector2(356, 52),
            FocusMode = Control.FocusModeEnum.None
        };
        YokaiTheme.StyleButton(begin, important: true);
        begin.AddThemeFontOverride("font", font);
        begin.AddThemeFontSizeOverride("font_size", 11);
        begin.Pressed += Close;
        backdrop.AddChild(begin);

        var reveal = CreateTween().SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        reveal.TweenProperty(boss, "modulate:a", 1f, 0.85);
        reveal.Parallel().TweenProperty(boss, "position",
            new Vector2(470, 168), 0.85);
        reveal.TweenProperty(title, "modulate:a", 1f, 0.45);
        reveal.Parallel().TweenProperty(subtitle, "modulate:a", 1f, 0.45);
        reveal.TweenInterval(3.0);
        reveal.TweenCallback(Callable.From(Close));
    }

    private static Label Label(string text, Font font, int size, Color color,
        Vector2 position, Vector2 dimensions)
    {
        var label = new Label
        {
            Text = text, Position = position, Size = dimensions,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private void Close()
    {
        if (_closing) return;
        _closing = true;
        var fade = CreateTween();
        fade.TweenProperty(GetChild<ColorRect>(0), "modulate:a", 0f, 0.27);
        fade.Finished += QueueFree;
    }
}
