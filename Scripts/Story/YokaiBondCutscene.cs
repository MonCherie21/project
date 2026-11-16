using Godot;
using System;

// Appears only after a first-time story victory grants a new companion.
// Candy is a meaningful optional gift; a warm welcome costs nothing.
public partial class YokaiBondCutscene : CanvasLayer
{
    private static readonly string[] FirstWords =
    {
        "",
        "Tattletell has finally stopped trembling. For the first time, someone listened without judging.",
        "Cheeksqueek peeks out from hiding. It wants to know whether the kindness it felt was real.",
        "Draggie's embers settle into a gentle glow. Under the fierce scales is a spirit looking for home.",
        "Enma bows its head. The weight of its crown seems lighter when someone offers friendship.",
        "Wobblewok's storm quiets. Its wavering shape becomes steady when you offer your hand.",
        "Orcanos lowers its guard. Even the mightiest Yokai can choose a different path."
    };

    private int _index;
    private Action _finished;
    private bool _chosen;
    private TextureRect _yokai;
    private TextureRect _trainer;
    private Label _story;
    private Button _gift;
    private Button _welcome;
    private Button _continue;

    public static void Show(Node parent, int index, Action onFinished)
    {
        if (index < 1 || index >= CandyInventory.PartnerCount)
        {
            onFinished?.Invoke();
            return;
        }

        var scene = new YokaiBondCutscene
        {
            Name = "NewYokaiBond",
            Layer = 180,
            ProcessMode = ProcessModeEnum.Always,
            _index = index,
            _finished = onFinished
        };
        parent.AddChild(scene);
    }

    public override void _Ready()
    {
        var font = GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf");
        var shade = new ColorRect
        {
            Color = new Color(0.025f, 0.045f, 0.036f, 0.94f),
            Size = new Vector2(1280, 720),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        AddChild(shade);

        var card = new Panel
        {
            Position = new Vector2(190, 120),
            Size = new Vector2(900, 480)
        };
        card.AddThemeStyleboxOverride("panel", YokaiTheme.Panel(5));
        shade.AddChild(card);

        var header = Text("A NEW FRIENDSHIP BLOOMS", font, 18, YokaiTheme.BrassBright,
            new Vector2(41, 30), new Vector2(818, 40));
        header.HorizontalAlignment = HorizontalAlignment.Center;
        card.AddChild(header);
        var sub = Text($"STAGE {_index + 1}  /  {YokaiRoster.Name(_index)} JOINS YOUR PARTY",
            font, 11, YokaiTheme.MossBright,
            new Vector2(35, 79), new Vector2(830, 32));
        sub.HorizontalAlignment = HorizontalAlignment.Center;
        card.AddChild(sub);

        _trainer = Portrait(PlayerData.Gender == "Male"
            ? "res://Sprites/P2_Whole.png" : "res://Sprites/P1_Whole.png",
            new Vector2(36, 130), new Vector2(207, 235));
        card.AddChild(_trainer);

        _yokai = Portrait(YokaiRoster.BattleSprite(_index),
            new Vector2(652, 122), new Vector2(208, 243));
        card.AddChild(_yokai);
        card.AddChild(new YokaiRuneSeal
        {
            Position = new Vector2(427, 115),
            Size = new Vector2(44, 44)
        });

        _story = Text(FirstWords[_index], null, 18, YokaiTheme.Parchment,
            new Vector2(250, 170), new Vector2(402, 147));
        _story.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _story.HorizontalAlignment = HorizontalAlignment.Center;
        _story.VerticalAlignment = VerticalAlignment.Center;
        card.AddChild(_story);

        _gift = Button("SHARE A CANDY", font,
            new Vector2(85, 386), new Vector2(340, 56));
        _gift.Disabled = CandyInventory.Candies < 1;
        _gift.TooltipText = "A real candy gift raises this Yokai's friendship";
        _gift.Pressed += () => MakePromise(true);
        card.AddChild(_gift);

        _welcome = Button("WELCOME TO OUR FAMILY", font,
            new Vector2(476, 386), new Vector2(340, 56));
        _welcome.Pressed += () => MakePromise(false);
        card.AddChild(_welcome);

        _continue = Button("RETURN TO YOUR JOURNEY", font,
            new Vector2(247, 386), new Vector2(406, 56));
        _continue.Visible = false;
        _continue.Pressed += () =>
        {
            if (!_chosen) return;
            Action next = _finished;
            QueueFree();
            next?.Invoke();
        };
        card.AddChild(_continue);

        card.Modulate = new Color(1, 1, 1, 0);
        var entrance = CreateTween().SetTrans(Tween.TransitionType.Sine);
        entrance.TweenProperty(card, "modulate:a", 1f, 0.65);
        entrance.Parallel().TweenProperty(_yokai, "position",
            new Vector2(626, 122), 0.65);
    }

    private void MakePromise(bool candy)
    {
        if (_chosen) return;
        if (candy && !CandyInventory.TryFeed(_index)) return;
        _chosen = true;
        _gift.Hide();
        _welcome.Hide();
        _continue.Show();
        string name = YokaiRoster.Name(_index);
        _story.Text = candy
            ? $"You share a candy with {name}. It takes the gift carefully, then reaches for your hand. Friendship is stronger than fear."
            : $"You kneel and gently embrace {name}. For a moment Wawa is quiet. A new friend knows they will never have to face the darkness alone.";

        var friendship = CreateTween().SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        friendship.TweenProperty(_trainer, "position",
            new Vector2(117, 130), 0.45);
        friendship.Parallel().TweenProperty(_yokai, "position",
            new Vector2(564, 122), 0.45);
        friendship.Parallel().TweenProperty(_yokai, "modulate",
            new Color(0.77f, 1f, 0.81f), 0.35);
        friendship.TweenProperty(_yokai, "modulate", Colors.White, 0.35);
    }

    private static TextureRect Portrait(string path, Vector2 position, Vector2 size) => new()
    {
        Texture = GD.Load<Texture2D>(path),
        Position = position,
        Size = size,
        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        MouseFilter = Control.MouseFilterEnum.Ignore
    };

    private static Label Text(string value, Font font, int size, Color color,
        Vector2 position, Vector2 dimensions)
    {
        var label = new Label
        {
            Text = value, Position = position, Size = dimensions,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        if (font != null)
            label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private static Button Button(string text, Font font, Vector2 position, Vector2 size)
    {
        var button = new Button
        {
            Text = text, Position = position, Size = size,
            FocusMode = Control.FocusModeEnum.None
        };
        YokaiTheme.StyleButton(button, important: true);
        button.AddThemeFontOverride("font", font);
        button.AddThemeFontSizeOverride("font_size", 10);
        return button;
    }
}
