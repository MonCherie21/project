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
            Color = new Color(0.012f, 0.020f, 0.017f, 0.91f),
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
        card.AddThemeStyleboxOverride("panel", YokaiTheme.Panel(5));
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

        headingRow.AddChild(new YokaiRuneSeal
        {
            CustomMinimumSize = new Vector2(44, 44),
            MouseFilter = Control.MouseFilterEnum.Ignore
        });

        var headingColumn = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        headingColumn.AddThemeConstantOverride("separation", 7);
        headingRow.AddChild(headingColumn);
        headingColumn.AddChild(Text(_journal ? "WAWA  /  ADVENTURE JOURNAL" : "WAWA  /  STORY STATUS",
            10, "84a88d", pixel));
        headingColumn.AddChild(Text(_journal ? "FIELD NOTES" : "YOKAI WAVE PROGRESS",
            _journal ? 23 : 18, "e0be7a", pixel));

        var close = Button("", 12, pixel);
        YokaiTheme.StyleClose(close);
        close.Pressed += Close;
        headingRow.AddChild(close);

        var separator = new ColorRect
        {
            Color = YokaiTheme.Moss,
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
        var label = Text("STORY PROGRESS", 12, "d5d9c9", pixel);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(label);
        row.AddChild(Text($"{done} / {GameProgress.TotalLevels} CLEARED", 11, "e0be7a", pixel));

        var progress = new ProgressBar
        {
            MaxValue = GameProgress.TotalLevels,
            Value = done,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 18),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        progress.AddThemeStyleboxOverride("background", YokaiTheme.Box(YokaiTheme.Shadow, YokaiTheme.FrameEdge, 2));
        progress.AddThemeStyleboxOverride("fill", YokaiTheme.Box(YokaiTheme.Moss, YokaiTheme.Moss, 0));
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
        panel.AddThemeStyleboxOverride("panel", YokaiTheme.Box(YokaiTheme.Inset, YokaiTheme.FrameEdge, 2));
        stack.AddChild(panel);

        var margin = new MarginContainer();
        SetPadding(margin, 15, 13);
        panel.AddChild(margin);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 8);
        margin.AddChild(column);

        column.AddChild(Text(heading, 12, "cfb17b", pixel));
        if (!string.IsNullOrEmpty(subtitle))
            column.AddChild(Text(subtitle, 13, "83ae94", pixel));
        column.AddChild(Text(paragraph, 17, "dedacb", null, wrap: true));
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
        YokaiTheme.StyleButton(button, important: true);
        button.AddThemeFontSizeOverride("font_size", size);
        if (pixel != null)
            button.AddThemeFontOverride("font", pixel);
        return button;
    }

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
