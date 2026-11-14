using System;
using Godot;

// One-time instructional cards shared by the world introduction and first battle.
// The game pauses under the card, but the card itself can still receive input.
public static class TutorialOverlay
{
    public static void Show(Node caller, string title, string instructions)
        => ShowCard(caller, title, instructions, null, "GOT IT!");

    // Uses the same dark card as the in-game tutorials, with a deliberate
    // Yes/No choice for account creation and irreversible slot removal.
    public static void Confirm(Node caller, string title, string message,
        Action onConfirmed, string confirmText = "YES, CONTINUE")
        => ShowCard(caller, title, message, onConfirmed, confirmText);

    private static void ShowCard(Node caller, string title, string instructions,
        Action onConfirmed, string confirmText)
    {
        SceneTree tree = caller.GetTree();
        bool wasPaused = tree.Paused;
        var layer = new CanvasLayer { Layer = 130, ProcessMode = Node.ProcessModeEnum.Always };
        tree.Root.AddChild(layer);

        Vector2 screen = caller.GetViewport().GetVisibleRect().Size;
        var shade = new ColorRect
        {
            Size = screen,
            Color = new Color(0.025f, 0.03f, 0.055f, 0.90f),
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
            BgColor = new Color("151c2b"),
            BorderColor = new Color("68788d"),
            BorderWidthLeft = 4,
            BorderWidthRight = 4,
            BorderWidthTop = 4,
            BorderWidthBottom = 4
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
        heading.AddThemeColorOverride("font_color", new Color("ead49a"));
        column.AddChild(heading);

        var body = new Label
        {
            Text = instructions,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        body.AddThemeFontSizeOverride("font_size", 17);
        body.AddThemeColorOverride("font_color", new Color("d6dbe5"));
        column.AddChild(body);

        var actions = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        actions.AddThemeConstantOverride("separation", 20);
        column.AddChild(actions);

        var button = new Button
        {
            Text = confirmText,
            CustomMinimumSize = new Vector2(onConfirmed == null ? 210 : 195, 42),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        var buttonNormal = new StyleBoxFlat
        {
            BgColor = new Color("293345"),
            BorderColor = new Color("8292a6"),
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2
        };
        var buttonHover = (StyleBoxFlat)buttonNormal.Duplicate();
        buttonHover.BgColor = new Color("3b4c62");
        button.AddThemeStyleboxOverride("normal", buttonNormal);
        button.AddThemeStyleboxOverride("hover", buttonHover);
        button.AddThemeStyleboxOverride("focus", buttonHover);
        button.AddThemeColorOverride("font_color", new Color("f4f1e8"));
        button.AddThemeColorOverride("font_hover_color", new Color("ffdfa0"));
        button.Pressed += () =>
        {
            tree.Paused = wasPaused;
            layer.QueueFree();
            onConfirmed?.Invoke();
        };
        actions.AddChild(button);

        if (onConfirmed != null)
        {
            var cancel = new Button
            {
                Text = "CANCEL",
                CustomMinimumSize = new Vector2(160, 42)
            };
            cancel.AddThemeStyleboxOverride("normal", (StyleBoxFlat)buttonNormal.Duplicate());
            cancel.AddThemeStyleboxOverride("hover", (StyleBoxFlat)buttonHover.Duplicate());
            cancel.AddThemeColorOverride("font_color", new Color("d6dbe5"));
            cancel.Pressed += () => { tree.Paused = wasPaused; layer.QueueFree(); };
            actions.AddChild(cancel);
        }
        tree.Paused = true;
        button.GrabFocus();
    }
}
