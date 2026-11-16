using Godot;
using System;

// Shared journal and story-gate popup, styled like the Party and Victory UI.
// Uses heading/paragraph hierarchy instead of Godot's default AcceptDialog.
public partial class StoryNoticeUI : CanvasLayer
{
    private static StoryNoticeUI _active;
    private SceneTree _tree;
    private bool _wasPaused;
    private bool _journal;
    private int _requestedLevel;

    public static void ShowJournal(Node caller) => Open(caller, true, 0);
    public static void ShowProgress(Node caller, int level) => Open(caller, false, level);

    private static void Open(Node caller, bool journal, int level)
    {
        if (GodotObject.IsInstanceValid(_active))
            return;

        SceneTree tree = caller.GetTree();
        var notice = new StoryNoticeUI
        {
            Name = "StoryNoticeUI",
            Layer = 135,
            ProcessMode = ProcessModeEnum.Always,
            _tree = tree,
            _wasPaused = tree.Paused,
            _journal = journal,
            _requestedLevel = level
        };
        _active = notice;
        tree.Root.AddChild(notice);
        tree.Paused = true;
    }

    public override void _Ready()
    {
        Font pixel = GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf");
        Vector2 screen = GetViewport().GetVisibleRect().Size;

        var surface = new Control { Name = "StoryNoticeSurface", MouseFilter = Control.MouseFilterEnum.Stop };
        AddChild(surface);
        surface.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var dimmer = new ColorRect
        {
            Name = "DimmedWorld",
            Color = new Color(0.015f, 0.025f, 0.045f, 0.83f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        surface.AddChild(dimmer);
        dimmer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var center = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        surface.AddChild(center);
        center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var card = new PanelContainer
        {
            Name = _journal ? "FieldNotesCard" : "ProgressCard",
            CustomMinimumSize = new Vector2(Mathf.Min(_journal ? 716 : 654, screen.X - 32), 0),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        card.AddThemeStyleboxOverride("panel", Box("192838", "f0cf85", 5));
        center.AddChild(card);

        var padding = new MarginContainer();
        SetPadding(padding, 27, 26);
        card.AddChild(padding);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 15);
        padding.AddChild(stack);

        var headingRow = new HBoxContainer();
        headingRow.AddThemeConstantOverride("separation", 16);
        stack.AddChild(headingRow);

        var headingColumn = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        headingColumn.AddThemeConstantOverride("separation", 7);
        headingRow.AddChild(headingColumn);
        headingColumn.AddChild(Text(_journal ? "WAWA  /  ADVENTURE JOURNAL" : "WAWA  /  STORY STATUS",
            10, "86c4ba", pixel));
        headingColumn.AddChild(Text(_journal ? "FIELD NOTES" : "YOKAI WAVE PROGRESS",
            _journal ? 23 : 18, "ffdf9b", pixel));

        var close = Button("X", 38, pixel);
        close.CustomMinimumSize = new Vector2(40, 38);
        close.Pressed += Close;
        headingRow.AddChild(close);

        var separator = new ColorRect
        {
            Color = new Color("619c91"),
            CustomMinimumSize = new Vector2(0, 4),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        stack.AddChild(separator);

        AddStoryProgress(stack, pixel);

        if (_journal)
            AddJournalContent(stack, pixel);
        else
            AddGateContent(stack, pixel);

        var footer = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        stack.AddChild(footer);
        var done = Button("BACK TO WAWA", 12, pixel);
        done.CustomMinimumSize = new Vector2(268, 46);
        done.Pressed += Close;
        footer.AddChild(done);

        card.Modulate = new Color(1, 1, 1, 0);
        CreateTween().TweenProperty(card, "modulate", Colors.White, 0.18);
    }

    private static void AddStoryProgress(VBoxContainer parent, Font pixel)
    {
        int done = GameProgress.CompletedLevels;
        var row = new HBoxContainer();
        parent.AddChild(row);
        var label = Text("STORY PROGRESS", 12, "dfe9e5", pixel);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(label);
        row.AddChild(Text($"{done} / {GameProgress.TotalLevels} CLEARED", 11, "ffdf9b", pixel));

        var progress = new ProgressBar
        {
            MaxValue = GameProgress.TotalLevels,
            Value = done,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 18),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        progress.AddThemeStyleboxOverride("background", Box("0d1726", "536174", 2));
        progress.AddThemeStyleboxOverride("fill", Box("609e91", "609e91", 0));
        parent.AddChild(progress);
    }

    private static void AddJournalContent(VBoxContainer stack, Font pixel)
    {
        bool complete = GameProgress.CompletedLevels >= GameProgress.TotalLevels;
        string destination = complete ? "ALL DISTRICTS CLEARED" :
            $"LEVEL {GameProgress.CompletedLevels + 1}  /  {GameProgress.NextBuilding.ToUpperInvariant()}";

        AddSection(stack, "CURRENT OBJECTIVE", destination,
            complete
                ? "You defeated every Yokai threatening Wawa. The town is safe... for now."
                : $"Head to {GameProgress.NextBuilding} and win the Yokai battle to advance.",
            pixel);

        if (!complete)
            AddSection(stack, "FIND YOUR WAY", null,
                "Follow the white flag on the map. It marks your next battle location.", pixel);

        int level = CandyInventory.FriendshipLevel(0);
        int remaining = Math.Max(0, CandyInventory.FeedsPerLevel - CandyInventory.FeedCount(0));
        AddSection(stack, level < 1 ? "FRIENDSHIP QUEST  !" : "FRIENDSHIP QUEST  COMPLETE",
            null,
            level < 1
                ? $"Feed Whisper {remaining} more candy to reach Friendship Level 1. Open PARTY [I] to feed a Yokai, and explore Wawa to find more candy."
                : $"Whisper reached Friendship Level {level}. Keep feeding Yokai to strengthen them for battle.",
            pixel);
    }

    private void AddGateContent(VBoxContainer stack, Font pixel)
    {
        int completed = GameProgress.CompletedLevels;
        bool allDone = completed >= GameProgress.TotalLevels;
        bool alreadyCleared = _requestedLevel <= completed;
        string status = allDone ? "ADVENTURE COMPLETE" :
            alreadyCleared ? "LOCATION CLEARED" : "LOCATION LOCKED";
        string location = $"LEVEL {_requestedLevel}  /  {GameProgress.BuildingName(_requestedLevel).ToUpperInvariant()}";

        AddSection(stack, status, location,
            allDone
                ? "Every Yokai battle is complete. Wawa is safe... for now."
                : alreadyCleared
                    ? "You've already defeated the Yokai here. Cleared locations can't be replayed."
                    : "This location is not available yet. Finish the battles in story order.",
            pixel);

        if (!allDone)
            AddSection(stack, "NEXT STEP", null,
                $"Go to {GameProgress.NextBuilding} (Level {completed + 1}) and win the battle. Follow the white flag to find it.",
                pixel);
    }

    private static void AddSection(VBoxContainer stack, string heading, string subtitle,
        string paragraph, Font pixel)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", Box("243648", "536d74", 2));
        stack.AddChild(panel);

        var margin = new MarginContainer();
        SetPadding(margin, 15, 13);
        panel.AddChild(margin);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 8);
        margin.AddChild(column);

        column.AddChild(Text(heading, 12, "f2d18e", pixel));
        if (!string.IsNullOrEmpty(subtitle))
            column.AddChild(Text(subtitle, 13, "99cfc4", pixel));
        column.AddChild(Text(paragraph, 17, "e4e9eb", null, wrap: true));
    }

    private static Label Text(string value, int size, string color, Font font, bool wrap = false)
    {
        var label = new Label
        {
            Text = value,
            AutowrapMode = wrap ? TextServer.AutowrapMode.WordSmart : TextServer.AutowrapMode.Off,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", new Color(color));
        if (font != null)
            label.AddThemeFontOverride("font", font);
        return label;
    }

    private static Button Button(string title, int size, Font pixel)
    {
        var button = new Button { Text = title, FocusMode = Control.FocusModeEnum.None };
        button.AddThemeStyleboxOverride("normal", Box("2c4b53", "d9c080", 3));
        button.AddThemeStyleboxOverride("hover", Box("426d6c", "fff0b8", 3));
        button.AddThemeStyleboxOverride("pressed", Box("1d343c", "ffda8b", 3));
        button.AddThemeColorOverride("font_color", new Color("fff0d6"));
        button.AddThemeFontSizeOverride("font_size", size);
        if (pixel != null)
            button.AddThemeFontOverride("font", pixel);
        return button;
    }

    private static StyleBoxFlat Box(string fill, string border, int thickness) => new()
    {
        BgColor = new Color(fill),
        BorderColor = new Color(border),
        BorderWidthLeft = thickness,
        BorderWidthRight = thickness,
        BorderWidthTop = thickness,
        BorderWidthBottom = thickness
    };

    private static void SetPadding(MarginContainer container, int horizontal, int vertical)
    {
        container.AddThemeConstantOverride("margin_left", horizontal);
        container.AddThemeConstantOverride("margin_right", horizontal);
        container.AddThemeConstantOverride("margin_top", vertical);
        container.AddThemeConstantOverride("margin_bottom", vertical);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo &&
            key.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
    }

    private void Close()
    {
        if (_tree != null)
            _tree.Paused = _wasPaused;
        if (ReferenceEquals(_active, this))
            _active = null;
        QueueFree();
    }

    public override void _ExitTree()
    {
        if (ReferenceEquals(_active, this))
        {
            _active = null;
            if (_tree != null)
                _tree.Paused = _wasPaused;
        }
    }
}
