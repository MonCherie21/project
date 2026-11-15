using Godot;

// Drop this scene anywhere under MapBackground and duplicate with Ctrl+D.
// Leave PickupId blank to use its unique scene-tree path as the save ID.
// Give a fixed PickupId if you plan to rename/reparent it after publishing.
public partial class CandyPickup : Area2D
{
    [Export] public string PickupId = "";
    [Export(PropertyHint.Range, "1,20,1")] public int Amount = 1;

    private string _saveId = "";

    public override void _Ready()
    {
        _saveId = string.IsNullOrWhiteSpace(PickupId)
            ? GetPath().ToString() : PickupId.Trim();
        if (CandyInventory.IsPickedUp(_saveId))
        {
            Hide();
            SetDeferred("monitoring", false);
            return;
        }

        var icon = GetNodeOrNull<Sprite2D>("Icon");
        if (icon != null)
            icon.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player || !CandyInventory.Collect(_saveId, Amount))
            return;

        Hide();
        SetDeferred("monitoring", false);

        // A short world-space toast lets players know they found hidden candy.
        if (GetParent() is not Node2D parent)
            return;
        var text = new Label
        {
            Text = "+ " + Amount + " CANDY!",
            Position = Position + new Vector2(-28, -48),
            ZIndex = 100,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        text.AddThemeColorOverride("font_color", new Color("ffdb88"));
        text.AddThemeColorOverride("font_outline_color", Colors.Black);
        text.AddThemeConstantOverride("outline_size", 4);
        parent.AddChild(text);
        var tween = parent.CreateTween();
        tween.TweenProperty(text, "position:y", text.Position.Y - 22, 0.9);
        tween.Parallel().TweenProperty(text, "modulate:a", 0.0, 0.9);
        tween.Finished += text.QueueFree;
    }
}
