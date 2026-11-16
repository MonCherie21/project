using Godot;
using System;

// Chapter-zero cinematic: the family's exorcist promise, Whisper's first
// friendship, and the invasion that sends the player out to save Wawa.
// Built from existing game art so both trainer variants share one story.
public partial class OpeningCutscene : Control
{
    private enum Chapter { Family, Whisper, Bond, Invasion, Promise, Departure }

    private readonly struct Moment
    {
        public readonly string Speaker;
        public readonly string Line;
        public readonly Chapter Chapter;
        public readonly bool Feed;

        public Moment(string speaker, string line, Chapter chapter, bool feed = false)
        {
            Speaker = speaker;
            Line = line;
            Chapter = chapter;
            Feed = feed;
        }
    }

    private Moment[] _story;
    private Font _pixelFont;
    private TextureRect _house;
    private TextureRect _town;
    private TextureRect _candy;
    private ColorRect _sceneTint;
    private ColorRect _flash;
    private ColorRect _fade;
    private Sprite2D _trainer;
    private Sprite2D _whisper;
    private Sprite2D _shadowOne;
    private Sprite2D _shadowTwo;
    private Label _chapterLabel;
    private Label _chapterTitle;
    private Label _speaker;
    private Label _speech;
    private Label _pageLabel;
    private Label _status;
    private Button _continue;
    private Button _feed;
    private ColorRect[] _embers;
    private float[] _emberSpeeds;
    private readonly Random _random = new Random(2026);

    private int _page;
    private int _visibleCharacters;
    private double _typeTime;
    private bool _textComplete;
    private bool _feeding;
    private bool _leaving;
    private Chapter? _currentChapter;
    private Tween _entranceFade;

    public override void _Ready()
    {
        GetTree().Paused = false;
        _pixelFont = GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf");
        string name = string.IsNullOrWhiteSpace(PlayerData.PlayerName)
            ? "YOU" : PlayerData.PlayerName.Trim();

        _story = new[]
        {
            new Moment("NARRATOR",
                "Wawa. A town of familiar faces, busy streets, and quiet secrets. For years, its people have slept peacefully beneath the same stars.", Chapter.Family),
            new Moment("NARRATOR",
                "But your family knows what most cannot see: Yokai walk beside humans. Some are harmless. Others have forgotten how to find their way home.", Chapter.Family),
            new Moment("GRANDMOTHER'S WORDS",
                "Our family has been exorcists for generations. Remember, " + name + ": we protect people, but we do not condemn every spirit we meet.", Chapter.Family),
            new Moment("GRANDMOTHER'S WORDS",
                "A friendly Yokai is not a tool. It is a companion. Share your kindness, earn its trust, and your strength will grow together.", Chapter.Family),
            new Moment("NARRATOR",
                "Tonight, as you step outside your family's home, a little white spirit trembles beside the garden gate.", Chapter.Whisper),
            new Moment("???",
                "P-please don't banish me! I wasn't trying to scare anyone. I just needed somewhere safe to hide...", Chapter.Whisper),
            new Moment(name.ToUpperInvariant(),
                "Hey, it's okay. My family taught me to listen before judging a Yokai. I'm " + name + ". What's your name?", Chapter.Whisper),
            new Moment("WHISPER",
                "Whisper! I can hear what other spirits feel. And lately... Wawa's Yokai have been afraid of something. Even I don't know what.", Chapter.Whisper),
            new Moment("NARRATOR",
                "You find a wrapped candy in your pocket. Whisper looks at it with wide eyes. Perhaps every great friendship begins with one small kindness.", Chapter.Bond, feed: true),
            new Moment("WHISPER",
                "For me?! You're really sharing it? Hee-hee! I feel warmer already. I think... I think I'd like to be your friend!", Chapter.Bond),
            new Moment("GRANDMOTHER'S WORDS",
                "That is the first bond. Feed your Yokai, care for them, and stand by them. As friendship grows, so will their courage and power.", Chapter.Bond),
            new Moment("TOWN EMERGENCY RADIO",
                "WARNING! Yokai sightings are spreading across Wawa! Shadows near House 2, disturbances at the Sari-Sari Store... Citizens, stay indoors!", Chapter.Invasion),
            new Moment("NARRATOR",
                "The lights flicker. A crimson haze rolls over the roads. Yokai pour into the town, turning ordinary places into dangerous battlegrounds.", Chapter.Invasion),
            new Moment("WHISPER",
                "I can hear them crying out... Some are angry, but many are scared! Something is drawing them here. Wawa can't face this alone.", Chapter.Invasion),
            new Moment(name.ToUpperInvariant(),
                "Then we won't abandon them. My family has protected Wawa for generations. Now it's our turn—and we'll do it together.", Chapter.Promise),
            new Moment("WHISPER",
                "Together! We'll become stronger, make more Yokai friends, and bring peace back. I promise I'll be brave if you stay beside me!", Chapter.Promise),
            new Moment("NARRATOR",
                "Seven disturbances threaten Wawa. Visit each location in order, defeat the hostile Yokai, and restore the town one battle at a time.", Chapter.Departure),
            new Moment("NARRATOR",
                "Your first destination is House 2. Follow the white flag. Protect your neighbors. Keep the family promise. Wawa is counting on you.", Chapter.Departure)
        };

        BuildScene();
        ShowPage(0);
        _entranceFade = CreateTween();
        _entranceFade.TweenProperty(_fade, "color:a", 0f, 1.15)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
    }

