using Godot;

// Advance once on confirmation. Holding Enter must not restart the battle.
public partial class Dialogue1 : Node
{
    private bool _leaving;

    public override void _Ready() => GetTree().Paused = false;

    public override void _Input(InputEvent @event)
    {
        if (_leaving || @event is not InputEventKey key || !key.Pressed || key.Echo ||
            !@event.IsActionPressed("ui_accept"))
            return;

        _leaving = true;
        GetNode<SceneManager>("/root/SceneManager").TransitionTo("res://Scenes/Battles/BattleScene.tscn");
        GetViewport().SetInputAsHandled();
    }
}
