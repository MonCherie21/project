using Godot;

// Wawa's chapter-one epilogue: a real scene, not the engine's alert dialog.
public partial class Ending : Control
{
    private readonly (string speaker, string speech)[] _dialogue =
    {
        ("NARRATOR", "The last Yokai fades into the night.\nOne by one, Wawa's lights begin to glow again."),
        ("THE PEOPLE OF WAWA", "The streets are peaceful once more.\nThank you for standing beside us."),
        ("NARRATOR", "Your courage - and the friends you made along the way -\nhave kept the darkness away."),
        ("NARRATOR", "Wawa is safe from the Yokai... for now.\nSomewhere beyond the town, an old mystery still waits.")
    };

    private Label _speaker;
    private Label _dialogueText;
    private Label _pageCounter;
    private Label _endingTitle;
    private Button _nextButton;
    private int _page;

    public override void _Ready()
    {
        Font font = GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf");

        var map = new TextureRect
        {
            Name = "WawaRestored",
            Texture = GD.Load<Texture2D>("res://Sprites/Bg_map.jpg"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            Position = Vector2.Zero,
            Size = new Vector2(1280, 720),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(map);
        AddChild(new ColorRect
        {
            Color = new Color(0.07f, 0.14f, 0.21f, 0.62f),
            Size = new Vector2(1280, 720),
            MouseFilter = MouseFilterEnum.Ignore
        });

        _endingTitle = new Label
        {
            Position = new Vector2(100, 104),
            Size = new Vector2(1080, 68),
            Text = "WAWA IS SAFE",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _endingTitle.AddThemeFontOverride("font", font);
        _endingTitle.AddThemeFontSizeOverride("font_size", 35);
        _endingTitle.AddThemeColorOverride("font_color", new Color("ffe0a2"));
        _endingTitle.AddThemeColorOverride("font_outline_color", new Color("1a2738"));
        _endingTitle.AddThemeConstantOverride("outline_size", 9);
        AddChild(_endingTitle);

        var chapter = new Label
        {
            Text = "YOKAI WAVE  -  CHAPTER COMPLETE",
            Position = new Vector2(100, 185),
            Size = new Vector2(1080, 32),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        chapter.AddThemeFontOverride("font", font);
        chapter.AddThemeFontSizeOverride("font_size", 13);
        chapter.AddThemeColorOverride("font_color", new Color("bde2d3"));
        AddChild(chapter);

        var storyCard = new Panel
        {
            Name = "EpilogueDialogue",
            Position = new Vector2(172, 285),
            Size = new Vector2(936, 300)
        };
        storyCard.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("1b2d3d"),
            BorderColor = new Color("e9c782"),
            BorderWidthLeft = 5,
            BorderWidthTop = 5,
            BorderWidthRight = 5,
            BorderWidthBottom = 5,
            ShadowColor = new Color(0, 0, 0, 0.55f),
            ShadowSize = 8
        });
        AddChild(storyCard);

        _speaker = new Label
        {
            Position = new Vector2(42, 32),
            Size = new Vector2(820, 28),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _speaker.AddThemeFontOverride("font", font);
        _speaker.AddThemeFontSizeOverride("font_size", 14);
        _speaker.AddThemeColorOverride("font_color", new Color("f0ca87"));
        storyCard.AddChild(_speaker);

        _dialogueText = new Label
        {
            Position = new Vector2(42, 83),
            Size = new Vector2(840, 92),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _dialogueText.AddThemeFontOverride("font", font);
        _dialogueText.AddThemeFontSizeOverride("font_size", 13);
        _dialogueText.AddThemeColorOverride("font_color", new Color("f6efe0"));
        storyCard.AddChild(_dialogueText);

        _pageCounter = new Label
        {
            Position = new Vector2(42, 249),
            Size = new Vector2(180, 23),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _pageCounter.AddThemeFontOverride("font", font);
        _pageCounter.AddThemeFontSizeOverride("font_size", 11);
        _pageCounter.AddThemeColorOverride("font_color", new Color("8bb4ae"));
        storyCard.AddChild(_pageCounter);

        _nextButton = new Button
        {
            Position = new Vector2(564, 225),
            Size = new Vector2(322, 54),
            FocusMode = FocusModeEnum.None
        };
        _nextButton.AddThemeFontOverride("font", font);
        _nextButton.AddThemeFontSizeOverride("font_size", 12);
        _nextButton.AddThemeColorOverride("font_color", Colors.White);
        _nextButton.AddThemeStyleboxOverride("normal", ButtonStyle("335e55", "e9c782"));
        _nextButton.AddThemeStyleboxOverride("hover", ButtonStyle("4d8376", "f7eba9"));
        _nextButton.AddThemeStyleboxOverride("pressed", ButtonStyle("253e43", "f7eba9"));
        _nextButton.Pressed += AdvanceDialogue;
        storyCard.AddChild(_nextButton);

        DisplayDialogue();
    }

    private void AdvanceDialogue()
    {
        if (_page == _dialogue.Length - 1)
        {
            // World._Ready saves the seventh victory, then the player can
            // continue exploring the restored town on the same save slot.
            PlayerData.HasLoadedPosition = true;
            GetTree().Paused = false;
            Error error = GetTree().ChangeSceneToFile("res://world.tscn");
            if (error != Error.Ok)
                GD.PushError("Could not return to the restored Wawa: " + error);
            return;
        }

        _page++;
        DisplayDialogue();
    }

    private void DisplayDialogue()
    {
        _speaker.Text = _dialogue[_page].speaker;
        _dialogueText.Text = _dialogue[_page].speech;
        _pageCounter.Text = $"{_page + 1} / {_dialogue.Length}";
        _nextButton.Text = _page == _dialogue.Length - 1
            ? "RETURN TO WAWA" : "CONTINUE  >";
        if (_page == _dialogue.Length - 1)
            _endingTitle.Text = "FOR NOW...";
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