    private void BuildScene()
    {
        _house = Backdrop("res://Sprites/loc/loc1_house.jpg");
        _town = Backdrop("res://Sprites/Bg_map.jpg");
        _town.Modulate = new Color(1, 1, 1, 0);

        _sceneTint = new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(1280, 720),
            Color = new Color(0.07f, 0.10f, 0.19f, 0.36f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_sceneTint);

        // Small drifting spirit motes: warm at first, then ominous red.
        _embers = new ColorRect[26];
        _emberSpeeds = new float[_embers.Length];
        for (int i = 0; i < _embers.Length; i++)
        {
            float size = i % 4 == 0 ? 6 : 3;
            _embers[i] = new ColorRect
            {
                Color = new Color(0.69f, 0.96f, 0.85f, 0.62f),
                Position = new Vector2(_random.Next(24, 1250), _random.Next(36, 550)),
                Size = new Vector2(size, size),
                MouseFilter = MouseFilterEnum.Ignore
            };
            _emberSpeeds[i] = 12 + i % 6 * 5;
            AddChild(_embers[i]);
        }

        _shadowOne = Sprite("res://Sprites/YokaiEnemy/M6_Front.png",
            new Vector2(1050, 305), new Vector2(1.50f, 1.50f));
        _shadowTwo = Sprite("res://Sprites/YokaiEnemy/M7_Front.png",
            new Vector2(655, 345), new Vector2(1.08f, 1.08f));
        _shadowOne.Modulate = new Color(0.50f, 0.10f, 0.19f, 0);
        _shadowTwo.Modulate = new Color(0.50f, 0.10f, 0.19f, 0);

        bool male = PlayerData.Gender == "Male";
        _trainer = Sprite(male ? "res://Sprites/P2_Whole.png" : "res://Sprites/P1_Whole.png",
            new Vector2(339, 348), male ? new Vector2(0.56f, 0.56f) : new Vector2(1.21f, 1.21f));
        _trainer.Modulate = new Color(1, 1, 1, 0);

        _whisper = Sprite("res://Sprites/M1_Front.png",
            new Vector2(882, 357), new Vector2(2.43f, 2.43f));
        _whisper.Modulate = new Color(0.82f, 1f, 0.95f, 0);

        _flash = new ColorRect
        {
            Size = new Vector2(1280, 720),
            Color = new Color(1f, 0.75f, 0.77f, 0),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_flash);

        _candy = new TextureRect
        {
            Texture = GD.Load<Texture2D>("res://Assets/CandyWrapperPixel.png"),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Position = new Vector2(720, 310),
            Size = new Vector2(144, 80),
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_candy);

        // Cinematic letterboxing and chapter label stay above the artwork.
        AddChild(new ColorRect
        {
            Color = new Color("0a1420"), Size = new Vector2(1280, 26),
            MouseFilter = MouseFilterEnum.Ignore
        });

        _chapterLabel = Label("PROLOGUE  /  A FAMILY PROMISE",
            new Vector2(76, 57), new Vector2(850, 26), 11, "b8ded2", true);
        _chapterTitle = Label("THE FIRST BOND",
            new Vector2(71, 105), new Vector2(1100, 59), 30, "ffe1a0", true);
        _chapterTitle.AddThemeColorOverride("font_outline_color", new Color("132336"));
        _chapterTitle.AddThemeConstantOverride("outline_size", 8);

        var skip = StyledButton("SKIP INTRO  >>", 12, "293a49", "c9a96e");
        skip.Position = new Vector2(1050, 38);
        skip.Size = new Vector2(184, 44);
        skip.Pressed += SkipIntro;
        AddChild(skip);

        var card = new Panel
        {
            Name = "OpeningDialogueCard",
            Position = new Vector2(128, 491),
            Size = new Vector2(1024, 204),
            MouseFilter = MouseFilterEnum.Stop
        };
        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("172738f2"),
            BorderColor = new Color("edd094"),
            BorderWidthLeft = 5,
            BorderWidthTop = 5,
            BorderWidthRight = 5,
            BorderWidthBottom = 5,
            ShadowSize = 12,
            ShadowColor = new Color(0, 0, 0, 0.6f)
        });
        AddChild(card);

        var accent = new ColorRect
        {
            Position = new Vector2(29, 16), Size = new Vector2(961, 4),
            Color = new Color("5b9489"), MouseFilter = MouseFilterEnum.Ignore
        };
        card.AddChild(accent);

        _speaker = Label("NARRATOR", new Vector2(35, 32), new Vector2(800, 24),
            13, "ffd897", true, card);
        _speech = Label("", new Vector2(35, 71), new Vector2(945, 74),
            17, "f4efe4", false, card);
        _speech.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _speech.VerticalAlignment = VerticalAlignment.Center;

        _pageLabel = Label("01 / 18", new Vector2(35, 167), new Vector2(180, 20),
            10, "8fbdb5", true, card);
        _status = Label("ENTER / SPACE TO CONTINUE", new Vector2(209, 166),
            new Vector2(430, 24), 10, "aac1b9", true, card);

        _continue = StyledButton("CONTINUE  >", 12, "315f58", "ead08e");
        _continue.Position = new Vector2(728, 150);
        _continue.Size = new Vector2(262, 44);
        _continue.Pressed += ContinueStory;
        card.AddChild(_continue);

        _feed = StyledButton("GIVE WHISPER CANDY", 11, "3d654c", "ffda8a");
        _feed.Position = new Vector2(661, 150);
        _feed.Size = new Vector2(329, 44);
        _feed.Icon = GD.Load<Texture2D>("res://Assets/CandyWrapperPixel.png");
        _feed.ExpandIcon = true;
        _feed.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        _feed.Pressed += FeedWhisper;
        _feed.Visible = false;
        card.AddChild(_feed);

        _fade = new ColorRect
        {
            Name = "CinematicFade",
            Color = new Color(0.015f, 0.025f, 0.04f, 1f),
            Size = new Vector2(1280, 720),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_fade);
    }

    private TextureRect Backdrop(string path)
    {
        var image = new TextureRect
        {
            Texture = GD.Load<Texture2D>(path),
            Position = new Vector2(-40, -22),
            Size = new Vector2(1360, 765),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(image);
        return image;
    }

    private Sprite2D Sprite(string path, Vector2 position, Vector2 scale)
    {
        var sprite = new Sprite2D
        {
            Texture = GD.Load<Texture2D>(path),
            Position = position,
            Scale = scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        AddChild(sprite);
        return sprite;
    }

    private Label Label(string text, Vector2 position, Vector2 size, int fontSize,
        string color, bool pixel, Node parent = null)
    {
        var label = new Label
        {
            Text = text, Position = position, Size = size,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", new Color(color));
        if (pixel)
            label.AddThemeFontOverride("font", _pixelFont);
        (parent ?? this).AddChild(label);
        return label;
    }

    private Button StyledButton(string title, int size, string fill, string border)
    {
        var button = new Button { Text = title, FocusMode = FocusModeEnum.None };
        button.AddThemeFontOverride("font", _pixelFont);
        button.AddThemeFontSizeOverride("font_size", size);
        button.AddThemeColorOverride("font_color", new Color("fff4dc"));
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        button.AddThemeStyleboxOverride("normal", ButtonStyle(fill, border));
        button.AddThemeStyleboxOverride("hover", ButtonStyle("4d7c73", "fff0bc"));
        button.AddThemeStyleboxOverride("pressed", ButtonStyle("203b40", "efd28e"));
        return button;
    }

    private static StyleBoxFlat ButtonStyle(string fill, string border) => new()
    {
        BgColor = new Color(fill), BorderColor = new Color(border),
        BorderWidthLeft = 3, BorderWidthTop = 3,
        BorderWidthRight = 3, BorderWidthBottom = 3
    };

    public override void _Process(double delta)
    {
        if (_leaving) return;

        if (!_textComplete && _story != null)
        {
            _typeTime += delta;
            int target = Math.Min(_story[_page].Line.Length, (int)(_typeTime * 48));
            if (target != _visibleCharacters)
            {
                _visibleCharacters = target;
                _speech.VisibleCharacters = target;
                if (target >= _story[_page].Line.Length)
                    FinishTyping();
            }
        }

        for (int i = 0; i < _embers.Length; i++)
        {
            Vector2 pos = _embers[i].Position;
            pos.Y -= _emberSpeeds[i] * (float)delta;
            pos.X += (float)Math.Sin((Time.GetTicksMsec() / 1000.0) + i) * 7f * (float)delta;
            if (pos.Y < 32)
                pos = new Vector2(_random.Next(24, 1250), 555);
            _embers[i].Position = pos;
        }

        if (_whisper.Modulate.A > 0.01f)
            _whisper.Offset = new Vector2(0, (float)Math.Sin(Time.GetTicksMsec() / 420.0) * 2.5f);
        if (_trainer.Modulate.A > 0.01f)
            _trainer.Offset = new Vector2(0, (float)Math.Sin(Time.GetTicksMsec() / 700.0) * 1.3f);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo || _leaving)
            return;
        if (key.Keycode is Key.Enter or Key.KpEnter or Key.Space)
        {
            ContinueStory();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ShowPage(int page)
    {
        _page = page;
        Moment moment = _story[page];
        SetChapter(moment.Chapter);
        if (page == 9)
            _chapterTitle.Text = "WHISPER JOINED YOU!";
        _speaker.Text = moment.Speaker;
        _speech.Text = moment.Line;
        _speech.VisibleCharacters = 0;
        _visibleCharacters = 0;
        _typeTime = 0;
        _textComplete = false;
        _pageLabel.Text = $"{page + 1:00} / {_story.Length:00}";
        _status.Text = moment.Feed ? "A FIRST ACT OF KINDNESS"
            : page == 9 ? $"CANDY x{CandyInventory.Candies:00}  /  BOND FORMED!"
            : "ENTER / SPACE TO CONTINUE";
        _continue.Visible = !moment.Feed;
        _continue.Text = page == _story.Length - 1 ? "ENTER WAWA  >" : "CONTINUE  >";
        _feed.Visible = moment.Feed;
        _feed.Disabled = true;
        _candy.Visible = moment.Feed;
        if (moment.Feed)
        {
            _candy.Position = new Vector2(720, 310);
            _candy.Modulate = Colors.White;
        }
    }

    private void FinishTyping()
    {
        _textComplete = true;
        _speech.VisibleCharacters = -1;
        if (_story[_page].Feed && !_feeding)
        {
            _feed.Disabled = false;
            _status.Text = "CHOOSE TO SHARE A CANDY";
        }
    }

    private void ContinueStory()
    {
        if (_leaving || _feeding) return;
        if (!_textComplete)
        {
            FinishTyping();
            return;
        }
        if (_story[_page].Feed)
        {
            FeedWhisper();
            return;
        }
        if (_page == _story.Length - 1)
        {
            EnterWawa();
            return;
        }
        ShowPage(_page + 1);
    }

    private void FeedWhisper()
    {
        if (_feeding || _leaving || !_story[_page].Feed || !_textComplete)
            return;

        // This is a real friendship feed, not a decorative cutscene choice.
        if (CandyInventory.FeedCount(0) == 0 && !CandyInventory.TryFeed(0))
        {
            _status.Text = "NO CANDY AVAILABLE";
            return;
        }

        _feeding = true;
        _feed.Disabled = true;
        _status.Text = "WHISPER'S TRUST GREW!";
        PlaySound("ChargeReady");

        var offer = CreateTween().SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        offer.TweenProperty(_candy, "position", new Vector2(841, 337), 0.55);
        offer.Parallel().TweenProperty(_candy, "modulate:a", 0f, 0.55);
        offer.Finished += () =>
        {
            if (!GodotObject.IsInstanceValid(this) || _leaving) return;
            Flash(new Color(0.55f, 1f, 0.82f, 0.70f));
            var glow = CreateTween().SetTrans(Tween.TransitionType.Sine);
            glow.TweenProperty(_whisper, "modulate", new Color(0.73f, 1f, 0.79f), 0.15);
            glow.TweenProperty(_whisper, "modulate", Colors.White, 0.38);
            glow.Finished += () =>
            {
                if (!GodotObject.IsInstanceValid(this) || _leaving) return;
                _feeding = false;
                ShowPage(_page + 1);
            };
        };
    }

    private void SetChapter(Chapter next)
    {
        if (_currentChapter == next) return;
        _currentChapter = next;
        switch (next)
        {
            case Chapter.Family:
                _chapterLabel.Text = "PROLOGUE  /  THE FAMILY PROMISE";
                _chapterTitle.Text = "THE FIRST BOND";
                _sceneTint.Color = new Color(0.06f, 0.11f, 0.19f, 0.34f);
                CreateTween().TweenProperty(_trainer, "modulate:a", 1f, 0.95);
                break;

            case Chapter.Whisper:
                _chapterLabel.Text = "PROLOGUE  /  A NEW FRIEND";
                _chapterTitle.Text = "MEET WHISPER";
                _whisper.Scale = new Vector2(1.65f, 1.65f);
                var appear = CreateTween().SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.Out);
                appear.TweenProperty(_whisper, "scale", new Vector2(2.43f, 2.43f), 0.85);
                appear.Parallel().TweenProperty(_whisper, "modulate:a", 1f, 0.85);
                PlaySound("ChargeReady", -19);
                break;

            case Chapter.Bond:
                _chapterLabel.Text = "PROLOGUE  /  THE FIRST FRIENDSHIP";
                _chapterTitle.Text = "KINDNESS IS POWER";
                _sceneTint.Color = new Color(0.05f, 0.19f, 0.14f, 0.27f);
                break;

            case Chapter.Invasion:
                _chapterLabel.Text = "CHAPTER ONE  /  THE WAWA INCIDENT";
                _chapterTitle.Text = "WAWA UNDER SIEGE";
                ShowInvadedTown();
                PlaySound("EnemyAttack", -7);
                var music = GetNodeOrNull<MusicPlayer>("/root/MusicPlayer");
                music?.PlayTrack(GD.Load<AudioStream>("res://Music/BattleTheme.ogg"));
                Flash(new Color(1f, 0.21f, 0.23f, 0.82f));
                break;

            case Chapter.Promise:
                _chapterLabel.Text = "CHAPTER ONE  /  AN EXORCIST'S VOW";
                _chapterTitle.Text = "WE FIGHT TOGETHER";
                CreateTween().TweenProperty(_sceneTint, "color",
                    new Color(0.10f, 0.17f, 0.25f, 0.50f), 0.8);
                break;

            case Chapter.Departure:
                _chapterLabel.Text = "CHAPTER ONE  /  YOUR ADVENTURE";
                _chapterTitle.Text = "SAVE WAWA";
                CreateTween().TweenProperty(_sceneTint, "color",
                    new Color(0.04f, 0.14f, 0.17f, 0.44f), 0.9);
                break;
        }

        bool invaded = next is Chapter.Invasion or Chapter.Promise or Chapter.Departure;
        Color emberColor = invaded ? new Color(1f, 0.48f, 0.39f, 0.60f)
                                   : new Color(0.69f, 0.96f, 0.85f, 0.62f);
        foreach (ColorRect ember in _embers)
            ember.Color = emberColor;
    }

    private void ShowInvadedTown()
    {
        var backdrop = CreateTween().SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        backdrop.TweenProperty(_town, "modulate:a", 1f, 0.95);
        backdrop.Parallel().TweenProperty(_house, "modulate:a", 0f, 0.95);
        backdrop.Parallel().TweenProperty(_sceneTint, "color",
            new Color(0.29f, 0.035f, 0.075f, 0.52f), 0.95);
        backdrop.Parallel().TweenProperty(_shadowOne, "modulate:a", 0.67f, 0.95);
        backdrop.Parallel().TweenProperty(_shadowTwo, "modulate:a", 0.48f, 0.95);
    }

    private void Flash(Color color)
    {
        _flash.Color = color;
        var flash = CreateTween().SetTrans(Tween.TransitionType.Sine);
        flash.TweenProperty(_flash, "color:a", 0f, 0.66);
    }

    private void PlaySound(string name, float volume = -12f)
    {
        AudioStream stream = GD.Load<AudioStream>($"res://Music/Sfx/{name}.wav");
        if (stream == null) return;
        var player = new AudioStreamPlayer { Stream = stream, VolumeDb = volume };
        AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }

    private void SkipIntro() => EnterWawa();

    private void EnterWawa()
    {
        if (_leaving) return;
        _leaving = true;
        _continue.Disabled = true;
        _feed.Disabled = true;
        if (GodotObject.IsInstanceValid(_entranceFade))
            _entranceFade.Kill();

        // Skipping still records the first act of friendship exactly once.
        if (CandyInventory.FeedCount(0) == 0)
            CandyInventory.TryFeed(0);

        var exit = CreateTween().SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        exit.TweenProperty(_fade, "color:a", 1f, 0.6);
        exit.Finished += () =>
        {
            if (!GodotObject.IsInstanceValid(this)) return;
            PlayerData.PendingMovementTutorial = true;
            GameProgress.QueueAutoSave();
            Error error = GetTree().ChangeSceneToFile("res://world.tscn");
            if (error != Error.Ok)
            {
                GD.PushError("Could not enter Wawa after the prologue: " + error);
                _leaving = false;
                _fade.Color = new Color(0.015f, 0.025f, 0.04f, 0);
                _continue.Disabled = false;
            }
        };
    }
}
