using Godot;
using System;

// Custom, pixel-themed battle result. No native AcceptDialog popup.
public partial class BattleResultUI : CanvasLayer
{
    private Label _heading;
    private Label _message;
    private Label _progress;
    private Button _continueButton;
    private Action _onContinue;

    public override void _Ready()
    {
        Font pixelFont = GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf");

        var dim = new ColorRect
        {
            Name = "ScreenDimmer",
            Position = Vector2.Zero,
            Size = new Vector2(1280, 720),
            Color = new Color(0.035f, 0.067f, 0.10f, 0.79f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        AddChild(dim);

        var card = new Panel
        {
            Name = "PixelVictoryCard",
            Position = new Vector2(302, 205),
            Size = new Vector2(676, 320)
        };
        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("192838"),
            BorderColor = new Color("f0cf85"),
            BorderWidthLeft = 5,
            BorderWidthTop = 5,
            BorderWidthRight = 5,
            BorderWidthBottom = 5,
            ShadowColor = new Color(0, 0, 0, 0.65f),
            ShadowSize = 9
        });
        AddChild(card);

        var upperAccent = new ColorRect
        {
            Position = new Vector2(20, 20),
            Size = new Vector2(636, 5),
            Color = new Color("60998d"),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        card.AddChild(upperAccent);

        _heading = new Label
        {
            Position = new Vector2(30, 44),
            Size = new Vector2(616, 40),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "VICTORY!",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _heading.AddThemeFontOverride("font", pixelFont);
        _heading.AddThemeFontSizeOverride("font_size", 25);
        _heading.AddThemeColorOverride("font_color", new Color("ffe1a0"));
        card.AddChild(_heading);

        _progress = new Label
        {
            Position = new Vector2(30, 104),
            Size = new Vector2(616, 25),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _progress.AddThemeFontOverride("font", pixelFont);
        _progress.AddThemeFontSizeOverride("font_size", 11);
        _progress.AddThemeColorOverride("font_color", new Color("8cc8bf"));
        card.AddChild(_progress);

        _message = new Label
        {
            Position = new Vector2(58, 143),
            Size = new Vector2(560, 76),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _message.AddThemeFontOverride("font", pixelFont);
        _message.AddThemeFontSizeOverride("font_size", 11);
        _message.AddThemeColorOverride("font_color", new Color("f3eed9"));
        card.AddChild(_message);

        _continueButton = new Button
        {
            Name = "ContinueButton",
            Text = "RETURN TO WAWA",
            Position = new Vector2(164, 240),
            Size = new Vector2(348, 53),
            FocusMode = Control.FocusModeEnum.None
        };
        _continueButton.AddThemeStyleboxOverride("normal", ButtonStyle("345e55", "e3c47e"));
        _continueButton.AddThemeStyleboxOverride("hover", ButtonStyle("518379", "fff0af"));
        _continueButton.AddThemeStyleboxOverride("pressed", ButtonStyle("263b41", "f8dea5"));
        _continueButton.AddThemeFontOverride("font", pixelFont);
        _continueButton.AddThemeFontSizeOverride("font_size", 12);
        _continueButton.AddThemeColorOverride("font_color", new Color("fff1d7"));
        _continueButton.Pressed += () => _onContinue?.Invoke();
        card.AddChild(_continueButton);

        // A restrained slide-in helps this feel like a victory screen rather
        // than the default editor alert.
        card.Position += new Vector2(0, 24);
        Tween entrance = CreateTween().SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        entrance.TweenProperty(card, "position", new Vector2(302, 205), 0.25);
    }

    public void Configure(string title, string message, string buttonLabel,
        int level, Action onContinue)
    {
        _onContinue = onContinue;
        _heading.Text = title;
        _message.Text = message;
        _continueButton.Text = buttonLabel;
        _progress.Text = title == "DEFEAT"
            ? "THE YOKAI ARE STILL WAITING..."
            : $"STAGE {level} / {GameProgress.TotalLevels} CLEARED";
        _heading.AddThemeColorOverride("font_color", title == "DEFEAT"
            ? new Color("ff9b89") : new Color("ffe1a0"));
    }

    private static StyleBoxFlat ButtonStyle(string background, string outline) => new()
    {
        BgColor = new Color(background),
        BorderColor = new Color(outline),
        BorderWidthLeft = 3,
        BorderWidthTop = 3,
        BorderWidthRight = 3,
        BorderWidthBottom = 3
    };
}
