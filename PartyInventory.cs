using Godot;
using System;

// Retro-styled, non-destructive party screen. The world stays instantiated
// underneath it, and feeding affects battles through CandyInventory.
public partial class PartyInventory : CanvasLayer
{
    private static PartyInventory _active;
    private static readonly string[] Portraits =
    {
        "res://Characters/one.png", "res://Characters/two.png",
        "res://Characters/three.png", "res://Characters/four.png",
        "res://Characters/five.png", "res://Characters/six.jpg",
        "res://Characters/seven.jpg"
    };

    private SceneTree _tree;
    private bool _wasPaused;
    private GridContainer _grid;
    private Label _candyLabel;

    public static bool IsOpen => GodotObject.IsInstanceValid(_active);

    public static void Open(Node caller)
    {
        if (IsOpen) return;
        PackedScene scene = GD.Load<PackedScene>("res://PartyInventory.tscn");
        if (scene == null)
        {
            GD.PushError("Could not load the party inventory.");
            return;
        }
        PartyInventory inventory = scene.Instantiate<PartyInventory>();
        inventory._tree = caller.GetTree();
        inventory._wasPaused = inventory._tree.Paused;
        _active = inventory;
        inventory._tree.Root.AddChild(inventory);
        inventory._tree.Paused = true;
    }

    public override void _Ready()
    {
        _grid = GetNode<GridContainer>("Panel/Scroll/PartyGrid");
        _candyLabel = GetNode<Label>("Panel/CandyCount");
        GetNode<Button>("Panel/CloseButton").Pressed += Close;
        CandyInventory.Changed += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        CandyInventory.Changed -= Refresh;
        if (ReferenceEquals(_active, this))
            _active = null;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo)
            return;
        if (key.Keycode != Key.Escape && key.Keycode != Key.I)
            return;
        Close();
        GetViewport().SetInputAsHandled();
    }

    private void Close()
    {
        if (_tree != null)
            _tree.Paused = _wasPaused;
        if (ReferenceEquals(_active, this))
            _active = null;
        QueueFree();
    }

    private void Refresh()
    {
        if (_grid == null)
            return;
        _candyLabel.Text = $"CANDY x{CandyInventory.Candies:00}";
        foreach (Node existing in _grid.GetChildren())
        {
            _grid.RemoveChild(existing);
            existing.QueueFree();
        }
        for (int i = 0; i < CandyInventory.AvailablePartners; i++)
            _grid.AddChild(BuildCard(i));
    }

    private static StyleBoxFlat Box(Color fill, Color border)
    {
        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            BorderWidthLeft = 3,
            BorderWidthTop = 3,
            BorderWidthRight = 3,
            BorderWidthBottom = 3
        };
        style.SetCornerRadiusAll(3);
        return style;
    }

    private static Label Text(string value, int fontSize, Color color)
    {
        var label = new Label { Text = value };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private PanelContainer BuildCard(int index)
    {
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(482, 145),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        card.AddThemeStyleboxOverride("panel", Box(new Color("1c293a"), new Color("8191a0")));

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        card.AddChild(margin);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        margin.AddChild(row);

        var portrait = new TextureRect
        {
            Texture = GD.Load<Texture2D>(Portraits[index]),
            CustomMinimumSize = new Vector2(110, 112),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        row.AddChild(portrait);

        var column = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", 5);
        row.AddChild(column);
        column.AddChild(Text(CandyInventory.Name(index), 20, new Color("ffde9e")));
        int level = CandyInventory.FriendshipLevel(index);
        column.AddChild(Text($"FRIENDSHIP LVL {level}/{CandyInventory.MaxFriendshipLevel}",
            16, Colors.White));

        var bar = new ProgressBar
        {
            MaxValue = 100,
            Value = CandyInventory.FriendshipProgress(index),
            CustomMinimumSize = new Vector2(250, 20),
            ShowPercentage = false
        };
        bar.AddThemeStyleboxOverride("background",
            Box(new Color("101a28"), new Color("4b566b")));
        bar.AddThemeStyleboxOverride("fill",
            Box(new Color("78b78c"), new Color("78b78c")));
        column.AddChild(bar);

        var feed = new Button
        {
            Text = level >= CandyInventory.MaxFriendshipLevel ? "MAX FRIENDSHIP" : "FEED 1 CANDY",
            CustomMinimumSize = new Vector2(220, 35),
            Disabled = CandyInventory.Candies == 0 ||
                level >= CandyInventory.MaxFriendshipLevel
        };
        feed.AddThemeStyleboxOverride("normal",
            Box(new Color("374c3a"), new Color("b5c49a")));
        feed.AddThemeStyleboxOverride("hover",
            Box(new Color("506e50"), new Color("ffdf9a")));
        feed.AddThemeFontSizeOverride("font_size", 17);
        feed.Pressed += () =>
        {
            if (!CandyInventory.TryFeed(index))
                return;
            // The save data and battle difficulty now read this same level.
            if (CandyInventory.FriendshipProgress(index) == 0 &&
                CandyInventory.FriendshipLevel(index) > 0)
                GD.Print(CandyInventory.Name(index) + " friendship increased to " +
                         CandyInventory.FriendshipLevel(index));
        };
        column.AddChild(feed);
        return card;
    }
}
