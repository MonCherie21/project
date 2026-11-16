using Godot;

// Shared confirm-to-battle logic: no repeated input callbacks or duplicate
// scene requests when Enter is held down.
public partial class DialogueAdvance : Node
{
    [Export(PropertyHint.File, "*.tscn")]
    public string NextScenePath { get; set; } = "res://Scenes/Battles/BattleScene.tscn";

    private bool _leaving;

    public override void _Ready() => GetTree().Paused = false;

    public override void _Input(InputEvent @event)
    {
        if (_leaving || @event is not InputEventKey key || !key.Pressed || key.Echo ||
            !@event.IsActionPressed("ui_accept"))
            return;

        _leaving = true;
        GetNode<SceneManager>("/root/SceneManager").TransitionTo(NextScenePath);
        GetViewport().SetInputAsHandled();
    }
}
