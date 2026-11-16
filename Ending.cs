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
            Color = new Color(0.035f, 0.10f, 0.085f, 0.72f),
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
        _endingTitle.AddThemeColorOverride("font_color", YokaiTheme.BrassBright);
        _endingTitle.AddThemeColorOverride("font_outline_color", YokaiTheme.Shadow);
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
        chapter.AddThemeColorOverride("font_color", YokaiTheme.MossBright);
        AddChild(chapter);

        var storyCard = new Panel
        {
            Name = "EpilogueDialogue",
            Position = new Vector2(172, 285),
            Size = new Vector2(936, 300)
        };
        storyCard.AddThemeStyleboxOverride("panel", YokaiTheme.Panel(5));
        AddChild(storyCard);

        storyCard.AddChild(new YokaiRuneSeal
        {
            Position = new Vector2(841, 22),
            Size = new Vector2(44, 44)
        });

        _speaker = new Label
        {
            Position = new Vector2(42, 32),
            Size = new Vector2(820, 28),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _speaker.AddThemeFontOverride("font", font);
        _speaker.AddThemeFontSizeOverride("font_size", 14);
        _speaker.AddThemeColorOverride("font_color", YokaiTheme.BrassBright);
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
        _dialogueText.AddThemeColorOverride("font_color", YokaiTheme.Parchment);
        storyCard.AddChild(_dialogueText);

        _pageCounter = new Label
        {
            Position = new Vector2(42, 249),
            Size = new Vector2(180, 23),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _pageCounter.AddThemeFontOverride("font", font);
        _pageCounter.AddThemeFontSizeOverride("font_size", 11);
        _pageCounter.AddThemeColorOverride("font_color", YokaiTheme.MossBright);
        storyCard.AddChild(_pageCounter);

        _nextButton = new Button
        {
            Position = new Vector2(564, 225),
            Size = new Vector2(322, 54),
            FocusMode = FocusModeEnum.None
        };
        _nextButton.AddThemeFontOverride("font", font);
        _nextButton.AddThemeFontSizeOverride("font_size", 12);
        YokaiTheme.StyleButton(_nextButton, important: true);
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

}
