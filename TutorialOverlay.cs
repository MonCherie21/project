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
            Color = new Color(0.012f, 0.020f, 0.017f, 0.93f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        layer.AddChild(shade);

        var panel = new PanelContainer
        {
            Position = new Vector2(Mathf.Max(8, (screen.X - 620) / 2),
                                   Mathf.Max(8, (screen.Y - 300) / 2)),
            Size = new Vector2(Mathf.Min(620, screen.X - 16), 300)
        };
        panel.AddThemeStyleboxOverride("panel", YokaiTheme.Panel());
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
        heading.AddThemeColorOverride("font_color", YokaiTheme.BrassBright);
        column.AddChild(heading);

        var body = new Label
        {
            Text = instructions,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        body.AddThemeFontSizeOverride("font_size", 17);
        body.AddThemeColorOverride("font_color", YokaiTheme.Parchment);
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
        YokaiTheme.StyleButton(button, important: true);
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
            YokaiTheme.StyleButton(cancel);
            cancel.Pressed += () => { tree.Paused = wasPaused; layer.QueueFree(); };
            actions.AddChild(cancel);
        }
        tree.Paused = true;
        button.GrabFocus();
    }
}
