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
            Color = new Color(0.014f, 0.022f, 0.017f, 0.88f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        AddChild(dim);

        var card = new Panel
        {
            Name = "PixelVictoryCard",
            Position = new Vector2(302, 205),
            Size = new Vector2(676, 320)
        };
        card.AddThemeStyleboxOverride("panel", YokaiTheme.Panel(5));
        AddChild(card);

        var upperAccent = new ColorRect
        {
            Position = new Vector2(20, 20),
            Size = new Vector2(636, 5),
            Color = YokaiTheme.Moss,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        card.AddChild(upperAccent);

        card.AddChild(new YokaiRuneSeal
        {
            Position = new Vector2(599, 41),
            Size = new Vector2(44, 44)
        });

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
        _heading.AddThemeColorOverride("font_color", YokaiTheme.BrassBright);
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
        _progress.AddThemeColorOverride("font_color", YokaiTheme.MossBright);
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
        _message.AddThemeColorOverride("font_color", YokaiTheme.Parchment);
        card.AddChild(_message);

        _continueButton = new Button
        {
            Name = "ContinueButton",
            Text = "RETURN TO WAWA",
            Position = new Vector2(164, 240),
            Size = new Vector2(348, 53),
            FocusMode = Control.FocusModeEnum.None
        };
        YokaiTheme.StyleButton(_continueButton, important: true);
        _continueButton.AddThemeFontOverride("font", pixelFont);
        _continueButton.AddThemeFontSizeOverride("font_size", 12);
        _continueButton.AddThemeColorOverride("font_color", YokaiTheme.Parchment);
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
            ? YokaiTheme.Rust : YokaiTheme.BrassBright);
    }
}
