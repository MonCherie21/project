using Godot;

// One-time instructional cards shared by the world introduction and first battle.
// The game pauses under the card, but the card itself can still receive input.
public static class TutorialOverlay
{
    public static void Show(Node caller, string title, string instructions)
    {
        SceneTree tree = caller.GetTree();
        bool wasPaused = tree.Paused;
        var layer = new CanvasLayer { Layer = 130, ProcessMode = Node.ProcessModeEnum.Always };
        tree.Root.AddChild(layer);

        Vector2 screen = caller.GetViewport().GetVisibleRect().Size;
        var shade = new ColorRect
        {
            Size = screen,
            Color = new Color(0.03f, 0.07f, 0.10f, 0.82f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        layer.AddChild(shade);

        var panel = new PanelContainer
        {
            Position = new Vector2(Mathf.Max(8, (screen.X - 620) / 2),
                                   Mathf.Max(8, (screen.Y - 300) / 2)),
            Size = new Vector2(Mathf.Min(620, screen.X - 16), 300)
        };
        var style = new StyleBoxFlat
        {
            BgColor = new Color("294c55"),
            BorderColor = new Color("ebd49a"),
            BorderWidthLeft = 5,
            BorderWidthRight = 5,
            BorderWidthTop = 5,
            BorderWidthBottom = 5
        };
        panel.AddThemeStyleboxOverride("panel", style);
        shade.AddChild(panel);

        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        foreach (string side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride("margin_" + side, 20);
        panel.AddChild(margin);

        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 18);
        margin.AddChild(column);

        var heading = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
        heading.AddThemeFontSizeOverride("font_size", 25);
        heading.AddThemeColorOverride("font_color", new Color("ffdf76"));
        column.AddChild(heading);

        var body = new Label
        {
            Text = instructions,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        body.AddThemeFontSizeOverride("font_size", 17);
        body.AddThemeColorOverride("font_color", new Color("f7f0db"));
        column.AddChild(body);

        var button = new Button
        {
            Text = "GOT IT!",
            CustomMinimumSize = new Vector2(210, 42),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        button.Pressed += () =>
        {
            tree.Paused = wasPaused;
            layer.QueueFree();
        };
        column.AddChild(button);
        tree.Paused = true;
        button.GrabFocus();
    }
}
